/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Whisperer
{
        /// <summary>
        ///     Pools positional audio sources and routes playback of audio clips
        /// </summary>
        public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioSource _narratorSource;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _auxSource;
        [SerializeField] private GameObject _spatialPrefab;
        [SerializeField] private int _spatialPoolSize = 3;
        [SerializeField] private List<SoundLib> _soundLibs;
        [SerializeField] private Easings.Functions _easing;

        [SerializeField] private List<Transform> _spatialSources = new();
        [SerializeField] private List<SoundLib.ClipData> _clipDatas = new();

        [Header("Debug")] [SerializeField] private int _voicesInUsePeak;

        public Progress MasterFader;
        public Progress AmbientFader;
        public Progress MusicFader;
        public Progress AuxFader;
        [SerializeField] private readonly List<IEnumerator> _coroutines = new();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            LevelLoader.Instance.OnLevelLoadComplete.AddListener(LevelLoadComplete);

            MasterFader = new Progress(FadeMaster);
            AmbientFader = new Progress(FadeAmbient);
            MusicFader = new Progress(FadeMusic);
            AuxFader = new Progress(FadeAux);

            MasterFader.SetProgress(0);
        }

        private void OnDestroy()
        {
            LevelLoader.Instance.OnLevelLoadComplete.RemoveListener(LevelLoadComplete);
        }

        private void OnApplicationQuit()
        {
            enabled = false;
        }

        public float Play(string playID, Transform positionTransform, float volume = 1)
        {
            float length = 0;

            var clipData = FindData(playID);

            if (clipData != null)
            {
                clipData.AudioClip = ChooseClip(clipData);

                if (clipData.AudioClip)
                {
                    clipData.Volume = volume;
                    length = clipData.AudioClip.length;

                    /// Use spatial pool
                    if (clipData.Spatial) PlaySpatialSource(clipData, positionTransform);
                    /// Stereo play
                    else _narratorSource.PlayOneShot(clipData.AudioClip, clipData.Volume);
                }
            }

            return length;
        }

        public void RemoveSpatialSource(Transform source)
        {
            if (enabled)
            {
                if (_spatialSources.Contains(source)) _spatialSources.Remove(source);

                if (FindObjectOfType<LevelLoader>()?.IsTransition == false)
                {
                    Debug.Log("SpatialSource: Warning! Is being destroyed! Is this intentional?");
                    RefreshPool();
                }
            }
        }

        public void Stop(string playID)
        {
            for (var i = 0; i < _clipDatas.Count; i++)
                if (_clipDatas[i] != null && _clipDatas[i].PlayID == playID)
                {
                    var source = _spatialSources[i].GetComponent<AudioSource>();
                    if (_coroutines[i] is not null) StopCoroutine(_coroutines[i]);
                    source.Stop();
                    source.loop = false;
                    source.transform.parent = null;
                    _clipDatas[i] = null;
                }
        }

        public void PlayMusic(string playID)
        {
            _musicSource.clip = ChooseClip(FindData(playID));
            _musicSource.Play();
        }

        public void StopMusic(float fadeTime)
        {
            MusicFader.PlayReverse(fadeTime);
        }

        public void PlayAux(string playID)
        {
            _auxSource.clip = ChooseClip(FindData(playID));
            _auxSource.Play();
        }

        public void StopAux(float fadeTime)
        {
            AuxFader.PlayReverse(fadeTime);
        }

        public void StopNarration()
        {
            _narratorSource.Stop();
        }

        public void StopAllSpatial()
        {
            _spatialSources.ForEach(s => s.GetComponent<AudioSource>().Stop());
        }

        private SoundLib.ClipData FindData(string playID)
        {
            SoundLib.ClipData data = null;

            _soundLibs.ForEach(lib =>
            {
                lib.ClipsData.ForEach(clipData =>
                {
                    if (clipData.PlayID == playID) data = clipData;
                });
            });

            return data;
        }

        private AudioClip ChooseClip(SoundLib.ClipData clipData)
        {
            AudioClip clip = null;

            switch (clipData.SelectMethod)
            {
                case SoundLib.SelectMethod.Sequential:
                    clip = clipData.AudioClips[clipData.Index];
                    clipData.Index = (clipData.Index + 1) % clipData.AudioClips.Count;
                    break;
                case SoundLib.SelectMethod.Random:
                    clip = clipData.AudioClips[Random.Range(0, clipData.AudioClips.Count)];
                    break;
            }

            if (clip is null)
                Debug.LogError("AudioManager: Can't find an audio clip for " + clipData.PlayID);

            return clip;
        }

        private void PlaySpatialSource(SoundLib.ClipData clipData, Transform positionTransform)
        {
            if (_spatialSources.Count < _spatialPoolSize) RefreshPool();
            var source = GetAudioSource(positionTransform);

            if (source)
            {
                var index = _spatialSources.IndexOf(source.transform);

                source.transform.GetComponent<SpatialSource>().FollowTransform = positionTransform;

                if (clipData.Loop)
                {
                    /// Looped
                    _clipDatas[_spatialSources.IndexOf(source.transform)] = clipData;
                    source.loop = true;
                    source.clip = clipData.AudioClip;
                    source.volume = clipData.Volume;
                    source.Play();
                }
                else
                {
                    /// One Shot
                    _coroutines[index] = PlaySpatialDelay(source, clipData);
                    StartCoroutine(_coroutines[index]);
                }
            }
        }

        private AudioSource GetAudioSource(Transform positionTransform)
        {
            if (_spatialSources.Count == 0) return null;

            AudioSource source = null;
            var assigned = false;

            /// Pre-assigned check (same transform parent, not looping)
            for (var i = 0; i < _spatialSources.Count; i++)
            {
                source = _spatialSources[i].GetComponent<AudioSource>();

                if (source.transform.parent == positionTransform && !source.loop)
                {
                    if (source.isPlaying) StopCoroutine(_coroutines[i]);
                    assigned = true;
                }

                if (assigned) break;
            }

            /// Find an unused source
            if (!assigned)
            {
                for (var i = 0; i < _spatialSources.Count; i++)
                {
                    source = _spatialSources[i].GetComponent<AudioSource>();

                    if (!source.isPlaying && !source.loop) assigned = true;

                    if (assigned) break;
                }

                if (!assigned)
                    Debug.Log("AudioManager: Ran out of voices!");
            }

            return source;
        }

        private IEnumerator PlaySpatialDelay(AudioSource audioSource, SoundLib.ClipData clipData)
        {
            audioSource.PlayOneShot(clipData.AudioClip, clipData.Volume);
            _clipDatas[_spatialSources.IndexOf(audioSource.transform)] = clipData;

            yield return new WaitForSeconds(clipData.AudioClip.length);

            //audioSource.transform.GetComponent<SpatialSource>().FollowTransform = null;
            _clipDatas[_spatialSources.IndexOf(audioSource.transform)] = null;
        }

        private void LevelLoadComplete()
        {
            RefreshPool();
        }

        private void FadeMaster(float progress)
        {
            var volume = Mathf.Lerp(-80, 0, Easings.Interpolate(progress, _easing));
            _audioMixer.SetFloat("SpatialVolume", volume);
            _audioMixer.SetFloat("NarratorVolume", volume);
            AmbientFader.SetProgress(progress);
        }

        private void FadeMusic(float progress)
        {
            var volume = Mathf.Lerp(-80, 0, Easings.Interpolate(progress, _easing));

            _audioMixer.SetFloat("MusicVolume", volume);

            if (progress == 0)
                _musicSource.Stop();
        }

        private void FadeAmbient(float progress)
        {
            var volume = Mathf.Lerp(-80, 0, Easings.Interpolate(progress, _easing));

            _audioMixer.SetFloat("AmbientVolume", volume);
        }

        private void FadeAux(float progress)
        {
            var volume = Mathf.Lerp(-80, 0, Easings.Interpolate(progress, _easing));

            _audioMixer.SetFloat("AuxVolume", volume);

            if (progress == 0)
                _auxSource.Stop();
        }

        private void RefreshPool()
        {
            var count = 0;
            while (_spatialSources.Count < _spatialPoolSize)
            {
                _spatialSources.Add(Instantiate(_spatialPrefab).transform);
                _coroutines.Add(null);
                _clipDatas.Add(null);
                count++;
            }
        }
    }
}
