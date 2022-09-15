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
		protected override void Start()
		{
			base.Start();

			_speakGestureWatcher.AllowSpeak = !_levelLogicEnabled;

			UXManager.Instance.SetDisplayEnabled("SettingsMenu", false);
		}

		public override void StartLevel()
		{
			_hands.SetRay();

			FindObjectOfType<CameraColorOverlay>().SetTargetColor(Color.black);

			StartMenu();
		}

		private void StartMenu()
		{
			UXManager.Instance.OpenDisplay("StartMenu");
		}
	}
}
