/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Whisperer
{
    /// <summary>
    ///     Handles Unity scene loading/unloading, engine settings.
    /// </summary>
    public class LevelLoader : MonoBehaviour
    {
        public static LevelLoader Instance;

        [SerializeField] private float _fadeTimeDelay = 2;
        [SerializeField] private float _pauseLoadDelay = 2;
        [SerializeField] private CameraColorOverlay _overlay;
        [SerializeField] private BoundsDetector _boundsDetector;
        [SerializeField] private List<LevelScenes> _levels = new();
        [SerializeField] private bool _use90Hz = true;
        [SerializeField] private bool _forceStartLevel0;
        [SerializeField] private bool _bypassLogos;
        [SerializeField] private bool _dev;

        public string VERSION_NAME = "version";
        public string COMPLETED_NAME = "completed";

        /// <summary>
        ///     Called immediately when all scenes are loaded.
        /// </summary>
        [HideInInspector] public UnityEvent OnLevelLoadComplete;

        /// <summary>
        ///     Called just before scenes begin to unload.
        /// </summary>
        [HideInInspector] public UnityEvent<float> OnLevelWillBeginUnload;

        private Vector3 _gravity;

        private int _loadCount;

        private string _sceneWillBeActive;
        private bool _working;

        public bool IsLoading { get; set; }
        public bool IsUnloading { get; set; }
        public bool IsTransition => IsLoading || IsUnloading;
        public int NextIndex { get; private set; }

        private void Awake()
        {
            Instance = this;

            if (!Application.isEditor)
            {
                _forceStartLevel0 = false;
                _bypassLogos = false;
                _dev = false;
            }
        }

        private void Start()
        {
            if (_use90Hz) SetupXR();
            if (_dev) return;

            UnloadAllScenes();

            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.black;

            if (_bypassLogos)
                StartApp();
            else
                LoadLogos();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadLevel(1, false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadLevel(2, false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                LoadLevel(3, false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                LoadLevel(4, false);
            }
#endif
        }

        private void LoadLogos()
        {
            SceneManager.sceneLoaded += DisplayLogos;
            SceneManager.LoadScene("Logos", LoadSceneMode.Additive);
        }

        private void DisplayLogos(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= DisplayLogos;
            UXManager.Instance.OpenDisplay("Logos");
        }

        public void StartApp()
        {
            UnloadAllScenes();

            /// Version check
            Debug.Log("Application Version: " + Application.version);

            if (Application.version != PlayerPrefs.GetString(VERSION_NAME, ""))
            {
                Debug.Log("New version detected, clearing storage.");
                PlayerPrefs.DeleteAll();
            }

            PlayerPrefs.SetString(VERSION_NAME, Application.version);
            PlayerPrefs.Save();

            _boundsDetector.Active = true;
            _overlay.SetColor(Color.black);

            var startIndex = _forceStartLevel0 ? 0 : PlayerPrefs.GetInt(COMPLETED_NAME, 0) == 1 ? 4 : 0;
            LoadLevel(_levels[startIndex], false);
        }

        public void LoadLevel(LevelScenes _level, bool useDelay = true)
        {
            NextIndex = _levels.IndexOf(_level);
            StartCoroutine(ProcessLoad(_level, useDelay));
        }

        public void LoadLevel(int index, bool useDelay = true)
        {
            NextIndex = Mathf.Clamp(index, 0, _levels.Count - 1);
            StartCoroutine(ProcessLoad(_levels[NextIndex], useDelay));
        }

        public void RestartLevel()
        {
            LoadLevel(NextIndex);
        }

        [ContextMenu("Load Next Level")]
        public void LoadNextLevel()
        {
            NextIndex = (NextIndex + 1) % _levels.Count;
            LoadLevel(_levels[NextIndex]);
        }

        private IEnumerator ProcessLoad(LevelScenes _level, bool useDelay)
        {
            IsUnloading = true;
            OnLevelWillBeginUnload.Invoke(_fadeTimeDelay);
            yield return new WaitForSeconds(_fadeTimeDelay);

            Camera.main.clearFlags = CameraClearFlags.Skybox;

            /// Unload all scenes except the main app scene
            UnloadAllScenes();
            while (_working) yield return null;

            /// Unload unused assets from memory
            yield return UnloadUnusedAssets();
            IsUnloading = false;

            /// Pause
            yield return new WaitForSeconds(useDelay ? _pauseLoadDelay : 0);

            /// Load next level
            IsLoading = true;
            _gravity = Physics.gravity;
            Time.timeScale = 0;
            Physics.gravity = Vector3.zero;
            LoadScenes(_level);
            while (_working) yield return null;

            Time.timeScale = 1;
            Physics.gravity = _gravity;
            OnLevelLoadComplete.Invoke();

            yield return new WaitForSeconds(_fadeTimeDelay);
            FindObjectOfType<LevelManager>()?.SetLoaderReady();

            IsLoading = false;
        }

        private void SetupXR()
        {
            if (Performance.TryGetDisplayRefreshRate(out var rate))
            {
                var newRate = 90f;
                if (Performance.TryGetAvailableDisplayRefreshRates(out var rates)) newRate = rates.Max();
                if (rate < newRate)
                    if (Performance.TrySetDisplayRefreshRate(newRate))
                    {
                        Time.fixedDeltaTime = 1f / newRate;
                        Time.maximumDeltaTime = 1f / newRate;
                    }
            }
        }

        #region Unloading

        public void UnloadAllScenes()
        {
            _working = true;

            if (SceneManager.sceneCount > 1)
            {
                SceneManager.sceneUnloaded += OnSceneUnloadedCheck;

                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene != gameObject.scene) SceneManager.UnloadSceneAsync(scene);
                }
            }
            else
            {
                _working = false;
            }
        }

        private void OnSceneUnloadedCheck(Scene scene)
        {
            if (SceneManager.sceneCount == 2) /// just the loader + DDOL scenes remain
            {
                SceneManager.sceneUnloaded -= OnSceneUnloadedCheck;
                _working = false;
            }
        }

        private IEnumerator UnloadUnusedAssets()
        {
            var startTime = Time.realtimeSinceStartup;
            var async = Resources.UnloadUnusedAssets();

            while (!async.isDone) yield return null;
        }

        #endregion

        #region Loading

        public void LoadScenes(LevelScenes _level)
        {
            _working = true;
            _loadCount = 0;

            SceneManager.sceneLoaded += OnSceneLoaded;
            _level.Scenes.ForEach(scene =>
            {
                SceneManager.LoadScene(scene, LoadSceneMode.Additive);

                /// Last scene in list will be active scene
                if (_level.Scenes.IndexOf(scene) == _level.Scenes.Count - 1)
                    _sceneWillBeActive = scene;
            });
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _loadCount++;

            if (_loadCount == _levels[NextIndex].Scenes.Count)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneWillBeActive));
                _working = false;
            }
        }

        #endregion
    }

    [Serializable]
    public class LevelScenes
    {
        public string LevelName;
        public List<string> Scenes;
    }
}
