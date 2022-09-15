/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using UnityEngine;
using Facebook.WitAi.Lib;
using Facebook.WitAi;
using Facebook.WitAi.Data.Intents;

namespace Whisperer
{
	public class Radio : Listenable
	{
		[SerializeField] private AudioSource _audioSource;
		[SerializeField] List<AudioClip> _stationClips;
		[SerializeField] List<float> _volumes;

		public int StationIndex { get => _index; }
		public bool IsOn { get => _isOn; }

		private int _index;
		private float _randTime;
		private bool _isOn;

		protected void Awake()
		{
			_audioSource.clip = _stationClips[0];
			_randTime = Random.value * 100;
		}

		protected override void DetermineAction(WitResponseNode witResponse)
		{
			WitIntentData data = witResponse.GetFirstIntentData();
			string action = data == null ? "" : data.name;

			// if there is no intent, look for "on" or "off" in the transcription
			if (action == "")
			{
				if (_witUI.LastTranscriptionCache.ToLower().Contains("on"))
				{
					action = "turn_on";
				}					

				if (_witUI.LastTranscriptionCache.ToLower().Contains("off")) {
					action = "turn_off";
				}				
			}			

			switch (action)
			{
				case "turn_on":
					PlayMusic();
					break;
				case "turn_on_radio":
					PlayMusic();
					break;
				case "wit$play_music":
					PlayMusic();
					break;
				case "turn_off":
					StopMusic();
					break;
				case "turn_off_radio":
					StopMusic();
					break;
				case "wit$stop_music":
					StopMusic();
					break;
				case "change_station":
					ChangeStation();
					break;
				default:
					ProcessComplete(action, false);
					break;
			}
			
		}

		public void ChangeStation()
		{
			if (!_isOn)
			{
				Debug.Log("Radio needs to be ON to change station");
				ProcessComplete("change_station", true);
			}
			else
			{
				/// todo: transition effect
				_audioSource.Stop();
				_index = (_index + 1) % _stationClips.Count;
				_audioSource.clip = _stationClips[_index];
				_audioSource.volume = _volumes[_index];
				_audioSource.Play();
				ProcessComplete("change_station", true);
			}		
		}

		public void PlayMusic()
		{

			if (_isOn) {
				Debug.Log("Music already playing");
				ProcessComplete("turn_on_radio", true);
				return;
			}

			_isOn = true;

			_audioSource.time = (_randTime + Time.time) % _audioSource.clip.length;
			_audioSource.Play();

			ProcessComplete("turn_on_radio", true);
		}

		public void StopMusic()
		{
			if (!_isOn)
			{
				Debug.Log("Music already stopped");
				ProcessComplete("turn_off_radio", true);
				return;
			}

			_isOn = false;

			_audioSource.Stop();

			ProcessComplete("turn_off_radio", true);
		}

	}
}