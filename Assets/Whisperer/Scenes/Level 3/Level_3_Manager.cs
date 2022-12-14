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
    public class Level_3_Manager : LevelManager
    {
        [SerializeField] private Animator _beaAnimator;
        [SerializeField] private SpeechBubble _beaSpeech;
        [SerializeField] private HaroldTheBird _harold;
        [SerializeField] private Chalkboard _chalkBoard;
        [SerializeField] private List<Checkmark> _checkmarks;

        /// soil -> seeds -> water
        [SerializeField] private WaterFaucet _water;

        [SerializeField] private ForceMovable _moveItem;
        [SerializeField] private List<Drawer> _drawers;
        [SerializeField] private List<GameObject> _seeds;
        [SerializeField] private List<GameObject> _sacks;
        [SerializeField] private List<Cactus> _cacti;
        [SerializeField] private GameObject _waterCan;
        [SerializeField] private GameObject _sackPoof;
        [SerializeField] private GameObject _drawerPoof;
        [SerializeField] private Material _plantsMaterial;
        [SerializeField] private List<HintDefinition> _hints;
        [SerializeField] private float _hintNarrationTimeout = 20f;
        [SerializeField] private FogController _fogController;
        private Cactus _cactus;
        private string _colorChoice = "";

        private bool _drawerOpen,
            _waitForAnimation,
            _waitForBeaPlants;

        private Progress _hintTimer;

        private IEnumerator _levelRoutine;

        protected override void Start()
        {
            base.Start();

            _speakGestureWatcher.AllowSpeak = !_levelLogicEnabled;

            if (_levelLogicEnabled) Setup();

            UXManager.Instance.SetDisplayEnabled("SettingsMenu", true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetComponent<ForceMovable>() == _moveItem)
                StartCoroutine(MoveItem(other.gameObject.GetComponent<ForceMovable>()));
        }

        public override void StartLevel()
        {
            _hands.SetSpeak();

            _speakGestureWatcher.AllowSpeak = true;

            GameObject.Find("Prop Drawers").SetActive(false);
            _drawers = new List<Drawer>(FindObjectsOfType<Drawer>());

            /// Find hero seeds & poof
            GameObject seeds = null;
            _drawers.ForEach(d =>
            {
                if (d.SeedPacket != null)
                {
                    seeds = d.SeedPacket;
                    _drawerPoof = d.Poof;
                }
            });
            _seeds[0] = seeds;
            _seeds[0].SetActive(true);


            _cacti = new List<Cactus>(FindObjectsOfType<Cactus>());

            _harold.HintDefinitions = new List<HintDefinition>();

            /// Let's go
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

            ///// Intro setup
            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_Intro3", transform));

            _chalkBoard.FadeToNext(2);
            _beaAnimator.SetTrigger("Bea_Starts");
            yield return new WaitForSeconds(
                _beaSpeech.SetSpeech("Let's get to work!", 3, "Bea_Starts"));

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_BeaTakeover", transform));

            _allListenableScripts.ForEach(l => l.SetListeningActive(true));

            _harold.SetTranscriptionMode();
            _harold.HintDefinitions.Add(_hints[0]);
            _harold.HintDefinitions.Add(_hints[1]);
            _harold.HintDefinitions.Add(_hints[2]);

            /// Wait for puzzle tasks
            while (!TasksComplete()) yield return null;

            yield return new WaitForSeconds(3);

            /// Save progress
            PlayerPrefs.SetInt(LevelLoader.Instance.COMPLETED_NAME, 1);
            PlayerPrefs.Save();

            /// Scene complete!!!
            GetCactus("none").gameObject.SetActive(false);
            if (_cactus is null) _cactus = GetCactus("red");
            _cactus.HideParent.SetActive(true);

            _beaAnimator.SetTrigger("Bea_Plants");
            _waitForBeaPlants = true;
            _hintTimer.Pause();
            _speakGestureWatcher.AllowSpeak = false;

            AudioManager.Instance.MusicFader.Play(0);
            AudioManager.Instance.PlayMusic("Ballad");
            AudioManager.Instance.AmbientFader.PlayReverse(8);

            yield return new WaitForSeconds(6);

            AudioManager.Instance.AuxFader.Play(10);
            AudioManager.Instance.PlayAux("WindLoop");

            while (_waitForBeaPlants) yield return null;

            UXManager.Instance.SetDisplayEnabled("SettingsMenu", false);
            AudioManager.Instance.MusicFader.PlayReverse(15);
            _hands.Transforms.ForEach(t => t.GetComponentInChildren<ParticleSystem>().Play());
            _hands.ColorFader.PlayFrom0(12);

            yield return new WaitForSeconds(
                _beaSpeech.SetSpeech("Oh, marvelous!", 3, "Bea_Marvel", .5f));

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_Complete3", transform));

            _fogController.FadeIn(45);
            _hands.Transforms.ForEach(t => t.GetComponentInChildren<ParticleSystem>().Stop());

            yield return new WaitForSeconds(5);

            yield return new WaitForSeconds(
                AudioManager.Instance.Play("Narrator_OtherSide", transform));

            yield return new WaitForSeconds(1);

            FindObjectOfType<CameraColorOverlay>().SetTargetColor(Color.white);

            LevelLoader.Instance.LoadLevel(4, false);

            yield return new WaitForSeconds(1.5f);
            _fogController.Reset();
            UXManager.Instance.SetDisplayEnabled("SettingsMenu", true);
        }

        protected override void OnListenableResponse(ListenableEventArgs eventArgs)
        {
            base.OnListenableResponse(eventArgs);

            /// Water hose
            if (eventArgs.Listenable == _water && _water.IsOn)
                StartCoroutine(WaterFill(eventArgs.Listenable as WaterFaucet));

            /// Drawer seeds
            if (eventArgs.Action == "open" && eventArgs.Success && _drawers.Contains(eventArgs.Listenable as Drawer))
                StartCoroutine(DrawerSeeds(eventArgs.Listenable as Drawer));

            /// Harold sets seed packet color
            if (eventArgs.Listenable == _harold)
                switch (eventArgs.Action)
                {
                    case "red":
                        _seeds[1].GetComponentInChildren<Renderer>().material
                            .SetVector("_Offset_Albedo", new Vector2(0, 0));
                        _cactus = GetCactus("red");
                        _colorChoice = "red";
                        break;
                    case "yellow":
                        _seeds[1].GetComponentInChildren<Renderer>().material
                            .SetVector("_Offset_Albedo", new Vector2(0.3f, 0));
                        _cactus = GetCactus("yellow");
                        _colorChoice = "yellow";
                        break;
                    case "blue":
                        _seeds[1].GetComponentInChildren<Renderer>().material
                            .SetVector("_Offset_Albedo", new Vector2(0.6f, 0));
                        _cactus = GetCactus("blue");
                        _colorChoice = "blue";
                        break;
                }

            /// General hint timer
            if (!eventArgs.Success && !_hintTimer.Playing)
                _hintTimer.PlayFrom0();
            else
                _hintTimer.Pause();
        }

        protected override void OnAnimationEvent(string eventName)
        {
            base.OnAnimationEvent(eventName);

            switch (eventName)
            {
                case "Drawer_Open":
                    _drawerOpen = true;
                    break;
                case "Cheers_Complete":
                    _waitForAnimation = false;
                    break;
                case "Bea_Plants_End":
                    _waitForBeaPlants = false;
                    break;
                case "Water_End":
                    _checkmarks[2].SetChecked(true);
                    break;
            }
        }

        private IEnumerator MoveItem(ForceMovable moveItem)
        {
            moveItem.SetListeningActive(false);

            yield return new WaitForSeconds(2);

            _sacks[0].SetActive(false);
            _sackPoof.SetActive(true);
            _sacks[1].SetActive(true);
            _checkmarks[0].SetChecked(true);
            _harold.HintDefinitions.Remove(_hints[0]);

            _beaAnimator.SetTrigger("Cheers");
            _waitForAnimation = true;
            _beaSpeech.SetSpeech("That's the one!", 3, "Bea_Excited", .5f);
            while (_waitForAnimation) yield return null;
        }

        private IEnumerator DrawerSeeds(Drawer drawer)
        {
            _drawerOpen = false;

            while (!_drawerOpen) yield return null;

            if (drawer.SeedPacket != null && _cactus is null)
            {
                drawer.SetListeningActive(false);
                _allListenableScripts.ForEach(l => l.SetListeningActive(false));
                _seeds[0].SetActive(false);
                _drawerPoof.SetActive(true);
                _beaAnimator.SetTrigger("SeedColor");
                yield return new WaitForSeconds(1.5f);

                _beaSpeech.SetSpeechStay("Harold, which one should I choose?", "Bea_Question", .5f);
                _harold.SetSelectSeedsMode();
                _harold.HintDefinitions = new List<HintDefinition>();
                _harold.HintDefinitions.Add(_hints[3]);

                while (_colorChoice == "") yield return null;

                yield return new WaitForSeconds(2.5f);

                _beaAnimator.SetTrigger("Bea_Stands");
                switch (_colorChoice)
                {
                    case "red":
                        _beaSpeech.SetSpeech("Red sounds right!", 3, "Bea_Surprise", .5f);
                        break;
                    case "yellow":
                        _beaSpeech.SetSpeech("Yes yes - yellow it is!", 3, "Bea_Surprise", .5f);
                        break;
                    case "blue":
                        _beaSpeech.SetSpeech("Blue sounds beautiful!", 3, "Bea_Surprise", .5f);
                        break;
                }

                _seeds[1].SetActive(true);
                _checkmarks[1].SetChecked(true);
                _harold.SetTranscriptionMode();
                _harold.HintDefinitions = new List<HintDefinition>();
                if (!_checkmarks[0].IsChecked) _harold.HintDefinitions.Add(_hints[0]); /// move item
                if (!_checkmarks[2].IsChecked) _harold.HintDefinitions.Add(_hints[1]); /// water hose
                _allListenableScripts.ForEach(l => l.SetListeningActive(true));
                drawer.SetListeningActive(true);
            }
        }

        private IEnumerator WaterFill(WaterFaucet water)
        {
            water.SetListeningActive(false);

            while (!_checkmarks[2].IsChecked)
                yield return null;

            _waterCan.SetActive(true);

            _harold.HintDefinitions.Remove(_hints[1]);

            _beaAnimator.SetTrigger("Cheers");
            _waitForAnimation = true;
            _beaSpeech.SetSpeech("Just what I needed!", 3, "Bea_Wow", .5f);
            while (_waitForAnimation) yield return null;
        }

        private void Setup()
        {
            _allListenableScripts.ForEach(l => l.SetListeningActive(false));

            _seeds[1].SetActive(false);
            _sacks[0].SetActive(true);
            _sacks[1].SetActive(false);

            _plantsMaterial.SetFloat("_Alive_Dead_Lerp", 0);

            _hintTimer = new Progress(PlayHint, _hintNarrationTimeout);
        }

        private bool TasksComplete()
        {
            var complete = true;
            _checkmarks.ForEach(c =>
            {
                if (!c.IsChecked) complete = false;
            });
            return complete;
        }

        private Cactus GetCactus(string color)
        {
            Cactus cactus = null;
            _cacti.ForEach(c =>
            {
                if (c.ColorTag == color) cactus = c;
            });
            return cactus;
        }

        private void PlayHint(float p)
        {
            if (p == 1) AudioManager.Instance.Play("Narrator_BirdHint", transform);
        }

        protected override void LevelWillUnload(float delay)
        {
            _inTransition = true;

            AudioManager.Instance.StopNarration();
            AudioManager.Instance.StopAllSpatial();
        }
    }
}
