/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using UnityEngine.Events;

namespace Whisperer
{
	public class Destructable : MonoBehaviour
	{
		public Transform brokenPrefab;
		public float velocityTrigger;

		public UnityEvent OnDestruct;

		private bool _destructable = true;

		void OnCollisionEnter(Collision collision)
		{
			if (!_destructable)
				return;

			if (collision.relativeVelocity.magnitude > velocityTrigger)
			{
				_destructable = false;

				OnDestruct.Invoke();
				Quaternion rotation = transform.rotation;
				Vector3 position = transform.position;
				Transform t = Instantiate(brokenPrefab, position, rotation);
				t.localScale = transform.localScale;
				Destroy(gameObject);
			}
		}
	}
}