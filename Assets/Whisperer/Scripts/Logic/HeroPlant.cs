/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using Meta.WitAi;
using Meta.WitAi.Json;
using UnityEngine;
using UnityEngine.Events;

namespace Whisperer
{
    public class HeroPlant : ForceMovable
    {
        private const string MOVE_INTENT = "move";
        private const string JUMP_INTENT = "jump";
        private const string PULL_INTENT = "pull";
        private const string PUSH_INTENT = "push";
        
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

        [MatchIntent(MOVE_INTENT)]
        public void Move(ForceDirection direction, WitResponseNode node)
        {
            ForceMove(direction, node);
        }

        [MatchIntent(JUMP_INTENT)]
        public void Jump(ForceDirection direction, WitResponseNode node)
        {
            ForceMove(direction, node);
        }

        [MatchIntent(PULL_INTENT)]
        public void Pull(ForceDirection direction, WitResponseNode node)
        {
            ForceMove(direction, node);
        }

        [MatchIntent(PUSH_INTENT)]
        public void Push(ForceDirection direction, WitResponseNode node)
        {
            ForceMove(direction, node);
        }
        
        public override void ForceMove(ForceDirection direction, WitResponseNode node)
        {
            if(!IsSelected || !_actionState)
            {
                return;
            }
            float multiplier = 1;
            var success = false;

            switch (direction)
            {
                case ForceDirection.left:
                    RigidbodyMove(Vector3.left, multiplier);
                    success = true;
                    break;
                case ForceDirection.right:
                    RigidbodyMove(Vector3.right, multiplier);
                    success = true;
                    break;
                case ForceDirection.toward:
                    RigidbodyMove(Vector3.back, multiplier);
                    success = true;
                    break;
                case ForceDirection.away:
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
