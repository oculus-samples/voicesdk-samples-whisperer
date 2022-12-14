/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Whisperer
{
        /// <summary>
        ///     Manages XR Rig tracked UI panels.
        /// </summary>
        public class UXManager : MonoBehaviour
    {
        public static UXManager Instance;

        [SerializeField] private Transform _playerRoot;
        [SerializeField] private RigHandsControl _rigHandsControl;
        [SerializeField] private List<UXTrackedDisplay> _displays = new();

        [HideInInspector] public UnityEvent<UXTrackedDisplay, bool> DisplayStateChanged;

        private bool _disableMenuToggles;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            LevelLoader.Instance.OnLevelWillBeginUnload.AddListener(BeginLevelLoad);

            _displays.ForEach(d => d.Setup(_playerRoot));
        }

        public void SetMenusAllowed(bool allowed)
        {
            _disableMenuToggles = !allowed;
            if (_disableMenuToggles) CloseAllDisplays();
        }

        public void AddDisplay(UXTrackedDisplay display)
        {
            if (!_displays.Contains(display))
            {
                display.IsSceneDisplay = true;
                _displays.Add(display);
                display.Setup(_playerRoot);
            }
        }

        public void RemoveDisplay(UXTrackedDisplay display)
        {
            _displays.Remove(display);
        }

        public void SetDisplay(UXTrackedDisplay display, bool displayed)
        {
            if (displayed) OpenDisplay(display);
            else CloseDisplay(display);
        }

        public void OpenDisplay(string menuName)
        {
            _displays.ForEach(d =>
            {
                if (d.gameObject.name == menuName) OpenDisplay(d);
            });
        }

        public void OpenDisplay(UXTrackedDisplay display)
        {
            if (_disableMenuToggles || !display.enabled) return;

            display.SetDisplayed(true);
            DisplayStateChanged.Invoke(display, true);
        }

        public void CloseDisplay(string menuName)
        {
            _displays.ForEach(d =>
            {
                if (d.gameObject.name == menuName)
                    StartCoroutine(CloseDisplayNextFrame(d));
            });
        }

        public void CloseDisplay(UXTrackedDisplay display)
        {
            StartCoroutine(CloseDisplayNextFrame(display));
        }

        public void CloseAllDisplays()
        {
            _displays.ForEach(d => StartCoroutine(CloseDisplayNextFrame(d)));
        }

        private IEnumerator CloseDisplayNextFrame(UXTrackedDisplay d)
        {
            yield return new WaitForEndOfFrame();

            DisplayStateChanged.Invoke(d, false);
            d.SetDisplayed(false);
        }

        public void DisposeDisplay(UXTrackedDisplay display)
        {
            _displays.Remove(display);
            Destroy(display.gameObject);
        }

        public void SetDisplayEnabled(string displayName, bool set)
        {
            _displays.ForEach(d =>
            {
                if (d.gameObject.name == displayName)
                    d.GetComponent<UXTrackedDisplay>().SetDisplayEnabled(set);
            });
        }

        private void BeginLevelLoad(float delay)
        {
            if (_displays.Count == 0) return;

            for (var i = _displays.Count - 1; i >= 0; i--)
                if (_displays[i].IsSceneDisplay)
                    DisposeDisplay(_displays[i]);
                else
                    CloseDisplay(_displays[i]);
        }
    }
}
