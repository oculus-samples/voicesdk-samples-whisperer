/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
	public class AudioPlay : MonoBehaviour
	{
		[SerializeField] Transform _positionTransform;
		[SerializeField] string _audioClip;
		[SerializeField] float _volume = 1;
		[SerializeField] bool _playOnAwake;

		private void OnEnable()
		{
			if (_positionTransform == null) _positionTransform = transform;

			if (enabled && _playOnAwake) Play();
		}

		public void Play()
		{
			AudioManager.Instance.Play(_audioClip, _positionTransform, _volume);
		}

		public void Stop()
		{
			AudioManager.Instance.Stop(_audioClip);
		}
	}
}

