/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
    public class FollowUI : MonoBehaviour
    {
        [SerializeField] protected Transform _followTransform;
        [SerializeField] protected Vector3 _offset = Vector3.zero;
        [SerializeField] protected float _scaleMultiplier = .5f; // Scale ui based on distance from camera
        protected float _fadeDuration = .2f;
        protected float _opacity;

        protected float _targetY;

        public Transform FollowTransform
        {
            get => _followTransform;
            set => _followTransform = value;
        }

        protected virtual void Start()
        {
            if (_followTransform != null) SetFollowTransform(_followTransform);
        }

        protected virtual void Update()
        {
            if (_opacity == 0) return;

            SetPosition();
            SetScale();
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }

        /// <summary>
        ///     Sets the transform this UI canvas is attached to
        /// </summary>
        /// <param name="t"></param>
        public void SetFollowTransform(Transform t)
        {
            _followTransform = t;

            var c = t.GetComponent<Collider>();
            if (c != null)
                _targetY = c.bounds.max.y - _followTransform.position.y;

            SetPosition();
            SetScale();
        }

        protected virtual void SetOpacity(float opacity)
        {
            _opacity = opacity;
        }

        protected void SetPosition()
        {
            if (_followTransform is not null)
                transform.position = new Vector3(
                    _followTransform.position.x + _offset.x,
                    _followTransform.position.y + _targetY + +_offset.y,
                    _followTransform.position.z + _offset.z
                );
        }

        protected void SetScale()
        {
            var distance = Vector3.Distance(Camera.main.transform.position, transform.position);
            var mult = Mathf.Max(1, _scaleMultiplier * distance);
            transform.localScale = Vector3.one * mult;
        }
    }
}
