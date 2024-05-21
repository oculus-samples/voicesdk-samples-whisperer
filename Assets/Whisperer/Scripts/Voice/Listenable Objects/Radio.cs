/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
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

        private const string TURN_ON_INTENT = "turn_on";
        private const string TURN_ON_RADIO_INTENT = "turn_on_radio";
        private const string PLAY_MUSIC_INTENT = "wit$play_music";
        private const string TURN_OFF_INTENT = "turn_off";
        private const string TURN_OFF_RADIO_INTENT = "turn_off_radio";
        private const string STOP_MUSIC_INTENT = "wit$stop_music";
        private const string CHANGE_STATION_INTENT = "change_station";


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

        [MatchIntent(CHANGE_STATION_INTENT)]
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

        [MatchIntent(TURN_ON_INTENT)]
        public void TurnOn()
        {
            StartPlayingMusic();
        }

        [MatchIntent(TURN_ON_RADIO_INTENT)]
        public void TurnOnRadio()
        {
            StartPlayingMusic();
        }

        [MatchIntent(PLAY_MUSIC_INTENT)]
        public void PlayMusic()
        {
            StartPlayingMusic();
        }
        
        private void StartPlayingMusic()
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

        [MatchIntent(TURN_OFF_INTENT)]
        public void TurnOff()
        {
            StopPlayingMusic();
        }
        
        [MatchIntent(TURN_OFF_RADIO_INTENT)]
        public void TurnOffRadio()
        {
            StopPlayingMusic();
        }
        
        [MatchIntent(STOP_MUSIC_INTENT)]
        public void StopMusic()
        {
            StopPlayingMusic();
        }
        
        private void StopPlayingMusic()
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
