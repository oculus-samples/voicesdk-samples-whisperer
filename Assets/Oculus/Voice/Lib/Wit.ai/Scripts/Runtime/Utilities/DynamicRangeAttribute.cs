/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Facebook.WitAi.Utilities
{
    public class DynamicRangeAttribute : PropertyAttribute
    {
        public DynamicRangeAttribute(string rangeProperty, float defaultMin = float.MinValue,
            float defaultMax = float.MaxValue)
        {
            DefaultMin = defaultMin;
            DefaultMax = defaultMax;
            RangeProperty = rangeProperty;
        }

        public string RangeProperty { get; }
        public float DefaultMin { get; }
        public float DefaultMax { get; }
    }
}
