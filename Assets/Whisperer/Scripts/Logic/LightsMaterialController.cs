/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{
	public class LightsMaterialController : MonoBehaviour
	{
		[SerializeField] List<Renderer> _renderers;
		[SerializeField] float _startValue = 0;

		private void Awake()
		{
			SetLights(_startValue);
		}

		public void SetLights(float intensity)
		{
			_renderers.ForEach(r => r.material.SetFloat("_LightIntensity", intensity));
		}
	}
}
