/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using System.Collections.Generic;

namespace Whisperer {

    [CreateAssetMenu(fileName = "Utterance Tooltip Definition", menuName = "ScriptableObjects/UtteranceTooltipDefinition", order = 1)]
    public class UtteranceTooltipDefinition : ScriptableObject
    {
        public List<string> exampleUtterances;
    } 
}