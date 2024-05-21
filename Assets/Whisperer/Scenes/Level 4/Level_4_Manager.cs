/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
    public class Level_4_Manager : LevelManager
    {
        [SerializeField] private Material _plantsMaterial;

        protected override void Start()
        {
            base.Start();

            _speakGestureWatcher.AllowSpeak = !_levelLogicEnabled;

            if (_levelLogicEnabled)
            {
                _allListenableScripts.ForEach(l => l.SetListeningActive(false));
                _allListenableScripts.ForEach(l => l.IsActionable = false);
            }

            _plantsMaterial.SetFloat("_Alive_Dead_Lerp", 0);

            UXManager.Instance.SetDisplayEnabled("SettingsMenu", false);
        }

        public override void StartLevel()
        {
            _hands.SetRay();

            FindObjectOfType<CameraColorOverlay>().SetTargetColor(Color.black);

            if (!_inTransition)
            {
                AudioManager.Instance.AuxFader.PlayReverse(5);
                AudioManager.Instance.MusicFader.Play(0);
                AudioManager.Instance.PlayMusic("EndMusic");
            }
        }

        protected override void LevelLoadComplete()
        {
            base.LevelLoadComplete();
        }
    }
}
