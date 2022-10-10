/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using Oculus.Voice;

namespace Whisperer
{
	public class ListenableUI : FollowUI
	{

		// Listenable
		protected AppVoiceExperience _appVoiceExperience;
		protected SpeakGestureWatcher _speakGestureWatcher;
		[SerializeField] protected Listenable _listenable;
		protected MicInputValue _micInputValue;

		public Listenable Listenable { get => _listenable; set => _listenable = value; }


		protected override void Start()
		{
			base.Start();

			_appVoiceExperience = FindObjectOfType<AppVoiceExperience>();
			_micInputValue = FindObjectOfType<MicInputValue>();
			_speakGestureWatcher = FindObjectOfType<SpeakGestureWatcher>();

			_appVoiceExperience.VoiceEvents.OnMinimumWakeThresholdHit.AddListener(OnMinimumWakeThresholdHit);

			SetOpacity(0);
			Reset();
		}

		protected virtual void OnMinimumWakeThresholdHit()
		{

		}

		protected virtual void Reset()
		{
			// Scale based on distance from camera
			SetScale();			
		}

		private void OnDestroy()
		{
			_appVoiceExperience.VoiceEvents.OnMinimumWakeThresholdHit.RemoveListener(OnMinimumWakeThresholdHit);
		}

	}
}
