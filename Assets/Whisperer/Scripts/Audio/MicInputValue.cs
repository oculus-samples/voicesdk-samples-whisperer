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
	public class MicInputValue : MonoBehaviour
	{
		public bool MicActive;
		public float AudioLevel;
		private float _minVol = float.MaxValue;
		private float _maxVol = float.MinValue;

		protected AppVoiceExperience _appVoiceExperience;

		private void Start()
		{
			_appVoiceExperience = FindObjectOfType<AppVoiceExperience>();
			_appVoiceExperience.VoiceEvents.OnMicLevelChanged.AddListener(_mic_OnSampleReady);
			_appVoiceExperience.VoiceEvents.OnStartListening.AddListener(_mic_OnStartRecording);
			_appVoiceExperience.VoiceEvents.OnStoppedListening.AddListener(_mic_OnStopRecording);
			_appVoiceExperience.VoiceEvents.OnAborted.AddListener(_mic_OnStartRecordingFailed);
		}

		private void _mic_OnStopRecording()
		{
			MicActive = false;
		}

		private void _mic_OnStartRecordingFailed()
		{
			MicActive = false;
		}

		private void _mic_OnStartRecording()
		{
			MicActive = true;
		}

		private void _mic_OnSampleReady(float levelMax)
		{
			// Normalize the mic levels
			_minVol = Mathf.Min(levelMax, _minVol);
			_maxVol = Mathf.Max(levelMax, _maxVol);
			if (_maxVol == _minVol)
			{
				AudioLevel = 0;
			}
			else
			{
				AudioLevel = Mathf.Clamp01((levelMax - _minVol) / (_maxVol - _minVol));
			}
		}
	}
}
