/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
	public class FogController : MonoBehaviour
	{
		[SerializeField] Vector2 _fogDensity;
		[SerializeField] Color _color;
		Progress _fader;

		Color _orig;

		private void Start()
		{
			_fader = new Progress(Fade);
			_orig = RenderSettings.fogColor;
		}

		public void FadeIn(float duration)
		{
			_fader.Play(duration);
		}

		public void Reset()
		{
			_fader.ResetTo0();
		}

		private void Fade(float progress)
		{
			RenderSettings.fogDensity = Mathf.Lerp(_fogDensity.x, _fogDensity.y, progress);
			RenderSettings.fogColor = Color.Lerp(_orig, _color, progress);
		}
	}
}