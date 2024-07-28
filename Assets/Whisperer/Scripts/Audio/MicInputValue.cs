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
    public class MicInputValue : MonoBehaviour
    {
        [SerializeField] private AppVoiceExperience _appVoiceExperience;

        public float AudioLevel;

        private void OnEnable()
        {
            _appVoiceExperience.VoiceEvents.OnMicLevelChanged.AddListener(OnMicLevelChanged);
        }

        private void OnDisable()
        {
            _appVoiceExperience.VoiceEvents.OnMicLevelChanged.RemoveListener(OnMicLevelChanged);
        }

        private void OnMicLevelChanged(float level)
        {
            AudioLevel = level;
        }
    }
}
