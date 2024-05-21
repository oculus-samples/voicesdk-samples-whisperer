/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using Oculus.Voice;
using UnityEngine;
using UnityEngine.UI;

namespace Whisperer
{
    public class LogosController : MonoBehaviour
    {
        [SerializeField] private List<Texture> _textures;
        [SerializeField] private Texture _error;
        [SerializeField] private float _preDelay = 2;
        [SerializeField] private float _fadeTime = 2;
        [SerializeField] private float _logoDisplayTime = 3;
        [SerializeField] private Image _image;
        [SerializeField] private RigHandsControl _hands;

        [SerializeField] private int _count;

        private Progress _fader,
            _timer;

        public bool Complete { get; set; }

        private void Start()
        {
            _hands.Transforms.ForEach(t => t.gameObject.SetActive(false));

            _fader = new Progress(Fade, _fadeTime);
            _timer = new Progress(Display, _logoDisplayTime);

            _image.material.mainTexture = _textures[0];
            _image.material.color = new Color(1, 1, 1, 0);

            Invoke("StartDisplay", _preDelay);

            UXManager.Instance.SetDisplayEnabled("SettingsMenu", false);
        }

        private void StartDisplay()
        {
            _fader.Play();
        }

        private void Display(float progress)
        {
            if (progress == 1) _fader.PlayReverse();
        }

        private void Fade(float progress)
        {
            _image.material.color = new Color(1, 1, 1, progress);

            if (progress == 1)
            {
                _timer.Play();
                _count++;
            }

            if (_count > 0 && progress == 0)
            {
                if (_count < _textures.Count)
                {
                    _image.material.mainTexture = _textures[_count];
                    _fader.Play();
                }
                else
                {
                    StartApp();
                }
            }
        }

        private void StartApp()
        {
            var token = FindObjectOfType<AppVoiceExperience>().RuntimeConfiguration.witConfiguration.GetClientAccessToken();
            if (!string.IsNullOrEmpty(token))
            {
                _hands.Transforms.ForEach(t => t.gameObject.SetActive(true));
                _hands.SetNone();
                LevelLoader.Instance.StartApp();
            }
            else
            {
                _image.material.mainTexture = _error;
                _image.material.color = new Color(1, 1, 1, 1);
                Debug.LogError("You need to configure Wit before running this project!");
            }
        }
    }
}
