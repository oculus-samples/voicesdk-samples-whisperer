/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{
    public class AudioEventPlay : MonoBehaviour
    {
        [SerializeField] private Transform _positionTransform;
        [SerializeField] private List<EventClip> _eventClips;

        private void Awake()
        {
            if (_positionTransform == null)
                _positionTransform = transform;
        }

        private void OnEnable()
        {
            if (enabled) OnAnimationAudioEvent("Start");
        }

        private void OnValidate()
        {
            _eventClips.ForEach(e =>
            {
                if (e.Volume == 0) e.Volume = 1;
            });
        }

        public void OnAnimationAudioEvent(string eventName)
        {
            _eventClips.ForEach(e =>
            {
                if (eventName == e.EventName)
                    AudioManager.Instance.Play(e.ClipName, _positionTransform, e.Volume);
            });
        }

        public void OnAnimationAudioEventStop(string eventName)
        {
            _eventClips.ForEach(e =>
            {
                if (eventName == e.EventName)
                    AudioManager.Instance.Stop(e.ClipName);
            });
        }

        [Serializable]
        private class EventClip
        {
            public string EventName;
            public string ClipName;
            public float Volume;
        }
    }
}
