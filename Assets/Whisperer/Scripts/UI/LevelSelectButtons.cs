/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Whisperer
{
    public class LevelSelectButtons : MonoBehaviour
    {
        [SerializeField] private List<Button> _buttons;

        private void Start()
        {
            StartCoroutine(EnableButtons());
        }

        private IEnumerator EnableButtons()
        {
            yield return new WaitForSeconds(2f);
            _buttons.ForEach(b => b.interactable = true);
        }
    }
}
