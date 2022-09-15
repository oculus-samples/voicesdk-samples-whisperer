/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
	public class BoundsDetector : MonoBehaviour
	{
		[SerializeField] float _maxDistance = 1f;
		[SerializeField] Transform _centerTransform;
		[SerializeField] float _frequency = 0.5f;
		[SerializeField] CanvasGroup _canvasGroup;
		[SerializeField] Color _overlayColor;
		[SerializeField] CameraColorOverlay _cameraColorOverlay;

		public float MaxDistance { get => _maxDistance; set => _maxDistance = value; }

		Vector3 _center;
		float _distance;
		bool _outOfBounds;

		Progress _canvasFader;

		private void Start()
		{
			_canvasGroup.alpha = 0;
			_canvasFader = new Progress(SetCanvasFade);

			InvokeRepeating("CheckDistance", _frequency, _frequency);
		}

		private void CheckDistance()
		{
			_center.Set(_centerTransform.position.x, Camera.main.transform.position.y, _centerTransform.position.z);
			_distance = Vector3.Distance(_center, Camera.main.transform.position);
			SetOutOfBounds(_distance > _maxDistance);
		}

		private void SetOutOfBounds(bool outOfBounds)
		{
			if (outOfBounds && !_outOfBounds)
			{
				_cameraColorOverlay.FadeToColor(_overlayColor, 0.25f);
				_canvasFader.Play();
				UXManager.Instance.OpenDisplay("OutOfBounds");

			}
			else if (!outOfBounds && _outOfBounds)
			{
				_canvasFader.PlayReverse();
				_cameraColorOverlay.FadeToClear(1);
			}

			_outOfBounds = outOfBounds;
		}

		private void SetCanvasFade(float p)
		{
			_canvasGroup.alpha = p;
			if(p == 0) UXManager.Instance.CloseDisplay("OutOfBounds");

		}
	}
}
