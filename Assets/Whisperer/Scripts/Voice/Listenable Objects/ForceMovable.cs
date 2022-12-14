/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Facebook.WitAi;
using Facebook.WitAi.Lib;
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

        protected override void DetermineAction(WitResponseNode witResponse)
        {
            var data = witResponse.GetFirstIntentData();
            var intent = data == null ? "" : data.name;

            var direction = witResponse.GetFirstEntityValue("direction:direction");
            var strength = witResponse.GetFirstEntityValue("move_strength:move_strength");

            // Set force strength 
            float forceMultiplier = 1;

            // If utterance contains no strength, use normal
            strength = strength ?? "normal";

            if (strength == "weak") forceMultiplier = _littleMod;
            if (strength == "strong") forceMultiplier = _lotMod;

            // If utterance contains no direction, assume one depending on the intent
            if (direction == "")
            {
                if (intent == "move") direction = "away";
                if (intent == "push") direction = "away";
                if (intent == "pull") direction = "toward";
                if (intent == "jump") direction = "up";
            }

            // If we have move intent, but we do have a direction, lets assign move
            if (intent == "" && direction != "")
                intent = "move";

            // Levitate mode
            if (_levitatable) // If we're levitating, use modified force
                if (!rb.useGravity)
                    forceMultiplier = forceMultiplier * _floatMod;

            switch (intent)
            {
                case "move":
                    ForceMove(direction, forceMultiplier);
                    break;
                case "jump":
                    ForceMove(direction, forceMultiplier);
                    break;
                case "pull":
                    ForceMove(direction, forceMultiplier);
                    break;
                case "push":
                    ForceMove(direction, forceMultiplier);
                    break;
                case "levitate":
                    Levitate();
                    break;
                case "drop":
                    Drop();
                    break;
                default:
                    ProcessComplete(intent, false);
                    break;
            }
        }

        /// <summary>
        ///     Moves a rigidbody in a direction
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="multiplier"></param>
        protected virtual void ForceMove(string direction, float multiplier = 1)
        {
            var success = false;

            var toCamera = Camera.main.transform.position - transform.position;
            toCamera.y = rb.useGravity ? 0 : toCamera.y;
            toCamera.Normalize();
            toCamera += Vector3.up * _upMod;

            switch (direction)
            {
                case "left":
                    AddForceDirection(Quaternion.Euler(0, 90, 0) * toCamera, multiplier);
                    success = true;
                    break;

                case "right":
                    AddForceDirection(Quaternion.Euler(0, -90, 0) * toCamera, multiplier);
                    success = true;
                    break;

                case "toward":
                    AddForceDirection(toCamera, multiplier);
                    success = true;
                    break;

                case "away":
                    AddForceDirection(-toCamera, multiplier);
                    success = true;
                    break;

                case "up":
                    AddForceDirection(Vector3.up, multiplier);
                    success = true;
                    break;

                case "across":
                    var toCenter = new Vector3(0f, transform.position.y, 0f) - transform.position;
                    AddForceDirection(toCenter.normalized, multiplier * _lotMod);
                    success = true;
                    break;

                case "wall":
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

        protected void Levitate()
        {
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

        protected void Drop()
        {
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
