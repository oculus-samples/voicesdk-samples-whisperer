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
	/// <summary>
	/// Unity event wrapper for animation events
	/// </summary>
	public class AnimationEvents : MonoBehaviour
	{
		public UnityEvent<string> OnAnimationEvent;
	
		public void InvokeEvent(string message)
		{
			OnAnimationEvent.Invoke(message);
		}
	}
}