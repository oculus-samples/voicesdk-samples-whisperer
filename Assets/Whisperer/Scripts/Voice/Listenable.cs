/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Voice;
using Facebook.WitAi;
using Facebook.WitAi.Lib;
using Facebook.WitAi.Data.Intents;

namespace Whisperer
{
	public class ListenableEventArgs
	{
		public Listenable Listenable;
		public WitResponseNode WitResponseNode;
		public string Action;
		public bool Success;
		public bool Error;

		public ListenableEventArgs(Listenable listenable, WitResponseNode witResponseNode, string action, bool success, bool error = false)
		{
			Listenable = listenable;
			WitResponseNode = witResponseNode;
			Action = action;
			Success = success;
			Error = error;
		}
	}

	/// <summary>
	/// An object that can be selected by the SpeakGestureWatcher, subscribe and respond to Wit
	/// </summary>
	public abstract class Listenable : MonoBehaviour
	{
		public bool AllowSelect { get => _listenActive && _allowSelect; }
		public bool IsSelected { get => _selected; }
		public bool IsActionable { get => _actionState; set => _actionState = value; }
		public bool TooltipEnabled { get => _tooltipEnabled; set => _tooltipEnabled = value; }
		public bool HighlightOnly { get => _highlightOnly; }

		protected AppVoiceExperience _appVoiceExperience;
		public Transform FollowTransformOverride;
		public UtteranceTooltipDefinition TooltipDefinition;
		public List<Renderer> _shimmerRends;

		[SerializeField] protected bool _listenActive = true;
		[SerializeField] protected bool _selected;
		[SerializeField] protected bool _actionState = true;
		protected float _timeoutDelay = 5f;
		protected HighlightObject _outline;
		protected WitResponseNode _witResponseNode;
		protected VoiceUI _witUI;
		protected Color _colorSelected = new Color32(0, 255, 59, 255);
		protected IEnumerator _timeoutRoutine;
		protected bool _tooltipEnabled = true,
					   _subscribed,
					   _allowSelect = true,
					   _highlightOnly;

		[HideInInspector] public UnityEvent<ListenableEventArgs> OnResponseProcessed;
		[HideInInspector] public UnityEvent<Listenable> OnWitSubscribed;
		[HideInInspector] public UnityEvent<Listenable> OnListeningDisabled;
		[HideInInspector] public UnityEvent<Listenable> OnDestroyed;

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
			string debug = "";
			debug += "Process Complete: " + gameObject.name;
			debug += " | Action: " + intent;
			debug += " | Success: " + (error ? "error" : success);
			debug += " | Transcription: " + _witUI.LastTranscriptionCache;

			//Debug.Log(debug);
			Logger.Instance.AddLog(debug);

			ListenableEventArgs eventArgs = new ListenableEventArgs(this, _witResponseNode, intent, success, error);
			OnResponseProcessed.Invoke(eventArgs);

			_witUI.SetUIState(intent, success, error);

			SetSubscribed(false);
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
				GameObject go_speechTranscriptionUI = Instantiate(prefab);
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
				_appVoiceExperience?.VoiceEvents.OnMinimumWakeThresholdHit.AddListener(OnMinimumWakeThresholdHit);
			else
			{
				SetSubscribed(false);
				_appVoiceExperience?.VoiceEvents.OnMinimumWakeThresholdHit.RemoveListener(OnMinimumWakeThresholdHit);
			}
		}

		public void SetHighlightOnlyMode(bool set)
		{
			_highlightOnly = set;

			if (!_highlightOnly)
				SetSelected(_selected);
		}

		protected abstract void DetermineAction(WitResponseNode witResponse);

		protected virtual void HandleAction(string action) { }

		protected void HandleResponse(WitResponseNode witResponse)
		{
			_witResponseNode = witResponse;

			WitIntentData data = witResponse.GetFirstIntentData();
			string intent = data == null ? "[null]" : data.name;
			Logger.Instance.AddLog("Handle response: " + gameObject.name + " | Intent: " + intent);

			if (_actionState)
			{

				DetermineAction(witResponse);
			}
			else
			{
				ProcessComplete("", true);
			}
		}

		protected void OnMinimumWakeThresholdHit()
		{
			SetSubscribed(true);
			RestartTimer();
		}

		protected void SetSubscribed(bool sub)
		{
			if (sub && !_subscribed)
			{
				_witUI?.SetVisible(true);

				_appVoiceExperience?.VoiceEvents.OnResponse.AddListener(HandleResponse);
				_appVoiceExperience?.VoiceEvents.OnStoppedListeningDueToInactivity.AddListener(HandleWitFailDueToInactivity);
				_appVoiceExperience?.VoiceEvents.OnStoppedListeningDueToTimeout.AddListener(HandleWitFailDueToTimeout);
				_appVoiceExperience?.VoiceEvents.OnAborted.AddListener(HandleWitFailOnAborted);
				_appVoiceExperience?.VoiceEvents.OnError.AddListener(HandleWitFailOnError);
				_appVoiceExperience?.VoiceEvents.onFullTranscription.AddListener(LogTranscription);

				_subscribed = true;

				OnWitSubscribed.Invoke(this);
			}
			else if (!sub && _subscribed)
			{
				_witUI?.SetVisible(false);

				_appVoiceExperience?.VoiceEvents.OnResponse.RemoveListener(HandleResponse);
				_appVoiceExperience?.VoiceEvents.OnStoppedListeningDueToInactivity.RemoveListener(HandleWitFailDueToInactivity);
				_appVoiceExperience?.VoiceEvents.OnStoppedListeningDueToTimeout.RemoveListener(HandleWitFailDueToTimeout);
				_appVoiceExperience?.VoiceEvents.OnAborted.RemoveListener(HandleWitFailOnAborted);
				_appVoiceExperience?.VoiceEvents.OnError.RemoveListener(HandleWitFailOnError);
				_appVoiceExperience?.VoiceEvents.onFullTranscription.RemoveListener(LogTranscription);

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
			string log = "Selected listenable: " + gameObject.name + " | Transcription: " + transcription;
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
				Debug.LogWarning("Outline is null");
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
			foreach (Renderer rend in _shimmerRends)
			{
				if (rend == null) return;

				float shimmerSpeed = enabled ? 0.3f : 0f; // prob should cache that 0.3 at start
				foreach (Material mat in rend.materials)
				{
					mat.SetFloat("_Shimmer_Speed", shimmerSpeed);
				}
			}
		}

		protected void GetShimmerRenderers()
		{
			Renderer[] renderers = GetComponentsInChildren<Renderer>();
			foreach (Renderer rend in renderers)
			{
				foreach (Material mat in rend.materials)
				{
					if (mat.shader.name == "Shader Graphs/Shimmer" ||
						mat.shader.name == "Shader Graphs/Toon_Projection_Shimmer" ||
						mat.shader.name == "Shader Graphs/Toon_AnimatedPlants"
						)
					{
						_shimmerRends.Add(rend);
					}
				}
			}

			foreach (Renderer rend in _shimmerRends)
				InitShimmer(rend);
		}

		protected void InitShimmer(Renderer rend)
		{
			foreach (Material mat in rend.materials)
			{
				// Set a random time offeset
				mat.SetFloat("_Shimmer_Time_Offset", UnityEngine.Random.Range(0, 3f));
				mat.SetFloat("_Shimmer_Interval", UnityEngine.Random.Range(2f, 4f));
				rend.material.SetFloat("_Shimmer_Speed", _listenActive ? 0.3f : 0f);
			}
		}

		#endregion
	}
}