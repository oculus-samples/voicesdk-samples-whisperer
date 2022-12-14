/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Facebook.WitAi.Lib;
using UnityEngine;

namespace Facebook.WitAi.Data.Entities
{
    public abstract class WitEntityDataBase<T>
    {
        public string body;

        public float confidence;
        public int end;

        public WitResponseArray entities;

        public bool hasData;
        public string id;
        public string name;
        public WitResponseNode responseNode;
        public string role;

        public int start;

        public string type;
        public T value;

        public WitEntityDataBase<T> FromEntityWitResponseNode(WitResponseNode node)
        {
            responseNode = node;
            id = node[WitEntity.Fields.ID];
            name = node[WitEntity.Fields.NAME];
            role = node[WitEntity.Fields.ROLE];
            start = node[WitEntity.Fields.START].AsInt;
            end = node[WitEntity.Fields.END].AsInt;
            type = node[WitEntity.Fields.TYPE];
            body = node[WitEntity.Fields.BODY];
            confidence = node[WitEntity.Fields.CONFIDENCE].AsFloat;
            hasData = !string.IsNullOrEmpty(node.Value);
            value = OnParseValue(node);
            entities = node[WitEntity.Fields.ENTITIES].AsArray;
            return this;
        }

        protected abstract T OnParseValue(WitResponseNode node);

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class WitEntityData : WitEntityDataBase<string>
    {
        public WitEntityData()
        {
        }

        public WitEntityData(WitResponseNode node)
        {
            FromEntityWitResponseNode(node);
        }

        protected override string OnParseValue(WitResponseNode node)
        {
            return node[WitEntity.Fields.VALUE];
        }

        public static implicit operator bool(WitEntityData data)
        {
            return null != data && !string.IsNullOrEmpty(data.value);
        }

        public static implicit operator string(WitEntityData data)
        {
            return data.value;
        }

        public static bool operator ==(WitEntityData data, object value)
        {
            return Equals(data?.value, value);
        }

        public static bool operator !=(WitEntityData data, object value)
        {
            return !Equals(data?.value, value);
        }

        public override bool Equals(object obj)
        {
            if (obj is string s) return s == value;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class WitEntityFloatData : WitEntityDataBase<float>
    {
        public WitEntityFloatData()
        {
        }

        public WitEntityFloatData(WitResponseNode node)
        {
            FromEntityWitResponseNode(node);
        }

        protected override float OnParseValue(WitResponseNode node)
        {
            return node[WitEntity.Fields.VALUE].AsFloat;
        }

        public static implicit operator bool(WitEntityFloatData data)
        {
            return null != data && data.hasData;
        }

        public bool Approximately(float v)
        {
            return Mathf.Approximately(value, v);
        }

        public static bool operator ==(WitEntityFloatData data, float value)
        {
            return data?.value == value;
        }

        public static bool operator !=(WitEntityFloatData data, float value)
        {
            return !(data == value);
        }

        public static bool operator ==(WitEntityFloatData data, int value)
        {
            return data?.value == value;
        }

        public static bool operator !=(WitEntityFloatData data, int value)
        {
            return !(data == value);
        }

        public static bool operator ==(float value, WitEntityFloatData data)
        {
            return data?.value == value;
        }

        public static bool operator !=(float value, WitEntityFloatData data)
        {
            return !(data == value);
        }

        public static bool operator ==(int value, WitEntityFloatData data)
        {
            return data?.value == value;
        }

        public static bool operator !=(int value, WitEntityFloatData data)
        {
            return !(data == value);
        }

        public override bool Equals(object obj)
        {
            if (obj is float f) return f == value;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class WitEntityIntData : WitEntityDataBase<int>
    {
        public WitEntityIntData()
        {
        }

        public WitEntityIntData(WitResponseNode node)
        {
            FromEntityWitResponseNode(node);
        }

        protected override int OnParseValue(WitResponseNode node)
        {
            return node[WitEntity.Fields.VALUE].AsInt;
        }

        public static implicit operator bool(WitEntityIntData data)
        {
            return null != data && data.hasData;
        }

        public static bool operator ==(WitEntityIntData data, int value)
        {
            return data?.value == value;
        }

        public static bool operator !=(WitEntityIntData data, int value)
        {
            return !(data == value);
        }

        public static bool operator ==(int value, WitEntityIntData data)
        {
            return data?.value == value;
        }

        public static bool operator !=(int value, WitEntityIntData data)
        {
            return !(data == value);
        }

        public override bool Equals(object obj)
        {
            if (obj is int i) return i == value;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
