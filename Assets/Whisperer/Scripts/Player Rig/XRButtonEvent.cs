/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class XRButtonEvent : MonoBehaviour
{
    [SerializeField] private InputActionReference inputButton;
    [SerializeField] private Button passthruUIButton;

    public UnityEvent ButtonPressed;

    private void Start()
    {
        inputButton.action.performed += Action_performed;
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        ButtonPressed.Invoke();
        if (passthruUIButton != null) passthruUIButton.onClick.Invoke();
    }
}
