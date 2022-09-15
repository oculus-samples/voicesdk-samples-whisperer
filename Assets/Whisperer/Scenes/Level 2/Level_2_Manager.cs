/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Internal
/// Needs animation event audio:
/// 
/// </summary>

namespace Whisperer
{
	public class Level_2_Manager : LevelManager
	{
		[SerializeField] Animator _beaAnimator;
		[SerializeField] SpeechBubble _beaSpeech;
		[SerializeField] Transform _beaNeckJoint;
		[SerializeField] Radio _radio;
		[SerializeField] HaroldTheBird _harold;
		[SerializeField] Chalkboard _chalkBoard;
		[SerializeField] List<Checkmark> _checkmarks;
		[SerializeField] HeroPlant _tablePlant;
		[SerializeField] Animator _waterPlantAnimator;
		[SerializeField] Animator _chestPlantAnimator;
		[SerializeField] TreasureChest _chest;
		[SerializeField] WaterFaucet _water;
		[SerializeField] List<Listenable> _actionItems;
		[SerializeField] Material _plantsMaterial;
		[SerializeField] List<HintDefinition> _hints;
		[SerializeField] float _hintNarrationTimeout = 20f;
		
		int _completeCount,
			_warningCount;

		bool _beaIsSleeping,
			 _waitForAnimation,
			 _noticeRunning,
			 _haroldWakeUpBea;
		IEnumerator _levelRoutine;
		IEnumerator _noticeRoutine;
		Progress _hintTimer;

		protected override void Start()
		{
			base.Start();

			_speakGestureWatcher.AllowSpeak = !_levelLogicEnabled;

			if (_levelLogicEnabled) Setup();

			_plantsMaterial.SetFloat("_Alive_Dead_Lerp", 1);

			_hintTimer = new Progress(PlayHint, _hintNarrationTimeout);

			UXManager.Instance.SetDisplayEnabled("SettingsMenu", true);	
		}
		
		public override void StartLevel()
		{
			_hands.SetSpeak();
			_speakGestureWatcher.AllowSpeak = true;

			FindObjectOfType<CameraColorOverlay>().SetTargetColor(Color.black);
		
			GameObject.Find("Hero Drawers").SetActive(false);

			_actionItems = new List<Listenable>();
			_actionItems.Add(_tablePlant = FindObjectOfType<HeroPlant>());
			_actionItems.Add(_chest = FindObjectOfType<TreasureChest>());
			_actionItems.Add(_water = FindObjectOfType<WaterFaucet>());
			_actionItems.ForEach(a => a.IsActionable = false);

			_harold.HintDefinitions = new List<HintDefinition>();

			_levelRoutine = LevelRoutine();
			StartCoroutine(_levelRoutine);
		}
		
		/// <summary>
		/// Main narrative logic
		/// </summary>
		/// <returns></returns>		
		private IEnumerator LevelRoutine()
		{
			yield return new WaitForSeconds(1);

			/// Intro setup
			yield return new WaitForSeconds(
				AudioManager.Instance.Play("Narrator_GainTrust", transform));

			/// Start the quest
			_chalkBoard.FadeToNext(2);
			_allListenableScripts.ForEach(l => l.SetListeningActive(true));
			_radio.SetListeningActive(true);
			_harold.SetTranscriptionMode();
			_harold.HintDefinitions.Add(_hints[0]);
			_hintTimer.PlayFrom0();

			/// Waiting for radio to turn on
			while (!_radio.IsOn)
				yield return null;

			/// Radio is on, nice music, Bea notices
			_radio.SetListeningActive(false);
			_beaAnimator.SetTrigger("Bea_Notices");
			_waitForAnimation = true;
			while (_waitForAnimation) yield return null;

			yield return new WaitForSeconds(AudioManager.Instance.Play("Narrator_RadioHint1", transform));

			/// Waiting for station change 1
			_radio.SetListeningActive(true);
			while (_radio.StationIndex != 1 || !_radio.IsOn)
				yield return null;

			/// Radio is on, bad music, Bea annoyed
			_beaAnimator.SetTrigger("Bea_Annoyed");
			_waitForAnimation = true;

			yield return new WaitForSeconds(1);
			yield return new WaitForSeconds(AudioManager.Instance.Play("Narrator_RadioHint2", transform));

			while (_waitForAnimation) yield return null;

			/// Waiting for station change 2
			while (_radio.StationIndex != 2 || !_radio.IsOn)
				yield return null;

			_radio.SetListeningActive(false);
			_actionItems.ForEach(a => a.SetListeningActive(false));
			_harold.SetQuietMode(true);

			/// Bea falls asleep
			_beaAnimator.SetTrigger("Bea_Sleep");
			_waitForAnimation = true;
			_hintTimer.Pause();

			while (_waitForAnimation) yield return null;

			_beaIsSleeping = true;
			_checkmarks[0].SetChecked(true);
			_harold.HintDefinitions.Remove(_hints[0]);
			_harold.HintDefinitions.Add(_hints[1]);
			_harold.HintDefinitions.Add(_hints[2]);
			_harold.HintDefinitions.Add(_hints[3]);
			AudioManager.Instance.Play("Bea_Snore", _beaNeckJoint, 0.3f);

			yield return new WaitForSeconds(
				AudioManager.Instance.Play("Narrator_GetToWork", transform));

			/// Enable all action items
			_chalkBoard.FadeToNext(2);
			_checkmarks[0].SetChecked(false);
			_actionItems.ForEach(a =>
			{
				a.SetListeningActive(true);
				a.IsActionable = true;
			});
			_hintTimer.PlayFrom0();

			/// Watch for placeable plant moved to sun spot
			_tablePlant.OnPlacedOnSpot.AddListener(() =>
			{
				_completeCount++;
				_checkmarks[1].SetChecked(true);
				_harold.HintDefinitions.Remove(_hints[1]);
			});

			/// Game puzzles
			while (_completeCount < 1)
				yield return null;

			yield return new WaitForSeconds(
				AudioManager.Instance.Play("Narrator_OneDown", transform));

			while (_completeCount < 2)
				yield return null;

			yield return new WaitForSeconds(
				AudioManager.Instance.Play("Narrator_LastOne", transform));

			while (_completeCount < 3)
				yield return null;

			/// Mission completed
			yield return new WaitForSeconds(
			AudioManager.Instance.Play("Narrator_WakeUpBea", transform));

			/// Wait for Harold to make noise OR radio change
			_chalkBoard.FadeToNext(2);
			for (int i = 1; i <= 3; i++) { _checkmarks[i].SetChecked(false); }

			_harold.SetQuietMode(false);
			_harold.HintDefinitions.Add(_hints[4]);
			_radio.SetListeningActive(true);
			while (!_haroldWakeUpBea && (_radio.StationIndex == 2 && _radio.IsOn))
				yield return null;

			/// This chunk for debug/tuning ending timings below. Comment out above.

			//_beaAnimator.SetTrigger("Bea_Annoyed");
			//_waitForAnimation = true;
			//while (_waitForAnimation) yield return null;
			//_beaIsSleeping = true;
			//_beaAnimator.SetTrigger("Bea_Sleep");
			//_waitForAnimation = true;
			//while (_waitForAnimation) yield return null;
			//_beaIsSleeping = true;

			_hintTimer.Pause();
			_hintTimer = null;

			AudioManager.Instance.Stop("Bea_Snore");
			_checkmarks[4].SetChecked(true);
			_beaAnimator.SetTrigger("Bea_Wake");
			_waitForAnimation = true;
			
			yield return new WaitForSeconds(5);

			yield return new WaitForSeconds(
				_beaSpeech.SetSpeech("Oh – but how...?", 4, "Bea_Yawn", 1f));

			yield return new WaitForSeconds(0.5f);

			yield return new WaitForSeconds(
			_beaSpeech.SetSpeech("My, aren't they beautiful!", 5));

			yield return new WaitForSeconds(1);

			yield return new WaitForSeconds(
			AudioManager.Instance.Play("Narrator_WellDone", transform));

			while (_waitForAnimation)
				yield return null;

			yield return new WaitForSeconds(2);

			LevelLoader.Instance.LoadNextLevel();
		}
		
		protected override void OnAnimationEvent(string eventName)
		{
			base.OnAnimationEvent(eventName);

			switch (eventName)
			{
				case "Bea_Annoyed_End":
					_waitForAnimation = false;
					break;
				case "Bea_Notices_End":
					_waitForAnimation = false;
					break;
				case "Bea_Wake_End":
					_waitForAnimation = false;
					break;
				case "Bea_Sleep_End":
					_waitForAnimation = false;
					break;
				case "Chest_Open":
					_completeCount++;
					_checkmarks[3].SetChecked(true);
					_harold.HintDefinitions.Remove(_hints[3]);
					_chest.SetListeningActive(false);
					_chestPlantAnimator.SetTrigger("Grow");
					break;
				case "Plant_Alive":
					break;
				default:
					break;
			}
		}

		protected override void OnListenableResponse(ListenableEventArgs eventArgs)
		{
			base.OnListenableResponse(eventArgs);

			/// Narrator issues warnings if radio is not set
			if (!_beaIsSleeping && eventArgs.Listenable != _radio && eventArgs.Listenable != _harold)
			{
				if (!_noticeRunning)
				{
					_noticeRoutine = BeaNotices();
					StartCoroutine(_noticeRoutine);
				}
				else
					Debug.Log("Watch out!! multiple 'BeaNotice' coroutines tried to run!!");
			}

			/// If water faucet has been turned on
			if (eventArgs.Listenable == _water && eventArgs.Action == "turn_on") 
			{
				_completeCount++;
				_checkmarks[2].SetChecked(true);
				_harold.HintDefinitions.Remove(_hints[2]);
				_water.SetListeningActive(false);
				_waterPlantAnimator.SetTrigger("Grow");
			}

			/// Bea responds to Harold
			if (eventArgs.Listenable == _harold)
			{
				if (!_beaIsSleeping && _radio.StationIndex != 2)
					Invoke("BeaRespondsToHarold", 2.5f);

				/// Wake up Bea
				if (_completeCount == 3 && (eventArgs.Action == "wake_up_bea" || eventArgs.Action == "bird_song"))
				{
					_haroldWakeUpBea = true;
					_harold.HintDefinitions.Add(_hints[4]);
				}
			}

			/// General hint timer
			if(_hintTimer != null)
			{
				if (eventArgs.Success)
					_hintTimer.PlayFrom0();
			}
		}
		
		private IEnumerator BeaNotices()
		{
			_allListenableScripts.ForEach(l => l.SetListeningActive(false));

			_noticeRunning = true;

			_beaAnimator.SetTrigger("Bea_Notices");

			yield return new WaitForSeconds(1);

			if (_radio.IsOn == false)
			{
				/// Radio still OFF
				if (_warningCount < 1)
					yield return new WaitForSeconds(AudioManager.Instance.Play("Narrator_DistractBea", transform));
				else
					yield return new WaitForSeconds(AudioManager.Instance.Play("Narrator_SpookWarning", transform));

				_warningCount++;
			}
			else
			{
				/// Radio ON
				yield return new WaitForSeconds(AudioManager.Instance.Play("Narrator_RadioHint1", transform));
			}

			_noticeRunning = false;
			_allListenableScripts.ForEach(l => l.SetListeningActive(true));
		}

		private void BeaRespondsToHarold()
		{
			if (UnityEngine.Random.value < 0.5f)
				_beaSpeech.SetSpeech("Harold, don't be silly.", 3, "Bea_Sigh", .5f);
			else
				_beaSpeech.SetSpeech("Such a funny bird...", 3, "Bea_Question", .5f);
		}

		private void Setup()
		{
			_allListenableScripts.ForEach(l => l.SetListeningActive(false));
		}

		private void PlayHint(float p)
		{
			if (p == 1) AudioManager.Instance.Play("Narrator_BirdHint", transform);
		}
	}
}

