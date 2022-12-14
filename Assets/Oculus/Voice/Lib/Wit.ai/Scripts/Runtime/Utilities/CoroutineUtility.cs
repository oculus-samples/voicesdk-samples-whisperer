/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Facebook.WitAi.Utilities
{
    public static class CoroutineUtility
    {
        // Start coroutine
        public static CoroutinePerformer StartCoroutine(IEnumerator asyncMethod, bool useUpdate = false)
        {
            var performer = GetPerformer();
            performer.CoroutineBegin(asyncMethod, useUpdate);
            return performer;
        }

        // Get performer
        private static CoroutinePerformer GetPerformer()
        {
            var performer = new GameObject("Coroutine").AddComponent<CoroutinePerformer>();
            performer.gameObject.hideFlags = HideFlags.HideAndDontSave;
            return performer;
        }

        // Coroutine performer
        public class CoroutinePerformer : MonoBehaviour
        {
            private Coroutine _coroutine;
            private IEnumerator _method;

            // Settings & fields
            private bool _useUpdate;

            // Whether currently running
            public bool IsRunning { get; private set; }

            // Dont destroy
            private void Awake()
            {
                DontDestroyOnLoad(gameObject);
            }

            // Update
            private void Update()
            {
                if (_useUpdate) CoroutineIterateUpdate();
            }

            // Cancel on destroy
            private void OnDestroy()
            {
                CoroutineUnload();
            }

            // Perform coroutine
            public void CoroutineBegin(IEnumerator asyncMethod, bool useUpdate)
            {
                // Cannot call twice
                if (IsRunning) return;

                // Begin running
                IsRunning = true;

                // Use update in batch mode
                if (Application.isBatchMode) useUpdate = true;
#if UNITY_EDITOR
                // Use update in editor mode
                if (!Application.isPlaying)
                {
                    useUpdate = true;
                    EditorApplication.update += EditorUpdate;
                }
#endif

                // Set whether to use update or coroutine implementation
                _useUpdate = useUpdate;
                _method = asyncMethod;

                // Begin with initial update
                if (_useUpdate)
                    CoroutineIterateUpdate();
                // Begin coroutine
                else
                    _coroutine = StartCoroutine(CoroutineIterateEnumerator());
            }

#if UNITY_EDITOR
            // Editor iterate
            private void EditorUpdate()
            {
                CoroutineIterateUpdate();
            }
#endif
            // Runtime iterate
            private IEnumerator CoroutineIterateEnumerator()
            {
                // Wait for completion
                yield return _method;
                // Complete
                CoroutineComplete();
            }

            // Batch iterate
            private void CoroutineIterateUpdate()
            {
                // Destroyed
                if (this == null || _method == null)
                    CoroutineCancel();
                // Continue
                else if (!MoveNext(_method)) CoroutineComplete();
            }

            // Move through queue
            private bool MoveNext(IEnumerator method)
            {
                // Move sub coroutine
                var current = method.Current;
                if (current != null && current.GetType().GetInterfaces().Contains(typeof(IEnumerator)))
                    if (MoveNext(current as IEnumerator))
                        return true;
                // Move this
                return method.MoveNext();
            }

            // Cancel current coroutine
            public void CoroutineCancel()
            {
                CoroutineComplete();
            }

            // Completed
            private void CoroutineComplete()
            {
                // Ignore unless running
                if (!IsRunning) return;

                // Unload
                CoroutineUnload();

                // Destroy
                if (this != null && gameObject != null) DestroyImmediate(gameObject);
            }

            // Unload
            private void CoroutineUnload()
            {
                // Done
                IsRunning = false;

                // Complete
                if (_method != null)
                {
#if UNITY_EDITOR
                    EditorApplication.update -= EditorUpdate;
#endif
                    _method = null;
                }

                // Stop coroutine
                if (_coroutine != null)
                {
                    StopCoroutine(_coroutine);
                    _coroutine = null;
                }
            }
        }
    }
}
