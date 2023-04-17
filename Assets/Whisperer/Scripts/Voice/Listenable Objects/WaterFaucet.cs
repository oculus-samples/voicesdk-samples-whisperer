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
    public class WaterFaucet : Listenable
    {
        [SerializeField] private Animator _animator;
        public bool IsOn { get; private set; }

        [MatchIntent("turn_on")]
        [MatchIntent("turn_on_water")]
        [MatchIntent("open")]
        public void TurnOnWater()
        {
            if(!IsSelected || !_actionState)
            {
                return;
            }
            if (IsOn)
            {
                ProcessComplete("turn_on", true);
            }
            else
            {
                IsOn = true;
                _animator.SetTrigger("Hose_On");
                ProcessComplete("turn_on", true);
            }
        }

        [MatchIntent("turn_off_water")]
        [MatchIntent("turn_off")]
        [MatchIntent("close")]
        public void TurnOffWater()
        {
            if(!IsSelected || !_actionState)
            {
                return;
            }
            if (!IsOn)
            {
                ProcessComplete("turn_off", true);
            }
            else
            {
                IsOn = false;
                ProcessComplete("turn_off", true);
            }
        }
    }
}
