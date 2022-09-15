/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using Facebook.WitAi;
using Facebook.WitAi.Lib;
using Facebook.WitAi.Data.Intents;

namespace Whisperer
{
    public class WaterFaucet : Listenable
    {
		[SerializeField] private Animator _animator;
		public bool IsOn { get => _isOn; }
		private bool _isOn;

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

				if (_witUI.LastTranscriptionCache.ToLower().Contains("off"))
				{
					action = "turn_off";
				}

			}

			switch (action)
				{
					case "turn_on":
						TurnOnWater();
						break;
					case "turn_on_water":
						TurnOnWater();
						break;
					case "open":
						TurnOnWater();
						break;
					case "turn_off":
						TurnOffWater();
						break;
					case "turn_off_water":
						TurnOffWater();
						break;
					case "close":
						TurnOffWater();
						break;
					default:
						ProcessComplete(action, false);
						break;
				}
			}

		public void TurnOnWater()
		{
			if (_isOn)
			{
				ProcessComplete("turn_on", true);
			}
			else
			{
				_isOn = true;
				_animator.SetTrigger("Hose_On");
				ProcessComplete("turn_on", true);
			}

		}

		public void TurnOffWater()
		{
			if (!_isOn)
			{
				ProcessComplete("turn_off", true);
			}
			else
			{
				_isOn = false;
				ProcessComplete("turn_off", true);
			}
		}
	}
}