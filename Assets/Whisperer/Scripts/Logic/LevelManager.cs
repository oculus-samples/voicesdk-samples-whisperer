/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using Oculus.Voice;
using System;

/// <summary>
/// Base class for Level/Story logic
/// </summary>
namespace Whisperer
{
	public abstract class LevelManager : MonoBehaviour
	{
		[SerializeField] protected bool _levelLogicEnabled = true;
		[SerializeField] protected Vector3 _rigStartPosition;

		[Header("References")]
		[SerializeField] protected GameObject _transcriptionUIPrefab;
		[SerializeField] protected AppVoiceExperience _appVoiceExperience;
		[SerializeField] protected RigHandsControl _hands;
		[SerializeField] protected SpeakGestureWatcher _speakGestureWatcher;
		[SerializeField] protected List<Listenable> _allListenableScripts;
		[SerializeField] protected List<AnimationEvents> _allAnimationEvents;

		protected bool _loaderReady,
					   _inTransition;

		protected virtual void OnValidate()
		{
			if (!_loaderReady) return;
			SetLevelEnabled();
		}

		protected virtual void Start()
		{
			LevelLoader.Instance.OnLevelWillBeginUnload.AddListener(LevelWillUnload);
			LevelLoader.Instance.OnLevelLoadComplete.AddListener(LevelLoadComplete);
			FindObjectOfType<XROrigin>().transform.SetPositionAndRotation(_rigStartPosition, Quaternion.identity);

			/// Set up references
			_appVoiceExperience = FindObjectOfType<AppVoiceExperience>();
			_speakGestureWatcher = FindObjectOfType<SpeakGestureWatcher>();
			_hands = FindObjectOfType<RigHandsControl>();

			if (LevelLoader.Instance.IsLoading)
			{
				_allListenableScripts = new List<Listenable>(FindObjectsOfType<Listenable>());
				_allListenableScripts.ForEach(listenable => listenable.InstantiateWitUI(_transcriptionUIPrefab));
			}	
		}

		/// <summary>
		/// Called automatically after a delay, by LevelLoader when all scenes are ready.<br></br>
		/// Use this to begin logic, as Unity's Awake/Start often get called before all scenes are finished loading.
		/// </summary>
		public abstract void StartLevel();

		protected virtual void LevelLoadComplete()
		{
			AudioManager.Instance.StopNarration();
			AudioManager.Instance.StopAllSpatial();
			AudioManager.Instance.MasterFader.PlayFrom0(2);
		}

		protected virtual void LevelWillUnload(float delay)
		{
			_inTransition = true;

			AudioManager.Instance.StopNarration();
			AudioManager.Instance.StopAllSpatial();
			AudioManager.Instance.MasterFader.PlayFrom1(delay);
			AudioManager.Instance.StopMusic(delay);

			_hands.SetNone();
		}

		/// <summary>
		/// Called whenever any listenable in the scene receives a response from Wit.
		/// </summary>
		/// <param name="listenable"></param>
		/// <param name="intent"></param>
		/// <param name="success"></param>
		protected virtual void OnListenableResponse(ListenableEventArgs eventArgs) { }

		/// <summary>
		/// Called whenever any animation event in the scene is invoked.
		/// </summary>
		/// <param name="eventName"></param>
		protected virtual void OnAnimationEvent(string eventName) { }

		public void SetLoaderReady()
		{
			_loaderReady = true;
			_inTransition = false;
			SetLevelEnabled();
		}

		private void SetLevelEnabled()
		{
			if (_levelLogicEnabled && _loaderReady)
			{
				SetEventsSubcribed(true);
				StartLevel();
			}
			else
			{
				SetEventsSubcribed(false);
				StopAllCoroutines();
				_hands.SetSpeak();
			}
		}

		protected void SetEventsSubcribed(bool subscribed)
		{
			if (subscribed)
			{
				/// Subscribing to listenable response events in the scene
				_allListenableScripts = new List<Listenable>(FindObjectsOfType<Listenable>());
				_allListenableScripts.ForEach(listenable => listenable.OnResponseProcessed.AddListener(OnListenableResponse));

				/// Subscribing to all animation events in the scene
				_allAnimationEvents = new List<AnimationEvents>(FindObjectsOfType<AnimationEvents>());
				_allAnimationEvents.ForEach(ae => ae.OnAnimationEvent.AddListener(OnAnimationEvent));
			}
			else
			{
				_allListenableScripts = new List<Listenable>(FindObjectsOfType<Listenable>());
				_allListenableScripts.ForEach(listenable => listenable.OnResponseProcessed.RemoveListener(OnListenableResponse));

				_allAnimationEvents = new List<AnimationEvents>(FindObjectsOfType<AnimationEvents>());
				_allAnimationEvents.ForEach(ae => ae.OnAnimationEvent.RemoveListener(OnAnimationEvent));
			}
		}
	}
}
