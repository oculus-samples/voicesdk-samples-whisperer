/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using Facebook.WitAi.Lib;

namespace Whisperer
{
	public class MicInputValue : MonoBehaviour
	{
		public bool MicActive;
		public float AudioLevel;

		Mic _mic;

		private void Start()
		{
			_mic = FindObjectOfType<Mic>();
			_mic.OnSampleReady += _mic_OnSampleReady;
			_mic.OnStartRecording += _mic_OnStartRecording;
			_mic.OnStartRecordingFailed += _mic_OnStartRecordingFailed;
			_mic.OnStopRecording += _mic_OnStopRecording;
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

		private void _mic_OnSampleReady(int sampleCount, float[] sample, float levelMax)
		{
			AudioLevel = levelMax;
		}
	}
}
