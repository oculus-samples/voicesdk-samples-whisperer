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
        [SerializeField] private float _speed = 1;
        [SerializeField] private float _rSpd = 1;
        [SerializeField] private List<Transform> _transforms;
        [SerializeField] private float _explRad = 2;
        [SerializeField] private float _power = 10;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private float _maxTime = 5;

        public SpeakGestureWatcher SGW { get; set; }

        private void Start()
        {
            Invoke("Explode", _maxTime);
        }

        private void Update()
        {
            if (SGW) transform.forward = SGW.RaycastDirection;

            transform.Translate(Vector3.forward * Time.deltaTime * _speed);

            _transforms.ForEach(t => { t.Rotate(Vector3.up * Time.deltaTime * _rSpd, Space.Self); });
        }

        private void OnCollisionEnter(Collision collision)
        {
            CancelInvoke();

            var explosionPos = transform.position;
            var colliders = Physics.OverlapSphere(explosionPos, _explRad);
            foreach (var hit in colliders)
            {
                var rb = hit.GetComponent<Rigidbody>();

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
