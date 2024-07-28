/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Whisperer
{
    public class LineVisualEnable : MonoBehaviour
    {
        [SerializeField] private bool hoverEnable;
        [SerializeField] private bool selectEnable;
        [SerializeField] private float delayDisableTime;

        public XRRayInteractor rayInteractor;
        public XRInteractorLineVisual interactorLineVisual;
        private float disabledTimestamp;

        private HoverEnterEvent rayHoverEnter;
        private HoverExitEvent rayHoverExit;
        private bool rayHovering, raySelecting, canEnable, delaying;
        private SelectEnterEvent raySelectEnter;
        private SelectExitEvent raySelectExit;

        //[SerializeField] List<XRBaseInteractable> targets = new List<XRBaseInteractable>();

        private void Awake()
        {
            rayHoverEnter = rayInteractor.hoverEntered;
            rayHoverExit = rayInteractor.hoverExited;
            rayHoverEnter.AddListener(args => rayHovering = true);
            rayHoverExit.AddListener(args => rayHovering = false);
            raySelectEnter = rayInteractor.selectEntered;
            raySelectExit = rayInteractor.selectExited;
            raySelectEnter.AddListener(args => raySelecting = true);
            raySelectExit.AddListener(args => raySelecting = false);
        }

        private void Update()
        {
            //rayInteractor.GetHoverTargets(targets);

            if (delayDisableTime > 0)
            {
                canEnable = (rayHovering && hoverEnable) || (raySelecting && selectEnable);
                if (canEnable)
                {
                    interactorLineVisual.enabled = true;
                }
                else
                {
                    if (interactorLineVisual.enabled && !delaying) /// need to set new delay
                    {
                        disabledTimestamp = Time.time;
                        delaying = true;
                    }
                    else if (delaying)
                    {
                        if (Time.time - disabledTimestamp >= delayDisableTime)
                        {
                            interactorLineVisual.enabled = false;
                            delaying = false;
                        }
                    }
                }
            }
            else
            {
                interactorLineVisual.enabled = (rayHovering && hoverEnable) || (raySelecting && selectEnable);
            }
        }
    }
}
