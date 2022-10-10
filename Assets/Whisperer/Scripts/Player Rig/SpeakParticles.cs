/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
	public class SpeakParticles : MonoBehaviour
	{
		[SerializeField] SpeakGestureWatcher _speakGestureWatcher;
		[SerializeField] ParticleSystem _ps;
		[SerializeField] MicInputValue _micInputValue;
		[SerializeField] float _threshold;

		ParticleSystem.EmissionModule _em;
		float _value;

		void Start()
		{
			if (_ps is null) _ps = GetComponent<ParticleSystem>();
			_em = _ps.emission;
		}

		private void Update()
		{
			_value = _speakGestureWatcher.HaveSpeakPose ? _micInputValue.AudioLevel * 100 : 0;
			SetEmissionEnabled(_value > _threshold);
		}

		public void SetEmissionEnabled(bool enable)
		{
			_em.enabled = enable;
		}
	}
}
