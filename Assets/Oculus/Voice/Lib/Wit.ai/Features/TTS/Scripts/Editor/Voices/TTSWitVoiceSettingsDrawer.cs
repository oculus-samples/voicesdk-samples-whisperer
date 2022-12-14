/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using System.Reflection;
using Facebook.WitAi.TTS.Integrations;
using Facebook.WitAi.Windows;
using UnityEditor;
using UnityEngine;

namespace Facebook.WitAi.TTS.Editor.Voices
{
    [CustomPropertyDrawer(typeof(TTSWitVoiceSettings))]
    public class TTSWitVoiceSettingsDrawer : PropertyDrawer
    {
        // Constants for var layout
        private const float VAR_HEIGHT = 20f;
        private const float VAR_MARGIN = 4f;

        // Constants for var lookup
        private const string VAR_SETTINGS = "settingsID";
        private const string VAR_VOICE = "voice";
        private const string VAR_STYLE = "style";

        // Subfields
        private static readonly FieldInfo[] _fields = FieldGUI.GetFields(typeof(TTSWitVoiceSettings));

        // Determine height
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Property
            if (!property.isExpanded) return VAR_HEIGHT;
            // Add each
            var total = _fields.Length + 1;
            var voiceIndex = GetVoiceIndex(property);
            if (voiceIndex != -1) total += 2;
            return total * VAR_HEIGHT + Mathf.Max(0, total - 1) * VAR_MARGIN;
        }

        // Handles gui layout
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // On gui
            var y = position.y;
            var voiceName = property.FindPropertyRelative(VAR_SETTINGS).stringValue;
            property.isExpanded =
                EditorGUI.Foldout(new Rect(position.x, y, position.width, VAR_HEIGHT), property.isExpanded, voiceName);
            if (!property.isExpanded) return;
            y += VAR_HEIGHT + VAR_MARGIN;

            // Increment
            EditorGUI.indentLevel++;

            // Get voice index
            var voiceIndex = GetVoiceIndex(property);

            // Iterate subfields
            for (var s = 0; s < _fields.Length; s++)
            {
                var subfield = _fields[s];
                var subfieldProperty = property.FindPropertyRelative(subfield.Name);
                var subfieldRect = new Rect(position.x, y, position.width, VAR_HEIGHT);
                if (string.Equals(subfield.Name, VAR_VOICE) && voiceIndex != -1)
                {
                    var newVoiceIndex = EditorGUI.Popup(subfieldRect, subfieldProperty.displayName, voiceIndex,
                        TTSWitVoiceUtility.VoiceNames.ToArray());
                    newVoiceIndex = Mathf.Clamp(newVoiceIndex, 0, TTSWitVoiceUtility.VoiceNames.Count);
                    if (voiceIndex != newVoiceIndex)
                    {
                        voiceIndex = newVoiceIndex;
                        subfieldProperty.stringValue = TTSWitVoiceUtility.VoiceNames[voiceIndex];
                        GUI.FocusControl(null);
                    }

                    y += VAR_HEIGHT + VAR_MARGIN;
                    continue;
                }

                if (string.Equals(subfield.Name, VAR_STYLE) && voiceIndex >= 0 &&
                    voiceIndex < TTSWitVoiceUtility.Voices.Length)
                {
                    // Get voice data
                    var voiceData = TTSWitVoiceUtility.Voices[voiceIndex];
                    EditorGUI.indentLevel++;

                    // Locale layout
                    EditorGUI.LabelField(subfieldRect, "Locale", voiceData.locale);
                    y += VAR_HEIGHT + VAR_MARGIN;

                    // Gender layout
                    subfieldRect = new Rect(position.x, y, position.width, VAR_HEIGHT);
                    EditorGUI.LabelField(subfieldRect, "Gender", voiceData.gender);
                    y += VAR_HEIGHT + VAR_MARGIN;

                    // Style layout/select
                    subfieldRect = new Rect(position.x, y, position.width, VAR_HEIGHT);
                    if (voiceData.styles != null && voiceData.styles.Length > 0)
                    {
                        // Get style index
                        var style = subfieldProperty.stringValue;
                        var styleIndex = new List<string>(voiceData.styles).IndexOf(style);

                        // Show style select
                        var newStyleIndex = EditorGUI.Popup(subfieldRect, subfieldProperty.displayName, styleIndex,
                            voiceData.styles);
                        newStyleIndex = Mathf.Clamp(newStyleIndex, 0, voiceData.styles.Length);
                        if (styleIndex != newStyleIndex)
                        {
                            // Apply style
                            styleIndex = newStyleIndex;
                            subfieldProperty.stringValue = voiceData.styles[styleIndex];
                            GUI.FocusControl(null);
                        }

                        // Move down
                        y += VAR_HEIGHT + VAR_MARGIN;
                        EditorGUI.indentLevel--;
                        continue;
                    }

                    // Undent
                    EditorGUI.indentLevel--;
                }

                // Default layout
                EditorGUI.PropertyField(subfieldRect, subfieldProperty, new GUIContent(subfieldProperty.displayName));

                // Clamp in between range
                var range = subfield.GetCustomAttribute<RangeAttribute>();
                if (range != null)
                {
                    var newValue = Mathf.Clamp(subfieldProperty.intValue, (int)range.min, (int)range.max);
                    if (subfieldProperty.intValue != newValue) subfieldProperty.intValue = newValue;
                }

                // Increment
                y += VAR_HEIGHT + VAR_MARGIN;
            }

            // Undent
            EditorGUI.indentLevel--;
        }

        // Get voice index
        private int GetVoiceIndex(SerializedProperty property)
        {
            var voiceProperty = property.FindPropertyRelative(VAR_VOICE);
            var voiceID = voiceProperty.stringValue;
            var voiceIndex = -1;
            var voiceNames = TTSWitVoiceUtility.VoiceNames;
            if (voiceNames != null && voiceNames.Count > 0)
            {
                if (string.IsNullOrEmpty(voiceID))
                {
                    voiceIndex = 0;
                    voiceID = voiceNames[0];
                    voiceProperty.stringValue = voiceID;
                    GUI.FocusControl(null);
                }
                else
                {
                    voiceIndex = voiceNames.IndexOf(voiceID);
                }
            }

            return voiceIndex;
        }
    }
}
