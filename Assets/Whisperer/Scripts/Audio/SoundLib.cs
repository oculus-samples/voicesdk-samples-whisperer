/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{
    [CreateAssetMenu(fileName = "SoundLib", menuName = "Buck/SoundLib")]
    public class SoundLib : ScriptableObject
    {
        public enum SelectMethod
        {
            Sequential,
            Random
        }

        public List<ClipData> ClipsData;

        [Serializable]
        public class ClipData
        {
            public string PlayID;
            public bool Spatial;
            public bool Loop;
            public SelectMethod SelectMethod;
            public List<AudioClip> AudioClips;

            public AudioClip AudioClip { get; set; }
            public float Volume { get; set; }
            public int Index { get; set; }
        }
    }
}
