/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Whisperer
{
    public class HeroPlant : ForceMovable
    {
            /// <summary>
            ///     Hero Move Setup
            /// </summary>
            [Header("Hero Move Setup")] [SerializeField]
        private Transform _targetSpotTransform;

        [SerializeField] private float _distanceThreshold = 0.35f;
        public UnityEvent OnPlacedOnSpot;

        /// <summary>
        ///     Rigidbody Move Settings
        /// </summary>
        [Header("Rigidbody Move Settings")] [SerializeField]
        private float _moveDistance = .25f;

        [SerializeField] private float _moveTime = .4f;
        [SerializeField] private Easings.Functions _easing;
        [SerializeField] private float _minX = -1.1f;
        [SerializeField] private float _maxX = 1.1f;
        [SerializeField] private float _minZ = .4f;
        [SerializeField] private float _maxZ = 1.38f;
        private Progress _positionProgress;

        private Vector3 _startPos;
        private Vector3 _targetPos;


        protected override void Start()
        {
            base.Start();

            StartCoroutine(WatchDistanceThreshold());
            _positionProgress = new Progress(LerpPosition);
        }

        protected IEnumerator WatchDistanceThreshold()
        {
            while (Vector3.Distance(transform.position, _targetSpotTransform.position) > _distanceThreshold)
                yield return null;

            SetListeningActive(false);
            OnPlacedOnSpot.Invoke();

            GetComponentInChildren<Animator>().SetTrigger("Grow");
        }

        /*protected override void ForceMove(string direction, float multiplier = 1)
        {
            bool success = false;

            Vector3 upVec = Vector3.up * _upMod;

            switch (direction)
            {
                case "left":
                    AddForceDirection(Vector3.left + upVec, multiplier);
                    success = true;
                    break;
                case "right":
                    AddForceDirection(Vector3.right + upVec, multiplier);
                    success = true;
                    break;
                case "toward":
                    AddForceDirection(Vector3.back + upVec, multiplier);
                    success = true;
                    break;
                case "away":
                    AddForceDirection(Vector3.forward + upVec, multiplier);
                    success = true;
                    break;
                case "up":
                    AddForceDirection(Vector3.up, multiplier);
                    success = true;
                    break;
                default:
                    success = false;
                    break;
            }
            ProcessComplete("move", success);
        }*/

        protected override void ForceMove(string direction, float multiplier = 1)
        {
            var success = false;

            switch (direction)
            {
                case "left":
                    RigidbodyMove(Vector3.left, multiplier);
                    success = true;
                    break;
                case "right":
                    RigidbodyMove(Vector3.right, multiplier);
                    success = true;
                    break;
                case "toward":
                    RigidbodyMove(Vector3.back, multiplier);
                    success = true;
                    break;
                case "away":
                    RigidbodyMove(Vector3.forward, multiplier);
                    success = true;
                    break;
                default:
                    success = false;
                    break;
            }

            ProcessComplete("move", success);
        }

        private void RigidbodyMove(Vector3 direction, float distanceMultiplier = 1)
        {
            _positionProgress.Pause();
            _startPos = transform.position;
            _targetPos = transform.position + direction * (_moveDistance * distanceMultiplier);

            var clampedTargetPos = new Vector3(Mathf.Clamp(_targetPos.x, _minX, _maxX), _startPos.y,
                Mathf.Clamp(_targetPos.z, _minZ, _maxZ));
            _targetPos = clampedTargetPos;

            _positionProgress.Play(_moveTime);
        }

        private void LerpPosition(float progress)
        {
            var easedProgress = Easings.Interpolate(progress, _easing);
            var pos = Vector3.Lerp(_startPos, _targetPos, easedProgress);
            rb.MovePosition(pos);
        }
    }
}
