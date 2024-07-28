/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Whisperer
{
    public class SpriteAnimator : MonoBehaviour
    {
        // Refs
        [Header("References")]
        [SerializeField]
        private Image _sprite;

        [SerializeField] private string _pathAndBaseSpriteName;
        [SerializeField] private Vector2 _loopFrames;

        // Props
        [Header("Properties")]
        [SerializeField]
        private float _fps = 24f;

        // Debug
        public bool _showLogs;
        private bool _animate;
        private int _currentFrame;

        // Vars
        private float _frameDuration;
        private Progress _progress;
        private float _runTime;

        private void Awake()
        {
            _frameDuration = 1 / _fps;
            _progress = new Progress(SetFrame);
            _progress.Loop = true;
            _runTime = _loopFrames.y / _fps;
        }

        private void OnEnable()
        {
            PlayAnimation();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        [ContextMenu("PlayAnimation")]
        private void PlayAnimation()
        {
            _progress.ResetTo0();
            _progress.Play(_runTime);

            //StopAllCoroutines();
            //StartCoroutine(Animate());
            //_animate = true;
        }

        [ContextMenu("StopAnimation")]
        private void StopAnimation()
        {
            _progress.Pause();

            //StopAllCoroutines();
            //_animate = false;
        }

        private void SetFrame(float progress)
        {
            var currentFrame = Mathf.RoundToInt(progress * _loopFrames.y);
            _sprite.sprite = Resources.Load<Sprite>($"{_pathAndBaseSpriteName}{currentFrame}");
        }

        private IEnumerator Animate()
        {
            _currentFrame = 0;

            while (true)
            {
                if (_currentFrame >= _loopFrames.y) // loopFrames.y <--- this is the last frame of each anim sequence
                    _currentFrame = (int)_loopFrames.x; // <--- this is the start of the looped section

                _sprite.sprite = Resources.Load<Sprite>($"{_pathAndBaseSpriteName}{_currentFrame}");
                if (_showLogs) Debug.Log($"{_pathAndBaseSpriteName}{_currentFrame}");

                _currentFrame++;
                yield return new WaitForSeconds(_frameDuration);
                yield return 0;
            }
        }
    }
}
