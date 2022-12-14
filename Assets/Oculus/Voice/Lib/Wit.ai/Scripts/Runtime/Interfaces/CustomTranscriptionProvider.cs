/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Facebook.WitAi.Events;
using UnityEngine;
using UnityEngine.Events;

namespace Facebook.WitAi.Interfaces
{
    public abstract class CustomTranscriptionProvider : MonoBehaviour, ITranscriptionProvider
    {
        [SerializeField] private bool overrideMicLevel;

        public string LastTranscription { get; }
        public WitTranscriptionEvent OnPartialTranscription { get; } = new();

        public WitTranscriptionEvent OnFullTranscription { get; } = new();

        public UnityEvent OnStoppedListening { get; } = new();

        public UnityEvent OnStartListening { get; } = new();

        public WitMicLevelChangedEvent OnMicLevelChanged { get; } = new();

        public bool OverrideMicLevel => overrideMicLevel;

        public abstract void Activate();
        public abstract void Deactivate();
    }
}
