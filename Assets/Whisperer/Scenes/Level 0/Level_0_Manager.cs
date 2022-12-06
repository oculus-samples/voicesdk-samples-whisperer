/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.VoiceSDK.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Whisperer
{
	public class Level_0_Manager : LevelManager
	{
		[SerializeField] GameObject _startMenu;
		[SerializeField] GameObject _consentDialog;
		[SerializeField] GameObject _micPermissionsDialog;
		
		protected override void Start()
		{
			PlayerPrefs.DeleteAll();
			base.Start();

			_speakGestureWatcher.AllowSpeak = !_levelLogicEnabled;

			_startMenu.SetActive(false);
			_consentDialog.SetActive(false);
			_micPermissionsDialog.SetActive(false);
		}

		public override void StartLevel()
		{
			_hands.SetRay();

			FindObjectOfType<CameraColorOverlay>().SetTargetColor(Color.black);

			if (MicPermissionsManager.HasMicPermission())
			{
				MicPermissionsObtained();
			}
			else
			{
				ShowMicPermissionsPrompt();
			}
		}

		private void MicPermissionsObtained()
		{
			Debug.Log("Mic permissions obtained");
			_micPermissionsDialog.SetActive(false);
			if (PlayerPrefs.GetInt("VoiceDataConsent", 0) == 1)
				StartMenu();
			else
				ShowConsent();
		}

		private void ShowMicPermissionsPrompt()
		{
			Debug.Log("Showing mic permissions dialog");
			_micPermissionsDialog.SetActive(true);
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

		public void AcceptPermissionsRequest()
		{
			Debug.Log("Showing Android permissions prompt");
			MicPermissionsManager.RequestMicPermission(PermissionGrantedCallback);
			Debug.Log("Done showing Android permissions prompt");
		}

		private void PermissionGrantedCallback(string permission)
		{
			Debug.Log("Android permission granted");
			MicPermissionsObtained();
		}

		public void RejectPermissions()
		{
			Application.Quit();
		}
	}
}
