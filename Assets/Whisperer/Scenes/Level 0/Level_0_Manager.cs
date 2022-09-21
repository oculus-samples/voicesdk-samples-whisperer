/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Whisperer
{
	public class Level_0_Manager : LevelManager
	{
		[SerializeField] GameObject _startMenu;
		[SerializeField] GameObject _consentDialog;

		protected override void Start()
		{
			PlayerPrefs.DeleteAll();
			base.Start();

			_speakGestureWatcher.AllowSpeak = !_levelLogicEnabled;

			_startMenu.SetActive(false);
			_consentDialog.SetActive(false);
		}

		public override void StartLevel()
		{
			_hands.SetRay();

			FindObjectOfType<CameraColorOverlay>().SetTargetColor(Color.black);

			if (PlayerPrefs.GetInt("VoiceDataConsent", 0) == 1)
				StartMenu();
			else
				ShowConsent();
		}

		private void ShowConsent()
		{
			_consentDialog.SetActive(true);
		}

		private void StartMenu()
		{
			_startMenu.SetActive(true);
		}

		public void ConsentAccept()
		{
			PlayerPrefs.SetInt("VoiceDataConsent", 1);

			_consentDialog.SetActive(false);

			StartMenu();
		}

		public void ConsentDecline()
		{
			Application.Quit();
		}
	}
}
