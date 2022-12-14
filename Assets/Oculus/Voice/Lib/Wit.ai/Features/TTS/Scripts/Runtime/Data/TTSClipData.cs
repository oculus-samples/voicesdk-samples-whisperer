/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Facebook.WitAi.TTS.Data
{
    // Various request load states
    public enum TTSClipLoadState
    {
        Unloaded,
        Preparing,
        Loaded,
        Error
    }

    [Serializable]
    public class TTSClipData
    {
        // Text to be spoken
        public string textToSpeak;

        // Unique identifier
        public string clipID;

        // Audio type
        public AudioType audioType = AudioType.WAV; // Default

        // Cache settings for request
        public TTSDiskCacheSettings diskCacheSettings;

        // Clip
        [NonSerialized] public AudioClip clip;

        // Clip load progress
        [NonSerialized] public float loadProgress;

        // Clip load state
        [NonSerialized] public TTSClipLoadState loadState;

        /// <summary>
        ///     A callback when clip has downloaded successfully
        ///     Returns an error if there was an issue
        /// </summary>
        public Action<string> onDownloadComplete;

        /// <summary>
        ///     A callback when clip stream is ready
        ///     Returns an error if there was an issue
        /// </summary>
        public Action<string> onPlaybackReady;

        // On clip state change
        public Action<TTSClipData, TTSClipLoadState> onStateChange;

        // Request data
        public Dictionary<string, string> queryParameters;

        // Voice settings for request
        public TTSVoiceSettings voiceSettings;
    }
}
