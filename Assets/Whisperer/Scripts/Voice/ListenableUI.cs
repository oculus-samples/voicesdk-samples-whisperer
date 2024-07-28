/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Oculus.Voice;
using UnityEngine;

namespace Whisperer
{
    public class ListenableUI : FollowUI
    {
        [SerializeField] protected Listenable _listenable;

        // Listenable
        protected AppVoiceExperience _appVoiceExperience;
        protected MicInputValue _micInputValue;
        protected SpeakGestureWatcher _speakGestureWatcher;

        public Listenable Listenable
        {
            get => _listenable;
            set => _listenable = value;
        }

        protected virtual void Reset()
        {
            // Scale based on distance from camera
            SetScale();
        }


        protected override void Start()
        {
            base.Start();

            _appVoiceExperience = FindObjectOfType<AppVoiceExperience>();
            _micInputValue = FindObjectOfType<MicInputValue>();
            _speakGestureWatcher = FindObjectOfType<SpeakGestureWatcher>();

            _appVoiceExperience.VoiceEvents.OnMinimumWakeThresholdHit.AddListener(OnMinimumWakeThresholdHit);

            SetOpacity(0);
            Reset();
        }

        private void OnDestroy()
        {
            _appVoiceExperience.VoiceEvents.OnMinimumWakeThresholdHit.RemoveListener(OnMinimumWakeThresholdHit);
        }

        protected virtual void OnMinimumWakeThresholdHit()
        {
        }
    }
}
