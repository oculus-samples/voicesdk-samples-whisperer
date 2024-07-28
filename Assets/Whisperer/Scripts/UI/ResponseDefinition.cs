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
    [CreateAssetMenu(fileName = "ResponseDefinition", menuName = "ScriptableObjects/ResponseDefinition", order = 3)]
    public class ResponseDefinition : ScriptableObject
    {
        public List<Response> Responses = new();
    }

    [Serializable]
    public class Response
    {
        public List<string> Phrases = new();
    }
}
