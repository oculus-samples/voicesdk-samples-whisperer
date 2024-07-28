/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Meta.WitAi;
using Meta.WitAi.Json;
using UnityEngine;

namespace Whisperer
{
    public class Openable : Listenable
    {
        private const string CLOSE_INTENT = "close";
        private const string OPEN_INTENT = "open";

        public Animator animator;
        public bool IsOpen { get; private set; }

        [MatchIntent(OPEN_INTENT)]
        public void Open()
        {
            if (!IsSelected || !_actionState)
            {
                return;
            }
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

        [MatchIntent(CLOSE_INTENT)]
        public void Close()
        {
            if (!IsSelected || !_actionState)
            {
                return;
            }
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
