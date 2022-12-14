/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using Facebook.WitAi.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Facebook.WitAi.Events
{
    [Serializable]
    public class VoiceEvents : ITranscriptionEvent, IAudioInputEvents
    {
        [Header("Activation Result Events")] [Tooltip("Called when a response from Wit.ai has been received")]
        public WitResponseEvent OnResponse = new();

        [Tooltip("Called when response from Wit.ai has been received from partial transcription")] [HideInInspector]
        public WitResponseEvent OnPartialResponse = new();

        [Tooltip(
            "Called after an on partial response to validate data.  If data.validResponse is true, service will deactivate & use the partial data as final")]
        public WitValidationEvent OnValidatePartialResponse = new();

        [Tooltip(
            "Called when there was an error with a WitRequest  or the RuntimeConfiguration is not properly configured.")]
        public WitErrorEvent OnError = new();

        [Tooltip(
            "Called when the activation is about to be aborted by a direct user interaction.")]
        public UnityEvent OnAborting = new();

        [Tooltip(
            "Called when the activation stopped because the network request was aborted. This can be via a timeout or call to AbortActivation.")]
        public UnityEvent OnAborted = new();

        [Tooltip(
            "Called when a a request has completed and all response and error callbacks have fired.")]
        public UnityEvent OnRequestCompleted = new();

        [Header("Mic Events")] [Tooltip("Called when the volume level of the mic input has changed")]
        public WitMicLevelChangedEvent OnMicLevelChanged = new();

        /// <summary>
        ///     Called when a request is created. This happens at the beginning of
        ///     an activation before the microphone is activated (if in use).
        /// </summary>
        [Header("Activation/Deactivation Events")]
        [Tooltip(
            "Called when a request is created. This happens at the beginning of an activation before the microphone is activated (if in use)")]
        public WitRequestCreatedEvent OnRequestCreated = new();

        [Tooltip(
            "Called when the microphone has started collecting data collecting data to be sent to Wit.ai. There may be some buffering before data transmission starts.")]
        public UnityEvent OnStartListening = new();

        [Tooltip(
            "Called when the voice service is no longer collecting data from the microphone")]
        public UnityEvent OnStoppedListening = new();

        [Tooltip(
            "Called when the microphone input volume has been below the volume threshold for the specified duration and microphone data is no longer being collected")]
        public UnityEvent OnStoppedListeningDueToInactivity = new();

        [Tooltip(
            "The microphone has stopped recording because maximum recording time has been hit for this activation")]
        public UnityEvent OnStoppedListeningDueToTimeout = new();

        [Tooltip("The Deactivate() method has been called ending the current activation.")]
        public UnityEvent OnStoppedListeningDueToDeactivation = new();

        [Tooltip(
            "Fired when recording stops, the minimum volume threshold was hit, and data is being sent to the server.")]
        public UnityEvent OnMicDataSent = new();

        [Tooltip("Fired when the minimum wake threshold is hit after an activation")]
        public UnityEvent OnMinimumWakeThresholdHit = new();

        [Header("Transcription Events")]
        [FormerlySerializedAs("OnPartialTranscription")]
        [Tooltip("Message fired when a partial transcription has been received.")]
        public WitTranscriptionEvent onPartialTranscription = new();

        [FormerlySerializedAs("OnFullTranscription")]
        [Tooltip("Message received when a complete transcription is received.")]
        public WitTranscriptionEvent onFullTranscription = new();

        [Header("Data")] public WitByteDataEvent OnByteDataReady = new();

        public WitByteDataEvent OnByteDataSent = new();

        #region Shared Event API - Transcription

        public WitTranscriptionEvent OnPartialTranscription => onPartialTranscription;
        public WitTranscriptionEvent OnFullTranscription => onFullTranscription;

        #endregion

        #region Shared Event API - Audio Input

        public WitMicLevelChangedEvent OnMicAudioLevelChanged => OnMicLevelChanged;
        public UnityEvent OnMicStartedListening => OnStartListening;
        public UnityEvent OnMicStoppedListening => OnStoppedListening;

        #endregion
    }
}
