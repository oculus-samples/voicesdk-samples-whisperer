/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
	/// <summary>
	/// Simple floating object animation
	/// </summary>
	public class FloatingAnim : MonoBehaviour
	{
		public float speed;
		[SerializeField] Vector3 rotSpeed;
		[SerializeField] Vector3 amount;
		[SerializeField] Vector3 rotAmount;

		[Header("Use to give appearance of random offset")]
		[SerializeField] Vector3 offset;

		Vector3 startPos;
		float x, y, z, Rotx, Roty, Rotz;

		private void Start()
		{
			startPos = transform.localPosition;
		}

		private void Update()
		{


			x = Mathf.Sin((float)(offset.x + Time.time) * speed) * amount.x;
			y = Mathf.Sin((float)(offset.y + Time.time) * speed) * amount.y;
			z = Mathf.Sin((float)(offset.z + Time.time) * speed) * amount.z;

			Rotx = Mathf.Sin((float)(Time.time + offset.x) * rotSpeed.x) * rotAmount.x;
			Roty = Mathf.Sin((float)(Time.time + offset.y) * rotSpeed.y) * rotAmount.y;
			Rotz = Mathf.Sin((float)(Time.time + offset.z) * rotSpeed.z) * rotAmount.z;


			transform.localPosition = new Vector3(x, y, z) + startPos;
			transform.Rotate(new Vector3(Rotx, Roty, Rotz));
		}
	}
}
