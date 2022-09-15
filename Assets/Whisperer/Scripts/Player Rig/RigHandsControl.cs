/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Whisperer
{
	/// <summary>
	/// Control visibility and manage switching between hands controller types
	/// </summary>
	public class RigHandsControl : MonoBehaviour
	{
		[SerializeField] List<GameObject> _rayHands;
		[SerializeField] List<GameObject> _speakHands;
		[SerializeField] List<GameObject> _handsModels;
		[SerializeField] List<Transform> _transforms;
		[SerializeField] Material _material;
		[SerializeField] float _startAlpha = 0.2f;

		public List<Transform> Transforms { get => _transforms; }

		public Progress ColorFader;
		public bool Hidden { get => _hidden; }
		public bool SpeakHandsReady { get => _speakHandsReady; }

		Color _startColor,
			  _tColor;
		bool _setup,
			 _hidden,
			 _speakHandsReady;
		enum HandsSet { None, Speak, Ray }
		HandsSet _set;

		private void Awake()
		{
			_hidden = true;
		}

		private void Start()
		{
			SetHandsRenderers(false);
			_speakHands.ForEach(h => h.GetComponent<XRBaseController>().enabled = false);

			_startColor = _material.color;
			_startColor.a = _startAlpha;

			_tColor = new Color(_startColor.r, _startColor.g, _startColor.b, 0);
			ColorFader = new Progress(FadeColor);
			ColorFader.ResetTo1();
		}

		private void OnDisable()
		{
			ColorFader.ResetTo0();
		}

		public void SetRay()
		{
			ColorFader.ResetTo1();
	
			_rayHands.ForEach(o => o.SetActive(true));
			_speakHands.ForEach(o => o.SetActive(false));
			_hidden = false;
			_set = HandsSet.Ray;
		}

		public void SetSpeak()
		{
			_rayHands.ForEach(o => o.SetActive(false));
			_speakHands.ForEach(o => o.SetActive(true));
			_hidden = false;
			_set = HandsSet.Speak;

			if (!_setup) SetupHands();
			else ColorFader.PlayFrom1(2);
		}

		public void SetNone()
		{
			_rayHands.ForEach(o => o.SetActive(false));
			_speakHands.ForEach(o => o.SetActive(false));
			_hidden = true;
			_set = HandsSet.None;

			ColorFader.ResetTo1();
		}

		private void FadeColor(float p)
		{
			_material.color = Color.Lerp(_startColor, _tColor, p);

			_speakHandsReady = !_hidden && p == 0;

			if (p == 1) _hidden = true;
		}

		private void SetHandsRenderers(bool visible)
		{
			_handsModels.ForEach(m =>
			{
				List<Renderer> rends = new List<Renderer>(m.GetComponentsInChildren<Renderer>());
				rends.ForEach(r => r.enabled = visible);
			});
		}

		private void SetupHands()
		{
			_setup = true;
			Invoke("EnableSpeakControllers", 2);
		}

		private void EnableSpeakControllers()
		{
			for (int i = 0; i < 2; i++)
			{
				_handsModels[i].transform.parent = null;
				_handsModels[i].transform.parent = _speakHands[i].transform;
				_handsModels[i].transform.localPosition = Vector3.zero;
				_handsModels[i].transform.localRotation = Quaternion.identity;
			}
			_speakHands.ForEach(h => h.GetComponent<XRBaseController>().enabled = true);
			SetHandsRenderers(true);

			Invoke("FirstFade", 2);
		}

		private void FirstFade()
		{
			ColorFader.PlayFrom1(3);
		}

		private void OnApplicationFocus(bool focus)
		{
			if (!focus)
			{
				_rayHands.ForEach(o => o.SetActive(false));
				_speakHands.ForEach(o => o.SetActive(false));
			}
			else
			{
				if (_set == HandsSet.Speak)
					_speakHands.ForEach(o => o.SetActive(true));
				if (_set == HandsSet.Ray)
					_rayHands.ForEach(o => o.SetActive(true));
			}
		}
	}
}
