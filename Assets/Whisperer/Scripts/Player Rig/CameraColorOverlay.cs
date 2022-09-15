/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
	public class CameraColorOverlay : MonoBehaviour
	{
		[Header("Camera Color Overlay")]
		[SerializeField] float _zOffset = 0.001f;
		[SerializeField] Renderer _rend;
		[SerializeField] Easings.Functions _easing;
		[SerializeField] Color _fadeColor;

		Color _clearColor;
		Progress _colorFader;

		private void Awake()
		{
			_colorFader = new Progress(UpdateOverlay);
			_clearColor.a = 0;
		}

		private void Start()
		{
			transform.localPosition = new Vector3(0, 0, Camera.main.nearClipPlane + _zOffset);
			transform.localRotation = Quaternion.identity;

			LevelLoader.Instance.OnLevelLoadComplete.AddListener(LevelLoadComplete);
			LevelLoader.Instance.OnLevelWillBeginUnload.AddListener(LevelWillUnload);
		}

		private void OnDestroy()
		{
			LevelLoader.Instance.OnLevelLoadComplete.RemoveListener(LevelLoadComplete);
			LevelLoader.Instance.OnLevelWillBeginUnload.RemoveListener(LevelWillUnload);
		}

		private void LevelWillUnload(float delay)
		{
			FadeToColor(_fadeColor, delay / 2);
		}

		private void LevelLoadComplete()
		{
			FadeToClear(2);
		}

		public void SetTargetColor(Color color)
		{
			_fadeColor = color;
		}

		public void SetColor(Color color)
		{
			_fadeColor = color;
			_rend.material.color = color;		
			_rend.enabled = _rend.material.color.a != 0;
		}

		public void SetClear()
		{
			_clearColor = _fadeColor;
			_clearColor.a = 0;

			_rend.material.color = _clearColor;
			_rend.enabled = false;
		}

		public void FadeToColor(Color color, float duration)
		{
			_fadeColor = color;
			_colorFader.Play(duration);
		}

		public void FadeToClear(float duration)
		{
			_clearColor = _fadeColor;
			_clearColor.a = 0;
			_colorFader.PlayReverse(duration);
		}

		private void UpdateOverlay(float p)
		{
			_rend.material.color = Color.Lerp(_clearColor, _fadeColor, Easings.Interpolate(p, _easing));
			_rend.enabled = _rend.material.color.a != 0;
		}
	}
}