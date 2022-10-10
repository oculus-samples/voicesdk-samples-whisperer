/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using System.Collections.Generic;

namespace Whisperer {

    [CreateAssetMenu(fileName = "ResponseDefinition", menuName = "ScriptableObjects/ResponseDefinition", order = 3)]
    public class ResponseDefinition : ScriptableObject
    {
        public List<Response> Responses = new List<Response>();       
    }
    [System.Serializable]
    public class Response
    {
        public List<string> Phrases = new List<string>();
    }
}