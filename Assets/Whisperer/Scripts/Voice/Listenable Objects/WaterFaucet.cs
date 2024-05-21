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
    public class WaterFaucet : Listenable
    {
        private const string TURN_ON_INTENT = "turn_on";
        private const string TURN_ON_WATER_INTENT = "turn_on_water";
        private const string OPEN_INTENT = "open";
        private const string CLOSE_INTENT = "close";
        private const string TURN_OFF_WATER_INTENT = "turn_off_water";
        private const string TURN_OFF_INTENT = "turn_off";

        [SerializeField] private Animator _animator;
        public bool IsOn { get; private set; }

        [MatchIntent(TURN_ON_WATER_INTENT)]
        public void TurnOnWater()
        {
            TurnOnWaterFaucet();
        }

        [MatchIntent(TURN_ON_INTENT)]
        public void TurnOn()
        {
            TurnOnWaterFaucet();
        }
        
        [MatchIntent(OPEN_INTENT)]
        public void Open()
        {
            TurnOnWaterFaucet();
        }
        
        private void TurnOnWaterFaucet()
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
        
        [MatchIntent(CLOSE_INTENT)]
        public void Close()
        {
            TurnOffWaterFaucet();
        }
        
        [MatchIntent(TURN_OFF_INTENT)]
        public void TurnOff()
        {
            TurnOffWaterFaucet();
        }
        
        [MatchIntent(TURN_OFF_WATER_INTENT)]
        public void TurnOffWater()
        {
            TurnOffWaterFaucet();
        }
        
        private void TurnOffWaterFaucet()
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
