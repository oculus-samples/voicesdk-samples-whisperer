/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Whisperer
{
    public class LineVisualEnable : MonoBehaviour
    {
        [SerializeField] bool hoverEnable;
        [SerializeField] bool selectEnable;
        [SerializeField] float delayDisableTime;

        public XRRayInteractor rayInteractor;
        public XRInteractorLineVisual interactorLineVisual;

        HoverEnterEvent rayHoverEnter;
        HoverExitEvent rayHoverExit;
        SelectEnterEvent raySelectEnter;
        SelectExitEvent raySelectExit;
        bool rayHovering, raySelecting, canEnable, delaying;
        float disabledTimestamp;

        //[SerializeField] List<XRBaseInteractable> targets = new List<XRBaseInteractable>();

        private void Awake()
        {
            rayHoverEnter = rayInteractor.hoverEntered;
            rayHoverExit = rayInteractor.hoverExited;
            rayHoverEnter.AddListener((HoverEnterEventArgs args) => rayHovering = true);
            rayHoverExit.AddListener((HoverExitEventArgs args) => rayHovering = false);
            raySelectEnter = rayInteractor.selectEntered;
            raySelectExit = rayInteractor.selectExited;
            raySelectEnter.AddListener((SelectEnterEventArgs args) => raySelecting = true);
            raySelectExit.AddListener((SelectExitEventArgs args) => raySelecting = false);
        }

        void Update()
        {
            //rayInteractor.GetHoverTargets(targets);

            if(delayDisableTime > 0)
            {
                canEnable = (rayHovering && hoverEnable) || (raySelecting && selectEnable);
                if (canEnable) interactorLineVisual.enabled = true;
                else
                {         
                    if (interactorLineVisual.enabled && !delaying) /// need to set new delay
                    {
                        disabledTimestamp = Time.time;
                        delaying = true;
                    }
                    else if(delaying)
                    {
                        if(Time.time - disabledTimestamp >= delayDisableTime)
                        {
                            interactorLineVisual.enabled = false;
                            delaying = false;
                        }
                    }
                }
            }
            else
                interactorLineVisual.enabled = (rayHovering && hoverEnable) || (raySelecting && selectEnable);      
        }
    }
}
