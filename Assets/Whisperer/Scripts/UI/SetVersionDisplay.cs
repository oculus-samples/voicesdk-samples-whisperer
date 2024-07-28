/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using TMPro;
using UnityEngine;

namespace Whisperer
{
    public class SetVersionDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _field;
        [SerializeField] private string _label;

        private void Start()
        {
            _field.text = _label + Application.version;
        }
    }
}
