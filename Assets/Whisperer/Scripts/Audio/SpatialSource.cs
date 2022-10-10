/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

/// <summary>
/// Follows transform position of a target object for positional sound
/// </summary>
namespace Whisperer
{
	public class SpatialSource : MonoBehaviour
	{
		[SerializeField] Transform _followTransform;

		public Transform FollowTransform
		{
			get
			{
				return _followTransform;
			}
			set
			{
				_followTransform = value;
				transform.position = _followTransform ? _followTransform.position : Vector3.zero;
				transform.rotation = _followTransform ? _followTransform.rotation : Quaternion.identity;
			}
		}

		private void OnDestroy()
		{
			if (AudioManager.Instance != null)
				AudioManager.Instance.RemoveSpatialSource(transform);
		}

		private void Update()
		{
			if (_followTransform)
			{
				transform.position = FollowTransform.position;
				transform.rotation = FollowTransform.rotation;
			}

		}
	}
}
