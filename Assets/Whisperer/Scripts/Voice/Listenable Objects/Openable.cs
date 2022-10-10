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
	public class Openable : Listenable
	{
		public Animator animator;
		public bool IsOpen { get => _open; }
		private bool _open;

		protected override void DetermineAction(WitResponseNode witResponse)
		{
			WitIntentData data = witResponse.GetFirstIntentData();
			string action = data == null ? "" : data.name;

			switch (action)
			{
				case "open":
					Open();
					break;
				case "close":
					Close();
					break;
				default:
					ProcessComplete(action, false);
					break;
			}
		}

		public void Open() {
			// If we're alread open, send non-actionable state 
			if (_open)
			{
				ProcessComplete("", true);
			}
			// otherwise open it
			else {
				animator.SetTrigger("Open");
				_open = true;
				ProcessComplete("open", true);
			}
		}

		public void Close() {
			// If we're alread closed, send non-actionable state 
			if (!_open)
			{
				ProcessComplete("", true);
			}
			// otherwise open it
			else
			{
				animator.SetTrigger("Close");
				_open = false;
				ProcessComplete("close", true);
			}
		}

	}
}