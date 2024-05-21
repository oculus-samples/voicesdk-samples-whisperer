/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using UnityEngine.Serialization;

namespace Whisperer
{
    public class SpeakParticles : MonoBehaviour
    {
        [SerializeField] private SpeakGestureWatcher _speakGestureWatcher;
        [SerializeField] private ParticleSystem _ps;
        [SerializeField] private MicInputValue _micInputValue;
        [SerializeField] private float _threshold;
        [SerializeField] private float _emissionRate = 100;
        [SerializeField] private bool _distanceBasedSize;
        [SerializeField] private float _distanceBasedSizeMultiplier = 1;
        private ParticleSystem.EmissionModule _em;
        private float _value;
        private ParticleSystem.MainModule _psMain;

        private void Start()
        {
            if (_ps is null) _ps = GetComponent<ParticleSystem>();
            _em = _ps.emission;
            _psMain = _ps.main;
        }

        private void Update()
        {
            var isContinuous = _threshold == 0;
            _value = _speakGestureWatcher.HaveSpeakPose ? _micInputValue.AudioLevel * 100 : 0;
            SetEmissionEnabled(_value >= _threshold);
            var rate = _speakGestureWatcher.HaveSpeakPose ? (_value - _threshold) : 0;
            
            // if we are speaking and the threshold is 0, we want to make sure we emit particles
            if (isContinuous && _speakGestureWatcher.HaveSpeakPose)
            {
                rate += 1;
            }

            if (isContinuous && _speakGestureWatcher.HaveListenable)
            {
                rate = 0;
            }
            
            _em.rateOverTime = (rate * _emissionRate );

            if (_distanceBasedSize)
            {
                var distance = Vector3.Distance(Camera.main.transform.position, transform.position);
                _psMain.startSize = distance * _distanceBasedSizeMultiplier;
            }
        }

        public void SetEmissionEnabled(bool enable)
        {
            _em.enabled = enable;
        }
    }
}
