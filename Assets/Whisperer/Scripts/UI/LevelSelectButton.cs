/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
    public class LevelSelectButton : MonoBehaviour
    {
        public void LoadLevel(int levelIndex)
        {
            if (levelIndex == 0 || levelIndex == 4)
                LevelLoader.Instance?.LoadLevel(levelIndex, false);
            else
                LevelLoader.Instance?.LoadLevel(levelIndex);
        }

        public void MainMenu()
        {
            var startIndex = PlayerPrefs.GetInt(LevelLoader.Instance.COMPLETED_NAME, 0) == 1 ? 4 : 0;
            LevelLoader.Instance?.LoadLevel(startIndex, false);
        }
    }
}
