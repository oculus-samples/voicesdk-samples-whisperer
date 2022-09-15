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
	public class FSpell : MonoBehaviour
	{
		[SerializeField] float _speed = 1;
		[SerializeField] float _rSpd = 1;
		[SerializeField] List<Transform> _transforms;
		[SerializeField] float _explRad = 2;
		[SerializeField] float _power = 10;
		[SerializeField] GameObject _prefab;
		[SerializeField] float _maxTime = 5;

		public SpeakGestureWatcher SGW { get; set; }

		private void Start()
		{
			Invoke("Explode", _maxTime);
		}

		private void Update()
		{
			if (SGW) transform.forward = SGW.RaycastDirection;

			transform.Translate(Vector3.forward * Time.deltaTime * _speed);

			_transforms.ForEach(t =>
			{
				t.Rotate(Vector3.up * Time.deltaTime * _rSpd, Space.Self);
			});
		}

		private void OnCollisionEnter(Collision collision)
		{
			CancelInvoke();

			Vector3 explosionPos = transform.position;
			Collider[] colliders = Physics.OverlapSphere(explosionPos, _explRad);
			foreach (Collider hit in colliders)
			{
				Rigidbody rb = hit.GetComponent<Rigidbody>();

				if (rb != null)
					rb.AddExplosionForce(_power, explosionPos, _explRad, 0.5f);
			}

			Explode();
		}

		private void Explode()
		{
			Instantiate(_prefab, transform.position, Quaternion.identity);
			Destroy(gameObject);
		}
	}
}