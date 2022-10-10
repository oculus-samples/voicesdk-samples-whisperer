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
	public class Checkmark : MonoBehaviour
	{
		[SerializeField] float _speed = 1;
		[SerializeField] Vector2 _progLerp = new Vector2(0.4f, 1.5f);
		[SerializeField] float _audioSync = 0.1f;
		Material _material;
		Progress _anim;

		public bool IsChecked { get => _anim.Value == 1; }

		private void Start()
		{
			_material = GetComponent<Renderer>().material;
			SetChecked(false);

			_anim = new Progress((p) => _material.SetFloat("_Progress", Mathf.Lerp(_progLerp.x, _progLerp.y, p)));
		}

		public void SetChecked(bool check)
		{
			if (check)
			{
				_anim.PlayFrom0(_speed);
				StartCoroutine(PlaySound());
			}
			else _material.SetFloat("_Progress", 0);
		}

		private IEnumerator PlaySound()
		{
			yield return new WaitForSeconds(_audioSync);
			AudioManager.Instance.Play("ChalkX", transform);
		}

		[ContextMenu("test")]
		public void Test()
		{
			SetChecked(true);
		}
	}
}