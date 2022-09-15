/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace Whisperer
{
	/// <summary>
	/// Handles Unity scene loading/unloading, engine settings.
	/// </summary>
	public class LevelLoader : MonoBehaviour
	{
		public static LevelLoader Instance;

		[SerializeField] float _fadeTimeDelay = 2;
		[SerializeField] float _pauseLoadDelay = 2;
		[SerializeField] CameraColorOverlay _overlay;
		[SerializeField] List<LevelScenes> _levels = new List<LevelScenes>();
		[SerializeField] bool _use90Hz = true;
		[SerializeField] bool _forceStartLevel0;
		[SerializeField] bool _bypassLogos;
		[SerializeField] bool _dev;

		int _levelIndex,
			_loadCount;
		bool _working;
		string _sceneWillBeActive;
		Vector3 _gravity;

		public string VERSION_NAME = "version";
		public string COMPLETED_NAME = "completed";

		public bool IsLoading { get; set; }
		public bool IsUnloading { get; set; }
		public bool IsTransition { get => IsLoading || IsUnloading; }
		public int NextIndex { get => _levelIndex; }

		/// <summary>
		/// Called immediately when all scenes are loaded.
		/// </summary>
		[HideInInspector] public UnityEvent OnLevelLoadComplete;
		/// <summary>
		/// Called just before scenes begin to unload.
		/// </summary>
		[HideInInspector] public UnityEvent<float> OnLevelWillBeginUnload;

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

			Debug.Log("Application Version: " + Application.version);

			if (Application.version != PlayerPrefs.GetString(VERSION_NAME, ""))
			{
				Debug.Log("New version detected, clearing storage.");
				PlayerPrefs.DeleteAll();
			}
			PlayerPrefs.SetString(VERSION_NAME, Application.version);
			PlayerPrefs.Save();

			_overlay.SetColor(Color.black);

			int startIndex = _forceStartLevel0 ? 0 : PlayerPrefs.GetInt(COMPLETED_NAME, 0) == 1 ? 4 : 0;
			LoadLevel(_levels[startIndex], false);
		}

		public void LoadLevel(LevelScenes _level, bool useDelay = true)
		{
			_levelIndex = _levels.IndexOf(_level);
			StartCoroutine(ProcessLoad(_level, useDelay));
		}

		public void LoadLevel(int index, bool useDelay = true)
		{
			_levelIndex = Mathf.Clamp(index, 0, _levels.Count - 1);
			StartCoroutine(ProcessLoad(_levels[_levelIndex], useDelay));
		}

		public void RestartLevel()
		{
			LoadLevel(_levelIndex);
		}

		[ContextMenu("Load Next Level")]
		public void LoadNextLevel()
		{
			_levelIndex = (_levelIndex + 1) % _levels.Count;
			LoadLevel(_levels[_levelIndex]);
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

		#region Unloading

		public void UnloadAllScenes()
		{
			_working = true;

			if (SceneManager.sceneCount > 1)
			{
				SceneManager.sceneUnloaded += OnSceneUnloadedCheck;

				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					Scene scene = SceneManager.GetSceneAt(i);
					if (scene != gameObject.scene) SceneManager.UnloadSceneAsync(scene);
				}
			}
			else
				_working = false;
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
			float startTime = Time.realtimeSinceStartup;
			AsyncOperation async = Resources.UnloadUnusedAssets();

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

			if (_loadCount == _levels[_levelIndex].Scenes.Count)
			{
				SceneManager.sceneLoaded -= OnSceneLoaded;
				SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneWillBeActive));
				_working = false;
			}
		}

		#endregion

		private void SetupXR()
		{
			if (Unity.XR.Oculus.Performance.TryGetDisplayRefreshRate(out var rate))
			{
				float newRate = 90f;
				if (Unity.XR.Oculus.Performance.TryGetAvailableDisplayRefreshRates(out var rates))
				{
					newRate = rates.Max();
				}
				if (rate < newRate)
				{
					if (Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(newRate))
					{
						Time.fixedDeltaTime = 1f / newRate;
						Time.maximumDeltaTime = 1f / newRate;
					}
				}
			}
		}
	}

	[System.Serializable]
	public class LevelScenes
	{
		public string LevelName;
		public List<string> Scenes;
	}
}

