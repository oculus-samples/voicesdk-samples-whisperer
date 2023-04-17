/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Meta.WitAi;
using Meta.WitAi.Json;
using UnityEngine;

namespace Whisperer
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HighlightObject))]
    public class ForceMovable : Listenable
    {
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

        /// <summary>
        ///     Moves a rigidbody in a direction
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="multiplier"></param>
        [MatchIntent("move")]
        [MatchIntent("jump")]
        [MatchIntent("pull")]
        [MatchIntent("push")]
        public virtual void ForceMove(ForceDirection direction, WitResponseNode witResponse)
        {
            if(!IsSelected || !_actionState)
            {
                return;
            }
            float multiplier = 1;
            var success = false;
            var strength = witResponse.GetFirstEntityValue("move_strength:move_strength");

            // Set force strength 

            // If utterance contains no strength, use normal
            strength = strength ?? "normal";

            if (strength == "weak") multiplier = _littleMod;
            if (strength == "strong") multiplier = _lotMod;
            
            var toCamera = Camera.main.transform.position - transform.position;
            toCamera.y = rb.useGravity ? 0 : toCamera.y;
            toCamera.Normalize();
            toCamera += Vector3.up * _upMod;

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

            ProcessComplete("move", success);
        }

        protected void AddForceDirection(Vector3 direction, float multiplier)
        {
            rb.AddForce(direction * _baseForce * multiplier, ForceMode.Impulse);
        }

        [MatchIntent("levitate")]
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
                ProcessComplete("levitate", true);
            }
            else
            {
                ProcessComplete("levitate", false);
            }
        }

        [MatchIntent("drop")]
        public void Drop()
        {
            if(!IsSelected || !_actionState)
            {
                return;
            }
            if (_levitatable)
            {
                EnableGravity();
                ProcessComplete("drop", true);
            }
            else
            {
                ProcessComplete("drop", false);
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
