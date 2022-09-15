/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using TMPro;

namespace Whisperer
{

	public class SetVersionDisplay : MonoBehaviour
	{
		[SerializeField] TMP_Text _field;
		[SerializeField] string _label;
		private void Start()
		{
			_field.text = _label + Application.version;
		}
	}
}
