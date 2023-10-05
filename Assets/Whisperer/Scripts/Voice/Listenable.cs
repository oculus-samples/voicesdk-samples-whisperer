/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using System.Collections.Generic;
using Meta.WitAi;
using Meta.WitAi.Json;
using Oculus.Voice;
using UnityEngine;
using UnityEngine.Events;

namespace Whisperer
{
    public class ListenableEventArgs
    {
        public string Action;
        public bool Error;
        public Listenable Listenable;
        public bool Success;
        public WitResponseNode WitResponseNode;

        public ListenableEventArgs(Listenable listenable, WitResponseNode witResponseNode, string action, bool success,
            bool error = false)
        {
            Listenable = listenable;
            WitResponseNode = witResponseNode;
            Action = action;
            Success = success;
            Error = error;
        }
    }

    /// <summary>
    ///     An object that can be selected by the SpeakGestureWatcher, subscribe and respond to Wit
    /// </summary>
    public abstract class Listenable : MonoBehaviour
    {
        public Transform FollowTransformOverride;
        public UtteranceTooltipDefinition TooltipDefinition;
        public List<Renderer> _shimmerRends;

        [SerializeField] protected bool _listenActive = true;
        [SerializeField] protected bool _selected;
        [SerializeField] protected bool _actionState = true;

        [HideInInspector] public UnityEvent<ListenableEventArgs> OnResponseProcessed;
        [HideInInspector] public UnityEvent<Listenable> OnWitSubscribed;
        [HideInInspector] public UnityEvent<Listenable> OnListeningDisabled;
        [HideInInspector] public UnityEvent<Listenable> OnDestroyed;

        protected AppVoiceExperience _appVoiceExperience;
        protected Color _colorSelected = new Color32(0, 255, 59, 255);
        protected HighlightObject _outline;
        protected float _timeoutDelay = 5f;
        protected IEnumerator _timeoutRoutine;

        protected bool _tooltipEnabled = true,
            _subscribed,
            _allowSelect = true,
            _highlightOnly;

        protected WitResponseNode _witResponseNode;
        protected VoiceUI _witUI;
        public bool AllowSelect => _listenActive && _allowSelect;
        public bool IsSelected => _selected;

        public bool IsActionable
        {
            get => _actionState;
            set => _actionState = value;
        }

        public bool TooltipEnabled
        {
            get => _tooltipEnabled;
            set => _tooltipEnabled = value;
        }

        public bool HighlightOnly => _highlightOnly;

        protected virtual void Start()
        {
            _appVoiceExperience = FindObjectOfType<AppVoiceExperience>();

            GetShimmerRenderers();
            SetHighlight(false);
            SetListeningActive(_listenActive);
        }

        protected virtual void OnDestroy()
        {
            if (_witUI)
                Destroy(_witUI.gameObject);

            OnListeningDisabled.Invoke(this);
            OnDestroyed.Invoke(this);
        }

        public void ProcessComplete(string intent, bool success, bool error = false)
        {
            var debug = "";
            debug += "Process Complete: " + gameObject.name;
            debug += " | Action: " + intent;
            debug += " | Success: " + (error ? "error" : success);
            debug += " | Transcription: " + _witUI.LastTranscriptionCache;

            //Debug.Log(debug);
            Logger.Instance.AddLog(debug);

            var eventArgs = new ListenableEventArgs(this, _witResponseNode, intent, success, error);
            OnResponseProcessed.Invoke(eventArgs);

            _witUI.SetUIState(intent, success, error);
        }

        public virtual void SetListeningActive(bool active)
        {
            _listenActive = active;

            if (!_listenActive) OnListeningDisabled.Invoke(this);

            SetShimmerEnabled(active);
        }

        public virtual void InstantiateWitUI(GameObject prefab)
        {
            if (_witUI == null)
            {
                var go_speechTranscriptionUI = Instantiate(prefab);
                go_speechTranscriptionUI.name = transform.name + "_witUI";
                _witUI = go_speechTranscriptionUI.GetComponent<VoiceUI>();

                _witUI.SetFollowTransform(FollowTransformOverride == null ? transform : FollowTransformOverride);
                _witUI.Listenable = this;
            }
        }

        public virtual void SetSelected(bool selected)
        {
            if (!_listenActive) selected = false;
            _selected = selected;

            SetHighlight(selected);

            if (_highlightOnly) return;

            if (selected)
            {
                SetSubscribed(true);
            }
            else
            {
                SetSubscribed(false);
                _appVoiceExperience.DeactivateAndAbortRequest();
            }
        }

        public void SetHighlightOnlyMode(bool set)
        {
            _highlightOnly = set;

            if (!_highlightOnly)
                SetSelected(_selected);
        }

        protected virtual void DetermineAction(WitResponseNode witResponse)
        {
        }

        protected virtual void HandleAction(string action)
        {
        }

        protected void HandleResponse(WitResponseNode witResponse)
        {
            _witResponseNode = witResponse;

            var data = witResponse.GetFirstIntentData();
            var intent = data == null ? "[null]" : data.name;
            Logger.Instance.AddLog("Handle response: " + gameObject.name + " | Intent: " + intent);

            if (data == null)
            {
                ProcessComplete("", false);
            }

            if (_actionState)
                DetermineAction(witResponse);
            else
                ProcessComplete("", true);
        }

        protected void OnMinimumWakeThresholdHit()
        {
            RestartTimer();
        }

        private void SetSubscribed(bool sub)
        {
            if (sub && !_subscribed)
            {
                _witUI?.SetVisible(true);

                _appVoiceExperience?.VoiceEvents.OnMinimumWakeThresholdHit.AddListener(OnMinimumWakeThresholdHit);
                _appVoiceExperience?.VoiceEvents.OnResponse.AddListener(HandleResponse);
                _appVoiceExperience?.VoiceEvents.OnStoppedListeningDueToInactivity.AddListener(
                    HandleWitFailDueToInactivity);
                _appVoiceExperience?.VoiceEvents.OnStoppedListeningDueToTimeout.AddListener(HandleWitFailDueToTimeout);
                _appVoiceExperience?.VoiceEvents.OnAborted.AddListener(HandleWitFailOnAborted);
                _appVoiceExperience?.VoiceEvents.OnError.AddListener(HandleWitFailOnError);
                _appVoiceExperience?.VoiceEvents.OnFullTranscription.AddListener(LogTranscription);

                _subscribed = true;

                OnWitSubscribed.Invoke(this);
            }
            else if (!sub && _subscribed)
            {
                _witUI?.SetVisible(false);

                _appVoiceExperience?.VoiceEvents.OnMinimumWakeThresholdHit.RemoveListener(OnMinimumWakeThresholdHit);
                _appVoiceExperience?.VoiceEvents.OnResponse.RemoveListener(HandleResponse);
                _appVoiceExperience?.VoiceEvents.OnStoppedListeningDueToInactivity.RemoveListener(
                    HandleWitFailDueToInactivity);
                _appVoiceExperience?.VoiceEvents.OnStoppedListeningDueToTimeout.RemoveListener(
                    HandleWitFailDueToTimeout);
                _appVoiceExperience?.VoiceEvents.OnAborted.RemoveListener(HandleWitFailOnAborted);
                _appVoiceExperience?.VoiceEvents.OnError.RemoveListener(HandleWitFailOnError);
                _appVoiceExperience?.VoiceEvents.OnFullTranscription.RemoveListener(LogTranscription);

                if (_timeoutRoutine != null) StopCoroutine(_timeoutRoutine);

                _subscribed = false;
            }
        }


        protected void HandleWitFailOnError(string arg0, string arg1)
        {
            ProcessComplete("Wit Failed: OnError " + arg0 + " | " + " | " + arg1, false);
        }

        protected void HandleWitFailDueToInactivity()
        {
            ProcessComplete("Wit Failed: Due To Inactivity", false);
        }

        protected void HandleWitFailDueToTimeout()
        {
            ProcessComplete("Wit Failed: Due To Timeout", false);
        }

        protected void HandleWitFailOnAborted()
        {
            ProcessComplete("Wit Failed: Aborted", false);
        }

        protected void RestartTimer()
        {
            if (_timeoutRoutine != null) StopCoroutine(_timeoutRoutine);
            _timeoutRoutine = WitTimeout();
            StartCoroutine(_timeoutRoutine);
        }

        protected IEnumerator WitTimeout()
        {
            yield return new WaitForSeconds(_timeoutDelay);
            ProcessComplete("timeout", false, true);
        }

        protected void LogTranscription(string transcription)
        {
            var log = "Selected listenable: " + gameObject.name + " | Transcription: " + transcription;
            Logger.Instance.AddLog(log);
        }

        #region Highlighting

        protected virtual void SetHighlight(bool enable)
        {
            _outline = GetComponentInChildren<HighlightObject>();

            if (_outline != null)
            {
                _outline.HighlightColor = _colorSelected;
                _outline.EnableHighlight(enable);
            }
            else
            {
                Debug.LogWarning("Outline is null");
            }
        }

        [ContextMenu("Enable Shimmer")]
        protected void EnableShimmer()
        {
            SetShimmerEnabled(true);
        }

        [ContextMenu("Disable Shimmer")]
        protected void DisableShimmer()
        {
            SetShimmerEnabled(false);
        }

        protected void SetShimmerEnabled(bool enabled)
        {
            //Debug.Log("SetShimmerEnabled: " + enabled + " on " + gameObject.name, gameObject);
            foreach (var rend in _shimmerRends)
            {
                if (rend == null) return;

                var shimmerSpeed = enabled ? 0.3f : 0f; // prob should cache that 0.3 at start
                foreach (var mat in rend.materials) mat.SetFloat("_Shimmer_Speed", shimmerSpeed);
            }
        }

        protected void GetShimmerRenderers()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            foreach (var mat in rend.materials)
                if (mat.shader.name == "Shader Graphs/Shimmer" ||
                    mat.shader.name == "Shader Graphs/Toon_Projection_Shimmer" ||
                    mat.shader.name == "Shader Graphs/Toon_AnimatedPlants"
                   )
                    _shimmerRends.Add(rend);

            foreach (var rend in _shimmerRends)
                InitShimmer(rend);
        }

        protected void InitShimmer(Renderer rend)
        {
            foreach (var mat in rend.materials)
            {
                // Set a random time offeset
                mat.SetFloat("_Shimmer_Time_Offset", Random.Range(0, 3f));
                mat.SetFloat("_Shimmer_Interval", Random.Range(2f, 4f));
                rend.material.SetFloat("_Shimmer_Speed", _listenActive ? 0.3f : 0f);
            }
        }

        #endregion
    }
}
