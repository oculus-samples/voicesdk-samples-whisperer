/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Facebook.WitAi.Lib;

namespace Facebook.WitAi.Data.Intents
{
    public class WitIntentData
    {
        public float confidence;

        public string id;
        public string name;
        public WitResponseNode responseNode;

        public WitIntentData()
        {
        }

        public WitIntentData(WitResponseNode node)
        {
            FromIntentWitResponseNode(node);
        }

        public WitIntentData FromIntentWitResponseNode(WitResponseNode node)
        {
            responseNode = node;
            id = node[WitIntent.Fields.ID];
            name = node[WitIntent.Fields.NAME];
            confidence = node[WitIntent.Fields.CONFIDENCE].AsFloat;
            return this;
        }
    }
}
