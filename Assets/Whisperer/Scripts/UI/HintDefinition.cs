/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using System.Collections.Generic;

namespace Whisperer {

    [CreateAssetMenu(fileName = "Hint Definition", menuName = "ScriptableObjects/HintDefinition", order = 2)]
    public class HintDefinition : ScriptableObject
    {
        public List<string> hints = new List<string>();
    } 
}