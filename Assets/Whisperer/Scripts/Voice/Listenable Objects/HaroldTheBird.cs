/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections;
using System.Collections.Generic;
using Meta.WitAi;
using Meta.WitAi.Json;
using Meta.WitAi.TTS;
using Meta.WitAi.TTS.Utilities;
using Oculus.Voice;
using UnityEngine;

namespace Whisperer
{
    public class HaroldTheBird : Listenable
    {
        [SerializeField] private SpeechBubble _speechBubble;
        [SerializeField] private Animator _animator;
        [SerializeField] private Vector2 _idleAnimMinMax = new(15, 30);

        public List<HintDefinition> HintDefinitions = new();

        [SerializeField] private bool _sleeping;
        [SerializeField] private ResponseDefinition _birdNameResponses;
        [SerializeField] private ResponseDefinition _prettyBirdResponses;
        [SerializeField] private ResponseDefinition _genericResponses;
        [SerializeField] private ResponseDefinition _pollyCrackerResponse;
        [SerializeField] private ResponseDefinition _helloResponses;

        [SerializeField] private TTSSpeaker _speaker;

        private int _hintIndex,
            _hintListIndex,
            _genericResponseIndex,
            _helloThereResponseIndex,
            _birdNameIndex,
            _prettyBirdIndex,
            _pollyCrackerIndex;

        private IEnumerator _idleRoutine;

        private string _lastWitTranscription;

        private List<Listenable> _listenables = new();

        private List<Response> _randomizedBirdNameResponses;
        private List<Response> _randomizedGenericResponses;
        private List<Response> _randomizedHelloResponses;
        private List<Response> _randomizedPollyCrackerResponse;
        private List<Response> _randomizedPrettyBirdResponses;

        private bool _selectingSeeds,
            _transcriptionMode,
            _quietMode;

        protected void Awake()
        {
            HintDefinitions = new List<HintDefinition>();
        }

        protected override void Start()
        {
            base.Start();

            // Set up refs
            _appVoiceExperience = FindObjectOfType<AppVoiceExperience>();

            // Randomize Harold Responses
            RandomizeResponses();

            // Sub to events
            _appVoiceExperience.VoiceEvents.onFullTranscription.AddListener(CacheTranscription);
            LevelLoader.Instance.OnLevelLoadComplete.AddListener(FindSceneListenables);

            if (_sleeping) Sleep();
            else Idle();
        }

        protected override void OnDestroy()
        {
            LevelLoader.Instance?.OnLevelLoadComplete.RemoveListener(FindSceneListenables);
            _listenables.ForEach(listenable => UnsubToListenableEvents(listenable));
            _appVoiceExperience?.VoiceEvents.onFullTranscription.RemoveListener(CacheTranscription);

            base.OnDestroy();
        }

        #region Wit Event Handling

        private void FindSceneListenables()
        {
            _listenables = new List<Listenable>(FindObjectsOfType<Listenable>());
            _listenables.ForEach(listenable => SubToListenableEvents(listenable));
        }

        private void SubToListenableEvents(Listenable listenable)
        {
            listenable.OnResponseProcessed.AddListener(HandleListenerProcessed);
        }

        private void UnsubToListenableEvents(Listenable listenable)
        {
            listenable.OnResponseProcessed.RemoveListener(HandleListenerProcessed);
        }

        private void HandleListenerProcessed(ListenableEventArgs eventArgs)
        {
            if (_selectingSeeds ||
                !_transcriptionMode ||
                eventArgs.Action == "help" ||
                eventArgs.Action == "ask_bird_name" ||
                eventArgs.Action == "pretty_bird" ||
                eventArgs.Action == "wake_up_bea" ||
                eventArgs.Action == "bird_song" ||
                eventArgs.Action == "polly_want_cracker" ||
                eventArgs.Action == "say_something"
               )
                return;

            /// Harold repeats successfully performed actions
            var action = eventArgs.Action;
            var haroldPhrase = action.Replace("_", " ").ToUpper() + "!!";

            if (eventArgs.Success && eventArgs.Action != "") RepeatIntent(haroldPhrase);
        }

        /// <summary>
        ///     Determine the action to perform based on Wit's reponse
        /// </summary>
        /// <param name="witResponse"></param>
        protected override void DetermineAction(WitResponseNode witResponse)
        {
            Debug.Log(witResponse.ToString());
            var data = witResponse.GetFirstIntentData();
            var action = data == null ? "" : data.name;

            Debug.Log(action);

            // If wit response contains a color, set action to the specific color for scene 3's color selection
            var color = witResponse.GetFirstEntityValue("color:color");

            switch (color)
            {
                case "yellow":
                    break;
                case "red":
                    break;
                case "blue":
                    break;
                default:
                    color = "none";
                    break;
            }

            if (_selectingSeeds && color != "none") action = color;
            if (action == "say_something")
            {
                var whatToSay = witResponse.GetFirstEntityValue("something:something");
                SaySomething(action, whatToSay);
            }
            else
            {
                HandleAction(action);
            }
        }

        protected override void HandleAction(string action)
        {
            Debug.Log(action);

            var success = true;

            switch (action)
            {
                case "help":
                    success = GiveHint();
                    Debug.Log("sucess = " + success);
                    break;

                case "blue":
                    success = SelectSeedColor(action);
                    break;

                case "yellow":
                    success = SelectSeedColor(action);
                    break;

                case "red":
                    success = SelectSeedColor(action);
                    break;

                case "wake_up_bea":
                    success = WakeUpBea();
                    break;

                case "bird_song":
                    success = SingASong();
                    break;

                case "ask_bird_name":
                    success = AskBirdName();
                    break;

                case "pretty_bird":
                    success = PrettyBird();
                    break;

                case "generic_response":
                    success = GenericResponse();
                    break;

                case "hello":
                    success = Hello();
                    break;

                case "polly_want_cracker":
                    success = PollyWantCracker();
                    break;

                default:
                    success = false;
                    break;
            }

            ProcessComplete(action, success);
        }

        #endregion

        #region Harold Actions

        private bool SelectSeedColor(string color)
        {
            if (!_selectingSeeds)
                return false;

            SetSpeechText(color.ToUpper() + "!!");
            return true;
        }

        public bool GiveHint()
        {
            if (HintDefinitions.Count == 0) return false;
            Debug.Log("about to give hint");
            /// practice safe indexing
            _hintListIndex = Mathf.Min(_hintListIndex, HintDefinitions.Count - 1);
            _hintIndex = Mathf.Min(_hintIndex, HintDefinitions[_hintListIndex].hints.Count - 1);

            SetSpeechText(HintDefinitions[_hintListIndex].hints[_hintIndex]);

            /// index up!
            _hintIndex = (_hintIndex + 1) % HintDefinitions[_hintListIndex].hints.Count;
            if (_hintIndex == 0) _hintListIndex = (_hintListIndex + 1) % HintDefinitions.Count;

            return true;
        }

        private bool WakeUpBea()
        {
            if (_quietMode)
                DontWakeBeaResponse();
            else
                MakeNoise();

            return true;
        }

        private bool SingASong()
        {
            if (_quietMode)
                DontWakeBeaResponse();
            else
                Sing();

            return true;
        }

        private void DontWakeBeaResponse()
        {
            Invoke("Squack", 0.5f);
            _witUI.FadeOut();
            _speechBubble.SetSpeech("A noisy bird might wake up Bea", 3f);
        }

        private void RepeatIntent(string intent)
        {
            Invoke("Squack", 0.5f);
            _witUI.FadeOut();
            _speechBubble.SetSpeech(intent, 2f);
        }

        private bool AskBirdName()
        {
            if (_randomizedBirdNameResponses.Count == 0) return false;

            HaroldResponds(_randomizedBirdNameResponses[_birdNameIndex].Phrases);
            _birdNameIndex = (_birdNameIndex + 1) % _randomizedBirdNameResponses.Count;

            return true;
        }

        private bool PrettyBird()
        {
            if (_randomizedPrettyBirdResponses.Count == 0) return false;

            HaroldResponds(_randomizedPrettyBirdResponses[_prettyBirdIndex].Phrases);
            _prettyBirdIndex = (_prettyBirdIndex + 1) % _randomizedPrettyBirdResponses.Count;

            return true;
        }

        private bool GenericResponse()
        {
            if (_randomizedGenericResponses.Count == 0) return false;

            HaroldResponds(_randomizedGenericResponses[_genericResponseIndex].Phrases);
            _genericResponseIndex = (_genericResponseIndex + 1) % _randomizedGenericResponses.Count;

            return true;
        }

        private bool Hello()
        {
            if (_randomizedHelloResponses.Count == 0) return false;

            HaroldResponds(_randomizedHelloResponses[_helloThereResponseIndex].Phrases);
            _helloThereResponseIndex = (_helloThereResponseIndex + 1) % _randomizedHelloResponses.Count;

            return true;
        }

        private bool PollyWantCracker()
        {
            if (_randomizedPollyCrackerResponse.Count == 0) return false;

            HaroldResponds(_randomizedPollyCrackerResponse[_pollyCrackerIndex].Phrases);
            _pollyCrackerIndex = (_pollyCrackerIndex + 1) % _randomizedPollyCrackerResponse.Count;

            return true;
        }

        private void SaySomething(string action, string whatToSay)
        {
            if (string.IsNullOrEmpty(whatToSay))
            {
                ProcessComplete(action, false);
                return;
            }
            
            Invoke("Squack", 0.5f);
            if (_speaker == null)
            {
                _speaker = FindObjectOfType<TTSSpeaker>();
            }
            _speaker.Speak(whatToSay);
            ProcessComplete(action, true);
        }

        #endregion

        #region Harold Speech

        private void HaroldResponds(List<string> phrases)
        {
            var i = 0;
            var delayBeforeShow = 0f;

            // Queu each phrase
            foreach (var phrase in phrases)
            {
                var phraseLength = phrase.Length;

                var waitBeforeFadeout = Mathf.Clamp(phraseLength * .06f, 2f, 4f) + .2f;
                delayBeforeShow += i == 0 ? 0 : waitBeforeFadeout;

                var haroldPhrase = phrases[i];
                var sayPhrase = DelayThenSetSpeechText(haroldPhrase, delayBeforeShow, waitBeforeFadeout);
                StartCoroutine(sayPhrase);
                i++;
            }
        }

        private IEnumerator DelayThenSetSpeechText(string speechText, float showDelay, float hideDelay)
        {
            yield return new WaitForSeconds(showDelay);
            Invoke("Squack", 0f);
            _witUI.FadeOut();
            _speechBubble.SetSpeech(speechText, hideDelay);
        }

        private void SetSpeechText(string speechText)
        {
            Invoke("Squack", 0.5f);
            _witUI.FadeOut();
            _speechBubble.SetSpeech(speechText, 2f);
        }

        private void CacheTranscription(string transcription)
        {
            _lastWitTranscription = transcription;
        }

        #endregion

        #region Animation Triggers

        public void Sleep()
        {
            _animator.SetTrigger("Sleep");
        }

        public void WakeUp()
        {
            _animator.SetTrigger("Awake");
        }

        public void Squack()
        {
            _animator.SetTrigger("Squack");
        }

        public void MakeNoise()
        {
            _animator.SetTrigger("Talk");
            AudioManager.Instance.Play("Make_Noise", _witUI.FollowTransform);
        }

        public void Sing()
        {
            _animator.SetTrigger("Sing");
            AudioManager.Instance.Play("HaroldSing", _witUI.FollowTransform);
        }

        public void Idle()
        {
            _idleRoutine = IdleRoutine();
            StartCoroutine(_idleRoutine);
        }

        private IEnumerator IdleRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(_idleAnimMinMax.x, _idleAnimMinMax.y));
                _animator.SetTrigger("Idle_Alt");
            }
        }

        #endregion

        #region Set Harold Modes

        public void SetTranscriptionMode()
        {
            _transcriptionMode = true;
            _selectingSeeds = false;
        }

        public void SetSelectSeedsMode()
        {
            SetListeningActive(true);
            _transcriptionMode = false;
            _selectingSeeds = true;
        }

        public void SetQuietMode(bool enabled)
        {
            _quietMode = enabled;
        }

        #endregion

        #region Utility

        private void RandomizeResponses()
        {
            _randomizedBirdNameResponses = RandomizeResponseList(_birdNameResponses.Responses);
            _randomizedPrettyBirdResponses = RandomizeResponseList(_prettyBirdResponses.Responses);
            _randomizedGenericResponses = RandomizeResponseList(_genericResponses.Responses);
            _randomizedPollyCrackerResponse = RandomizeResponseList(_pollyCrackerResponse.Responses);
            _randomizedHelloResponses = RandomizeResponseList(_helloResponses.Responses);
        }

        public static List<Response> RandomizeResponseList(List<Response> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var temp = list[i];
                var rand = Random.Range(i, list.Count);
                list[i] = list[rand];
                list[rand] = temp;
            }

            return list;
        }

        #endregion
    }
}
