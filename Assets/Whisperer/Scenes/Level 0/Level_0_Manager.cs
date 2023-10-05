/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Oculus.VoiceSDK.Utilities;
using UnityEngine;

namespace Whisperer
{
    public class Level_0_Manager : LevelManager
    {
        [SerializeField] private GameObject _startMenu;
        [SerializeField] private GameObject _consentDialog;
        [SerializeField] private GameObject _micPermissionsDialog;

        protected override void Start()
        {
            base.Start();

            _speakGestureWatcher.AllowSpeak = !_levelLogicEnabled;

            _startMenu.SetActive(false);
            _consentDialog.SetActive(false);
            _micPermissionsDialog.SetActive(false);
        }

        public override void StartLevel()
        {
            _hands.SetRay();

            FindObjectOfType<CameraColorOverlay>().SetTargetColor(Color.black);

            if (MicPermissionsManager.HasMicPermission())
                MicPermissionsObtained();
            else
                ShowMicPermissionsPrompt();
        }

        private void MicPermissionsObtained()
        {
            _micPermissionsDialog.SetActive(false);
            if (PlayerPrefs.GetInt("VoiceDataConsent", 0) == 1)
                StartMenu();
            else
                ShowConsent();
        }

        private void ShowMicPermissionsPrompt()
        {
            _micPermissionsDialog.SetActive(true);
        }

        private void ShowConsent()
        {
            _consentDialog.SetActive(true);
        }

        private void StartMenu()
        {
            _startMenu.SetActive(true);
        }

        public void ConsentAccept()
        {
            PlayerPrefs.SetInt("VoiceDataConsent", 1);
            PlayerPrefs.Save();

            _consentDialog.SetActive(false);

            StartMenu();
        }

        public void ConsentDecline()
        {
            Application.Quit();
        }

        public void AcceptPermissionsRequest()
        {
            MicPermissionsManager.RequestMicPermission(PermissionGrantedCallback);
        }

        private void PermissionGrantedCallback(string permissionName)
        {
            MicPermissionsObtained();
        }

        public void RejectPermissions()
        {
            Application.Quit();
        }
    }
}
