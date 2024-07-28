/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleObjectsActive : MonoBehaviour
{
    [SerializeField] private InputActionReference _button;

    [SerializeField] private List<GameObject> _objects = new();

    private void Start()
    {
        _button.action.performed += Action_performed;
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        _objects.ForEach(o => o.SetActive(!o.activeSelf));
    }
}
