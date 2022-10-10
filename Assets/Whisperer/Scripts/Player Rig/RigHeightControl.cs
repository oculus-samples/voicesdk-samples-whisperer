/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using UnityEngine.InputSystem;

namespace Whisperer
{
	/// <summary>
	/// Allows user control of XR Rig height offset 
	/// </summary>
	public class RigHeightControl : MonoBehaviour
	{
		[SerializeField] Transform _offsetTransform;
		[SerializeField] InputActionReference _inputAxis;
		[SerializeField] Vector2 _worldMinMax;
		[SerializeField] float _maxSpeed;
		[SerializeField] float _easeTime;
		[SerializeField] float _startHeight;

		float _target,
			  _current;

		private void Start()
		{
			_offsetTransform.position = new Vector3(_offsetTransform.position.x, _startHeight, _offsetTransform.position.z);
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