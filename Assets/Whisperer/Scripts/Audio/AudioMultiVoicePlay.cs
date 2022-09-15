/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using UnityEngine;

namespace Whisperer
{
	public class AudioMultiVoicePlay : MonoBehaviour
	{
		[SerializeField] AudioSource _audioSource;
		[SerializeField] AudioClip[] _clips;
		[SerializeField] AutoPlay _autoPlay;
		[SerializeField] Vector2 _autoMinMaxTime = new Vector2(5,10);

		int clipIndex = -1;
		AudioClip nextClip;
		enum AutoPlay { None, Random, Sequential }

		private void Awake()
		{
			if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
		}

		private void OnEnable()
		{
			if (_autoPlay != AutoPlay.None)
				StartCoroutine(AutoPlayRoutine());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}

		public void PlayRandomClip()
		{
			if (_clips.Length == 0) return;

			clipIndex = Random.Range(0, _clips.Length);
			nextClip = _clips[clipIndex];

			Play();
		}

		public void PlayNextClip()
		{
			if (_clips.Length == 0) return;

			clipIndex = (clipIndex + 1) % _clips.Length;
			nextClip = _clips[clipIndex];

			Play();
		}

		private void Play()
		{
			_audioSource.PlayOneShot(nextClip);
		}

		IEnumerator AutoPlayRoutine()
		{
			while (_autoPlay != AutoPlay.None)
			{
				yield return new WaitForSeconds(Random.Range(_autoMinMaxTime.x, _autoMinMaxTime.y));

				if (_autoPlay == AutoPlay.Random)
					PlayRandomClip();
				else
					PlayNextClip();
			}
		}
	}
}