/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
    public class BoundsDetector : MonoBehaviour
    {
        [SerializeField] private float _maxDistance = 1f;
        [SerializeField] private Transform _centerTransform;
        [SerializeField] private float _frequency = 0.5f;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Color _overlayColor;
        [SerializeField] private CameraColorOverlay _cameraColorOverlay;

        private Progress _canvasFader;

        private Vector3 _center;
        private float _distance;
        private bool _outOfBounds;

        public float MaxDistance
        {
            get => _maxDistance;
            set => _maxDistance = value;
        }

        public bool Active { get; set; } = false;

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
            SetOutOfBounds(Active && _distance > _maxDistance);
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
            if (p == 0) UXManager.Instance.CloseDisplay("OutOfBounds");
        }
    }
}
