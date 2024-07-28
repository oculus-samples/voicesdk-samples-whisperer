/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
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

        private void OnCollisionEnter(Collision collision)
        {
            if (!_destructable)
                return;

            if (collision.relativeVelocity.magnitude > velocityTrigger)
            {
                _destructable = false;

                OnDestruct.Invoke();
                var rotation = transform.rotation;
                var position = transform.position;
                var t = Instantiate(brokenPrefab, position, rotation);
                t.localScale = transform.localScale;
                Destroy(gameObject);
            }
        }
    }
}
