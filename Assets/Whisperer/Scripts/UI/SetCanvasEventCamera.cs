/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

public class SetCanvasEventCamera : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }
}
