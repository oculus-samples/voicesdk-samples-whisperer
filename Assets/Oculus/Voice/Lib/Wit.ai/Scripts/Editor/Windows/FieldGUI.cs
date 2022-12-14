/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Reflection;
using UnityEditor;

namespace Facebook.WitAi.Windows
{
    public class FieldGUI
    {
        /// <summary>
        ///     Custom gui layout callback, returns true if field is
        /// </summary>
        public Action onAdditionalGuiLayout;

        /// <summary>
        ///     Custom gui layout callback, returns true if field is
        /// </summary>
        public Func<FieldInfo, bool> onCustomGuiLayout;

        // Base type
        public Type baseType { get; private set; }

        // Fields
        public FieldInfo[] fields { get; private set; }

        // Refresh field list
        public void RefreshFields(Type newBaseType)
        {
            // Set base type
            baseType = newBaseType;

            // Obtain all public, instance fields
            fields = GetFields(baseType);
        }

        // Obtain all public, instance fields
        public static FieldInfo[] GetFields(Type newBaseType)
        {
            // Results
            var results = newBaseType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            // Sort parent class fields to top
            Array.Sort(results, (f1, f2) =>
            {
                if (f1.DeclaringType != f2.DeclaringType)
                {
                    if (f1.DeclaringType == newBaseType) return 1;
                    if (f2.DeclaringType == newBaseType) return -1;
                }

                return 0;
            });

            // Return results
            return results;
        }

        // Gui Layout
        public void OnGuiLayout(SerializedObject serializedObject)
        {
            // Ignore without object
            if (serializedObject == null || serializedObject.targetObject == null) return;
            // Attempt a setup if needed
            var desType = serializedObject.targetObject.GetType();
            if (baseType != desType || fields == null) RefreshFields(desType);
            // Ignore
            if (fields == null) return;

            // Iterate all fields
            foreach (var field in fields)
            {
                // Custom handle
                if (onCustomGuiLayout != null && onCustomGuiLayout(field)) continue;

                // Default layout
                var property = serializedObject.FindProperty(field.Name);
                EditorGUILayout.PropertyField(property);
            }

            // Additional items
            onAdditionalGuiLayout?.Invoke();

            // Apply
            serializedObject.ApplyModifiedProperties();
        }
    }
}
