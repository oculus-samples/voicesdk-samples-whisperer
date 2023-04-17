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

        [MatchIntent("open")]
        public void Open()
        {
            if(!IsSelected || !_actionState)
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

        [MatchIntent("close")]
        public void Close()
        {
            if(!IsSelected || !_actionState)
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
