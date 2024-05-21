/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using UnityEngine.InputSystem;

namespace Whisperer
{
        /// <summary>
        ///     Allows user control of XR Rig height offset
        /// </summary>
        public class RigHeightControl : MonoBehaviour
    {
        [SerializeField] private Transform _offsetTransform;
        [SerializeField] private InputActionReference _inputAxis;
        [SerializeField] private Vector2 _worldMinMax;
        [SerializeField] private float _maxSpeed;
        [SerializeField] private float _easeTime;
        [SerializeField] private float _startHeight;

        private float _target,
            _current;

        private void Start()
        {
            _offsetTransform.position =
                new Vector3(_offsetTransform.position.x, _startHeight, _offsetTransform.position.z);
        }

        private void LateUpdate()
        {
            _target = _inputAxis.action.ReadValue<Vector2>().y;

            if (_target < 0 && _offsetTransform.position.y < _worldMinMax.x) _target = 0;
            if (_target > 0 && _offsetTransform.position.y > _worldMinMax.y) _target = 0;

            _current = Mathf.MoveTowards(_current, _target, Time.deltaTime * _easeTime);
            _offsetTransform.Translate(Vector3.up * _current * Time.deltaTime * _maxSpeed);
        }
    }
}
