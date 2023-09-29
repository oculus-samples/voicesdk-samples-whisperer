/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using Meta.WitAi;
using Meta.WitAi.Json;
using UnityEngine;

namespace Whisperer
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HighlightObject))]
    public class ForceMovable : Listenable
    {
        private const string MOVE_INTENT = "move";
        private const string JUMP_INTENT = "jump";
        private const string PULL_INTENT = "pull";
        private const string PUSH_INTENT = "push";
        private const string LEVITATE_INTENT = "levitate";
        private const string DROP_INTENT = "drop";
        private const string DIRECTION_ENTITY_KEY = "direction:direction";
        private const string STRENGTH_ENTITY_KEY = "move_strength:move_strength";

        [Header("Force Move Settings")] [SerializeField]
        protected float _baseForce = 5;

        [SerializeField] protected float _littleMod = 0.5f;
        [SerializeField] protected float _lotMod = 2.0f;
        [SerializeField] protected float _upMod = .12f;
        [SerializeField] protected bool _levitatable = true;

        [Header("Impact SFX Settings")] [SerializeField]
        private string _impactAudioClip;

        [SerializeField] private float _volume = 1;
        [SerializeField] private float _impactVelocityThreshold = .01f;

        protected float _floatMod = .1f;
        protected Vector3 _movementVector;
        protected float _originalForce;
        protected Rigidbody rb;

        private static ForceDirection GetDirectionOrDefault(WitResponseNode response, ForceDirection defaultDirection)
        {
            var directionValue = response.GetFirstEntityValue(DIRECTION_ENTITY_KEY);
            if (!Enum.TryParse(directionValue, out ForceDirection direction))
            {
                direction = defaultDirection;
            }

            return direction;
        }

        protected void Awake()
        {
            rb = GetComponentInChildren<Rigidbody>();
            _originalForce = _baseForce;
        }

        protected void OnValidate()
        {
            _originalForce = _baseForce;
        }

        #region Wit Event Handling

        [MatchIntent(JUMP_INTENT)]
        public void Jump(WitResponseNode response)
        {
            ForceMove(ForceDirection.up, response);
        }
        
        [MatchIntent(PUSH_INTENT)]
        public void Push(WitResponseNode response)
        {
            ForceMove(ForceDirection.away, response);
        }
        
        [MatchIntent(PULL_INTENT)]
        public void Pull(WitResponseNode response)
        {
            ForceMove(ForceDirection.toward, response);
        }
        
        [MatchIntent(MOVE_INTENT)]
        public void Move(WitResponseNode response)
        {
            ForceMove(ForceDirection.right, response);
        }
        
        /// <summary>
        ///     Moves a rigidbody in a direction
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="witResponse"></param>
        public virtual void ForceMove(ForceDirection direction, WitResponseNode witResponse)
        {
            if(!IsSelected || !_actionState)
            {
                return;
            }
            
            float multiplier = 1;
            var success = false;
            var strength = witResponse.GetFirstEntityValue(STRENGTH_ENTITY_KEY);

            // Set force strength
            // If utterance contains no strength, use normal
            strength = strength ?? "normal";

            if (strength == "weak") multiplier = _littleMod;
            if (strength == "strong") multiplier = _lotMod;

            var toCamera = Camera.main.transform.position - transform.position;
            toCamera.y = rb.useGravity ? 0 : toCamera.y;
            toCamera.Normalize();
            toCamera += Vector3.up * _upMod;

            direction = GetDirectionOrDefault(witResponse, direction);

            switch (direction)
            {
                case ForceDirection.left:
                    AddForceDirection(Quaternion.Euler(0, 90, 0) * toCamera, multiplier);
                    success = true;
                    break;

                case ForceDirection.right:
                    AddForceDirection(Quaternion.Euler(0, -90, 0) * toCamera, multiplier);
                    success = true;
                    break;

                case ForceDirection.toward:
                    AddForceDirection(toCamera, multiplier);
                    success = true;
                    break;

                case ForceDirection.away:
                    AddForceDirection(-toCamera, multiplier);
                    success = true;
                    break;

                case ForceDirection.up:
                    AddForceDirection(Vector3.up, multiplier);
                    success = true;
                    break;

                case ForceDirection.across:
                    var toCenter = new Vector3(0f, transform.position.y, 0f) - transform.position;
                    AddForceDirection(toCenter.normalized, multiplier * _lotMod);
                    success = true;
                    break;

                case ForceDirection.wall:
                    var wallVector = transform.position - new Vector3(0f, transform.position.y, 0f);
                    AddForceDirection(wallVector.normalized, multiplier * _lotMod);
                    success = true;
                    break;

                default:
                    success = false;
                    break;
            }

            ProcessComplete(MOVE_INTENT, success);
        }

        protected void AddForceDirection(Vector3 direction, float multiplier)
        {
            rb.AddForce(direction * _baseForce * multiplier, ForceMode.Impulse);
        }

        [MatchIntent(LEVITATE_INTENT)]
        public void Levitate()
        {
            if(!IsSelected || !_actionState)
            {
                return;
            }
            if (_levitatable)
            {
                rb.useGravity = false;
                rb.drag = .05f;

                var toCamera = Camera.main.transform.position - transform.position;
                toCamera.y = 0;
                toCamera.Normalize();
                toCamera += Vector3.up * _upMod;

                rb.AddForce(toCamera * .1f + Vector3.up * .05f, ForceMode.Impulse);
                rb.AddTorque(transform.forward * .15f);
                ProcessComplete(LEVITATE_INTENT, true);
            }
            else
            {
                ProcessComplete(LEVITATE_INTENT, false);
            }
        }

        [MatchIntent(DROP_INTENT)]
        public void Drop()
        {
            if(!IsSelected || !_actionState)
            {
                return;
            }
            if (_levitatable)
            {
                EnableGravity();
                ProcessComplete(DROP_INTENT, true);
            }
            else
            {
                ProcessComplete(DROP_INTENT, false);
            }
        }

        public void EnableGravity()
        {
            rb.drag = 0;
            rb.useGravity = true;
        }

        #endregion

        #region Audio

        private void OnCollisionEnter(Collision collision)
        {
            if (_impactAudioClip == "")
                return;

            if (collision.relativeVelocity.magnitude > _impactVelocityThreshold) PlayThonkSound();
        }

        public void PlayThonkSound()
        {
            Debug.Log("thonk");
            PlayAudio(_impactAudioClip, transform, _volume);
        }

        public void PlayAudio(string audioClip, Transform transform, float volume)
        {
            AudioManager.Instance.Play(audioClip, transform, _volume);
        }

        public void StopAudio(string audioClip)
        {
            AudioManager.Instance.Stop(audioClip);
        }

        #endregion
    }
}
