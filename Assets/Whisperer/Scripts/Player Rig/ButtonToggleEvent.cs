/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace Whisperer
{
    public class ButtonToggleEvent : MonoBehaviour
    {
        public InputActionReference[] buttons;
        public UnityEvent<bool> OnToggle;
        public UnityEvent OnStateTrue;
        public UnityEvent OnStateFalse;

        [SerializeField] bool state;

        private void Awake()
        {
            foreach (InputActionReference button in buttons)
            {
                button.action.performed += Action_performed;
            }
        }

        private void Start()
        {
            Execute();
        }

        private void OnDisable()
        {
            foreach (InputActionReference button in buttons)
            {
                button.action.performed -= Action_performed;
            }
        }

        private void Action_performed(InputAction.CallbackContext obj)
        {
            state = !state;
            Execute();
        }

        private void Execute()
        {
            OnToggle.Invoke(state);

            if (state) OnStateTrue.Invoke();
            if (!state) OnStateFalse.Invoke();
        }

        public void ToggleOn()
        {
            state = true;
            Execute();
        }

        public void ToggleOff()
        {
            state = false;
            Execute();
        }
    }
}
