/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Meta.WitAi;
using Meta.WitAi.Json;
using UnityEngine;

namespace Whisperer
{
    public class Openable : Listenable
    {
        public Animator animator;
        public bool IsOpen { get; private set; }

        protected override void DetermineAction(WitResponseNode witResponse)
        {
            var data = witResponse.GetFirstIntentData();
            var action = data == null ? "" : data.name;

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

        public void Open()
        {
            // If we're alread open, send non-actionable state 
            if (IsOpen)
            {
                ProcessComplete("", true);
            }
            // otherwise open it
            else
            {
                animator.SetTrigger("Open");
                IsOpen = true;
                ProcessComplete("open", true);
            }
        }

        public void Close()
        {
            // If we're alread closed, send non-actionable state 
            if (!IsOpen)
            {
                ProcessComplete("", true);
            }
            // otherwise open it
            else
            {
                animator.SetTrigger("Close");
                IsOpen = false;
                ProcessComplete("close", true);
            }
        }
    }
}
