/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
    public class SpeakParticles : MonoBehaviour
    {
        [SerializeField] private SpeakGestureWatcher _speakGestureWatcher;
        [SerializeField] private ParticleSystem _ps;
        [SerializeField] private MicInputValue _micInputValue;
        [SerializeField] private float _threshold;

        private ParticleSystem.EmissionModule _em;
        private float _value;

        private void Start()
        {
            if (_ps is null) _ps = GetComponent<ParticleSystem>();
            _em = _ps.emission;
        }

        private void Update()
        {
            _value = _speakGestureWatcher.HaveSpeakPose ? _micInputValue.AudioLevel * 100 : 0;
            SetEmissionEnabled(_value > _threshold);
        }

        public void SetEmissionEnabled(bool enable)
        {
            _em.enabled = enable;
        }
    }
}
