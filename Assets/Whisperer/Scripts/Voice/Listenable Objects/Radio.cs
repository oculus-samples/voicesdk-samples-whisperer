/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using Meta.WitAi;
using Meta.WitAi.Json;
using UnityEngine;

namespace Whisperer
{
    public class Radio : Listenable
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private List<AudioClip> _stationClips;
        [SerializeField] private List<float> _volumes;

        private float _randTime;

        public int StationIndex { get; private set; }

        public bool IsOn { get; private set; }

        protected void Awake()
        {
            _audioSource.clip = _stationClips[0];
            _randTime = Random.value * 100;
        }

        [MatchIntent("change_station")]
        public void ChangeStation()
        {
            if(!IsSelected || !_actionState)
            {
                return;
            }
            if (!IsOn)
            {
                Debug.Log("Radio needs to be ON to change station");
                ProcessComplete("change_station", true);
            }
            else
            {
                /// todo: transition effect
                _audioSource.Stop();
                StationIndex = (StationIndex + 1) % _stationClips.Count;
                _audioSource.clip = _stationClips[StationIndex];
                _audioSource.volume = _volumes[StationIndex];
                _audioSource.Play();
                ProcessComplete("change_station", true);
            }
        }

        [MatchIntent("turn_on")]
        [MatchIntent("turn_on_radio")]
        [MatchIntent("wit$play_music")]
        public void PlayMusic()
        {
            if(!IsSelected || !_actionState)
            {
                return;
            }
            if (IsOn)
            {
                Debug.Log("Music already playing");
                ProcessComplete("turn_on_radio", true);
                return;
            }

            IsOn = true;

            _audioSource.time = (_randTime + Time.time) % _audioSource.clip.length;
            _audioSource.Play();

            ProcessComplete("turn_on_radio", true);
        }

        [MatchIntent("turn_off")]
        [MatchIntent("turn_off_radio")]
        [MatchIntent("wit$stop_music")]
        public void StopMusic()
        {
            if(!IsSelected || !_actionState)
            {
                return;
            }
            if (!IsOn)
            {
                Debug.Log("Music already stopped");
                ProcessComplete("turn_off_radio", true);
                return;
            }

            IsOn = false;

            _audioSource.Stop();

            ProcessComplete("turn_off_radio", true);
        }
    }
}
