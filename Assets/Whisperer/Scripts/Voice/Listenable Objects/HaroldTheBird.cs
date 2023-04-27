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
        private const string SELECT_COLOR_INTENT = "select_color";
        private const string SELECT_SEED_INTENT = "select_seed";
        private const string HELP_INTENT = "help";
        private const string ASK_BIRD_NAME_INTENT = "ask_bird_name";
        private const string PRETTY_BIRD_INTENT = "pretty_bird";
        private const string WAKE_UP_BEA_INTENT = "wake_up_bea";
        private const string BIRD_SONG_INTENT = "bird_song";
        private const string POLLY_WANT_CRACKER_INTENT = "polly_want_cracker";
        private const string SAY_SOMETHING_INTENT = "say_something";
        private const string GENERIC_RESPONSE_INTENT = "generic_response";
        private const string HELLO_INTENT = "hello";

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

            if (_speaker == null)
            {
                _speaker = FindObjectOfType<TTSSpeaker>();
            }
            _speaker.Events.OnTextPlaybackStart.AddListener(OnTextPlaybackStart);
            _speaker.Events.OnTextPlaybackFinished.AddListener(OnTextPlaybackStop);

        }

        protected override void OnDestroy()
        {
            LevelLoader.Instance?.OnLevelLoadComplete.RemoveListener(FindSceneListenables);
            _listenables.ForEach(listenable => UnsubToListenableEvents(listenable));
            _appVoiceExperience?.VoiceEvents.onFullTranscription.RemoveListener(CacheTranscription);
            _speaker.Events.OnTextPlaybackStart.RemoveListener(OnTextPlaybackStart);
            _speaker.Events.OnTextPlaybackFinished.RemoveListener(OnTextPlaybackStop);

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
                eventArgs.Action == HELP_INTENT ||
                eventArgs.Action == ASK_BIRD_NAME_INTENT ||
                eventArgs.Action == PRETTY_BIRD_INTENT ||
                eventArgs.Action == WAKE_UP_BEA_INTENT ||
                eventArgs.Action == BIRD_SONG_INTENT ||
                eventArgs.Action == POLLY_WANT_CRACKER_INTENT ||
                eventArgs.Action == SAY_SOMETHING_INTENT
               )
                return;

            /// Harold repeats successfully performed actions
            var action = eventArgs.Action;
            var haroldPhrase = action.Replace("_", " ").ToUpper() + "!!";

            if (eventArgs.Success && eventArgs.Action != "") RepeatIntent(haroldPhrase);
        }

        protected override void HandleAction(string action)
        {
            Debug.Log(action);
        }

        #endregion

        #region Harold Actions

        [MatchIntent(SELECT_COLOR_INTENT)]
        [MatchIntent(SELECT_SEED_INTENT)]
        public void SelectSeedColor(SeedColor color)
        {
            if (!_selectingSeeds)
            {
                ProcessComplete(SELECT_COLOR_INTENT, false);
                return;
            }

            SetSpeechText(color.ToString().ToUpper() + "!!");
            ProcessComplete(color.ToString(), true);
        }

        [MatchIntent(HELP_INTENT)]
        public void GiveHint()
        {
            if (HintDefinitions.Count == 0)
            {
                ProcessComplete(HELP_INTENT, false);
                return;
            }
            Debug.Log("about to give hint");
            /// practice safe indexing
            _hintListIndex = Mathf.Min(_hintListIndex, HintDefinitions.Count - 1);
            _hintIndex = Mathf.Min(_hintIndex, HintDefinitions[_hintListIndex].hints.Count - 1);

            SetSpeechText(HintDefinitions[_hintListIndex].hints[_hintIndex]);

            /// index up!
            _hintIndex = (_hintIndex + 1) % HintDefinitions[_hintListIndex].hints.Count;
            if (_hintIndex == 0) _hintListIndex = (_hintListIndex + 1) % HintDefinitions.Count;

            ProcessComplete(HELP_INTENT, true);
        }

        [MatchIntent(WAKE_UP_BEA_INTENT)]
        public void WakeUpBea()
        {
            if (_quietMode)
                DontWakeBeaResponse();
            else
                MakeNoise();

            ProcessComplete(WAKE_UP_BEA_INTENT, true);
        }

        [MatchIntent(BIRD_SONG_INTENT)]
        public void SingASong()
        {
            if (_quietMode)
                DontWakeBeaResponse();
            else
                Sing();

            ProcessComplete(BIRD_SONG_INTENT, true);
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

        [MatchIntent(ASK_BIRD_NAME_INTENT)]
        public void AskBirdName()
        {
            if (_randomizedBirdNameResponses.Count == 0)
            {
                ProcessComplete(ASK_BIRD_NAME_INTENT, false);
                return;
            };

            HaroldResponds(_randomizedBirdNameResponses[_birdNameIndex].Phrases);
            _birdNameIndex = (_birdNameIndex + 1) % _randomizedBirdNameResponses.Count;

            ProcessComplete(ASK_BIRD_NAME_INTENT, true);
        }

        [MatchIntent(PRETTY_BIRD_INTENT)]
        public void PrettyBird()
        {
            if (_randomizedPrettyBirdResponses.Count == 0)  {
                ProcessComplete(PRETTY_BIRD_INTENT, false);
                return ;
            }

            HaroldResponds(_randomizedPrettyBirdResponses[_prettyBirdIndex].Phrases);
            _prettyBirdIndex = (_prettyBirdIndex + 1) % _randomizedPrettyBirdResponses.Count;

            ProcessComplete(PRETTY_BIRD_INTENT, true);
        }

        [MatchIntent(GENERIC_RESPONSE_INTENT)]
        public void GenericResponse()
        {
            if (_randomizedGenericResponses.Count == 0)
            {
                ProcessComplete(GENERIC_RESPONSE_INTENT, false);
                return ;
            }

            HaroldResponds(_randomizedGenericResponses[_genericResponseIndex].Phrases);
            _genericResponseIndex = (_genericResponseIndex + 1) % _randomizedGenericResponses.Count;
            ProcessComplete(GENERIC_RESPONSE_INTENT, true);

        }

        [MatchIntent(HELLO_INTENT)]
        public void Hello()
        {
            if (_randomizedHelloResponses.Count == 0)
            {
                ProcessComplete(HELLO_INTENT, false);
                return ;
            }

            HaroldResponds(_randomizedHelloResponses[_helloThereResponseIndex].Phrases);
            _helloThereResponseIndex = (_helloThereResponseIndex + 1) % _randomizedHelloResponses.Count;

            ProcessComplete(HELLO_INTENT, true);
        }

        [MatchIntent(POLLY_WANT_CRACKER_INTENT)]
        public void PollyWantCracker()
        {
            if (_randomizedPollyCrackerResponse.Count == 0)
            {
                ProcessComplete(POLLY_WANT_CRACKER_INTENT, false);
                return ;
            }

            HaroldResponds(_randomizedPollyCrackerResponse[_pollyCrackerIndex].Phrases);
            _pollyCrackerIndex = (_pollyCrackerIndex + 1) % _randomizedPollyCrackerResponse.Count;

            ProcessComplete(POLLY_WANT_CRACKER_INTENT, true);
        }

        [MatchIntent(SAY_SOMETHING_INTENT)]
        public void SaySomething(WitResponseNode witResponse)
        {
            var whatToSay = witResponse.GetFirstEntityValue("something:something");

            if (string.IsNullOrEmpty(whatToSay))
            {
                ProcessComplete(SAY_SOMETHING_INTENT, false);
                return;
            }

            _speaker.Speak(whatToSay);
            ProcessComplete(SAY_SOMETHING_INTENT, true);

        }

        void OnTextPlaybackStart(string text)
        {
            _animator.SetTrigger("Talk");
        }
        void OnTextPlaybackStop(string text)
        {
            _animator.SetTrigger("Idle_Alt");
            Invoke("Squack", 0.5f);
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
            _witUI.FadeOut();
            _speechBubble.SetSpeech(speechText, hideDelay);
            _speaker.Speak(speechText);
        }

        private void SetSpeechText(string speechText)
        {
            _witUI.FadeOut();
            _speechBubble.SetSpeech(speechText, 2f);
            _speaker.Speak(speechText);
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
