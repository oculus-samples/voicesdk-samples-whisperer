/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{
    public class Level_1_Manager : LevelManager
    {
        [Header("Level 1")] [SerializeField] private Animator _beaAnimator;

        [SerializeField] private SpeechBubble _beaSpeech;
        [SerializeField] private HaroldTheBird _harold;
        [SerializeField] private Chalkboard _chalkBoard;
        [SerializeField] private List<LightsMaterialController> _lightsMaterials;
        [SerializeField] private float _lightsOnIntensity = 2.3f;
        [SerializeField] private List<Checkmark> _checkmarks;
        [SerializeField] private List<Listenable> _movedItems;
        [SerializeField] private Listenable _heroPot;
        [SerializeField] private FirstRunTooltip _firstRunTooltip;
        [SerializeField] private List<HintDefinition> _hints;

        private bool _heroPotMoved,
            _tutorialComplete,
            _beaComplete,
            _twoMovesComplete,
            _movesComplete;

        private IEnumerator _levelRoutine;

        protected override void Start()
        {
            base.Start();

            _speakGestureWatcher.AllowSpeak = !_levelLogicEnabled;

            if (_levelLogicEnabled)
            {
                _allListenableScripts.ForEach(l => l.SetListeningActive(false));
                _allListenableScripts.ForEach(l => l.OnDestroyed.AddListener(RemoveFromLists));
            }

            UXManager.Instance.SetDisplayEnabled("SettingsMenu", true);
        }

        public override void StartLevel()
        {
            _hands.SetSpeak();

            FindObjectOfType<CameraColorOverlay>().SetTargetColor(Color.black);

            _lightsMaterials = new List<LightsMaterialController>(FindObjectsOfType<LightsMaterialController>());
            GameObject.Find("Hero Drawers").SetActive(false);

            _harold.HintDefinitions = new List<HintDefinition>();

            _heroPot.SetHighlightOnlyMode(true);

            _levelRoutine = LevelRoutine();
            StartCoroutine(_levelRoutine);
        }

        /// <summary>
        ///     Main narrative logic
        /// </summary>
        /// <returns></returns>
        private IEnumerator LevelRoutine()
        {
            yield return new WaitForSeconds(1);

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_Intro", transform));

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_Intro_2", transform));

            _speakGestureWatcher.AllowSpeak = true;
            while (!_speakGestureWatcher.HaveSpeakPose) yield return null;

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_Hands1", transform));

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_Hands2", transform));

            _heroPot.SetListeningActive(true);
            _firstRunTooltip.Show();
            while (!_speakGestureWatcher.HaveListenable) yield return null;

            yield return new WaitForSeconds(1);

            _speakGestureWatcher.AllowSpeak = false;
            _heroPot.SetListeningActive(false);

            yield return new WaitForSeconds(0.05f);

            _heroPot.SetListeningActive(true);
            _speakGestureWatcher.AllowSpeak = true;
            _appVoiceExperience.Deactivate();

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_MoveDirection", transform));

            if (_firstRunTooltip)
                _firstRunTooltip.Expand();

            /// Wait for hero pot move                        
            _heroPot.SetHighlightOnlyMode(false);
            while (!_heroPotMoved) yield return null;

            /// Harold wakes
            _harold.WakeUp();
            yield return new WaitForSeconds(2);

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_BirdWake", transform));

            /// Start move 3 sequence
            _tutorialComplete = true;
            _chalkBoard.FadeToNext(2);
            _allListenableScripts.ForEach(l => l.SetListeningActive(true));
            _harold.HintDefinitions.Add(_hints[0]);
            _harold.SetTranscriptionMode();

            while (!_twoMovesComplete) yield return null;

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_ShowingOff", transform));

            while (!_movesComplete) yield return null;

            yield return new WaitForSeconds(2);

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Door", _checkmarks[2].transform));

            _lightsMaterials.ForEach(m => m.SetLights(_lightsOnIntensity));
            AudioManager.Instance.Play("Lightswitch", _checkmarks[2].transform);
            _appVoiceExperience.VoiceEvents.OnRequestCompleted.RemoveAllListeners();

            yield return new WaitForSeconds(1);

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_Ruckus", transform));

            _beaAnimator.SetTrigger("Bea_Enters");

            _allListenableScripts.ForEach(listenable =>
            {
                if (listenable is ForceMovable) ((ForceMovable)listenable).EnableGravity();
                listenable.SetListeningActive(false);
            });

            yield return new WaitForSeconds(2);

            yield return new WaitForSeconds(
                _beaSpeech.SetSpeech("Who's there?", 2, "Bea_Confused"));

            yield return new WaitForSeconds(3);

            yield return new WaitForSeconds(
                _beaSpeech.SetSpeech("...rotten raccoons...", 2, "Bea_Sigh"));

            yield return new WaitForSeconds(0.5f);

            yield return new WaitForSeconds(
                _beaSpeech.SetSpeech("It can wait till morning", 2, "Bea_LetsGo"));

            yield return new WaitForSeconds(2);

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_Mess", transform));

            while (!_beaComplete) yield return null;

            yield return new WaitForSeconds(2);
            LevelLoader.Instance.LoadNextLevel();
        }

        protected override void OnAnimationEvent(string message)
        {
            base.OnAnimationEvent(message);

            switch (message)
            {
                case "Bea_Exit":
                    _beaComplete = true;
                    break;
            }
        }

        protected override void OnListenableResponse(ListenableEventArgs eventArgs)
        {
            base.OnListenableResponse(eventArgs);

            /// Hero pot
            if (!_tutorialComplete && eventArgs.Listenable == _heroPot && eventArgs.Success)
            {
                _heroPotMoved = true;
                return;
            }

            /// Collect moved objects to list
            if (eventArgs.Action == "move"
                && eventArgs.Success
                && !_movedItems.Contains(eventArgs.Listenable)
                && !_movesComplete)
            {
                _movedItems.Add(eventArgs.Listenable);

                for (var i = 0; i < _checkmarks.Count; i++)
                    if (!_checkmarks[i].IsChecked)
                    {
                        _checkmarks[i].SetChecked(true);
                        if (i == 1) _twoMovesComplete = true;
                        if (i == 2) _movesComplete = true;

                        break;
                    }
            }
        }

        protected virtual void RemoveFromLists(Listenable listenable)
        {
            _allListenableScripts.Remove(listenable);
            _movedItems.Remove(listenable);
        }
    }
}
