/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
	public class MaterialFader : MonoBehaviour
	{
		[SerializeField] string _parameterName;
		[SerializeField] Renderer _renderer;
		[SerializeField] float _materialFadeTime = 2;

		Progress _materialFader;
		Material _material;

		private void Start()
		{
			_material = _renderer.material;
			_materialFader = new Progress(SetMaterial);
			_materialFader.Play(0);
		}

		private void SetMaterial(float progress)
		{
			_material.SetFloat(_parameterName, progress);
		}

		[ContextMenu("FadeDown")]
		public void FadeDown()
		{
			_materialFader.PlayFrom1(_materialFadeTime);
		}

		[ContextMenu("FadeUp")]
		public void FadeUp()
		{
			_materialFader.PlayFrom0(_materialFadeTime);
		}
	}
}
