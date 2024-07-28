/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
    /// <summary>
    ///     Simple object rotation animation
    /// </summary>
    public class Rotate : MonoBehaviour
    {
        [SerializeField] private Vector3 amount;

        public float timemult = 1;

        private void Update()
        {
            transform.Rotate(new Vector3(amount.x, amount.y, amount.z) * Time.deltaTime * timemult);
        }
    }
}
