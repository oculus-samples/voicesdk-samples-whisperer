/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{
	public class TimerUtils : Singleton<TimerUtils>
	{
		public List<Progress> ActiveProgress = new List<Progress>();
		
		private void Update()
		{
			for (int i = ActiveProgress.Count - 1; i >= 0; i--)
			{
				ActiveProgress[i].Update(Time.deltaTime);
			}
		}
	}

	[Serializable]
	public class Progress
	{
		bool _haveCallback,
			 _registered;
		Action<float> _callback;

		public Progress(Action<float> callback, float duration = 1)
		{
			Callback = callback;
			_duration = duration;
			_haveCallback = true;
		}

		public Progress(float duration = 1)
		{
			_duration = duration;
		}

		public bool Enabled = true;
		/// <summary>
		/// Method taking float parameter (progress) called every frame this object is updated.
		/// </summary>
		public Action<float> Callback { get { return _callback; } set { _callback = value; _haveCallback = true; } }
		/// <summary>
		/// Current progress value;
		/// </summary>
		public float Value { get => _progress; }
		/// <summary>
		/// Is playback paused?
		/// </summary>
		public bool Paused { get => _direction == 0; }
		/// <summary>
		/// The current duration value. 
		/// </summary>
		public float Duration { get => _duration; set => _duration = value; }
		/// <summary>
		/// Is playback currently running?
		/// </summary>
		public bool Playing { get => _direction != 0; }
		/// <summary>
		/// Should playback loop?
		/// </summary>
		public bool Loop;
		/// <summary>
		/// Should playback pingpong?
		/// </summary>
		public bool PingPong;
		/// <summary>
		/// Current direction value. Forward = 1, Reverse = -1, Paused = 0;
		/// </summary>
		public int Direction { get => _direction; }

		[SerializeField]
		float _progress,
			  _duration;
		[SerializeField]
		int _direction,
			_lastDirection = 1;
		[SerializeField]
		bool _active;

		/// <summary>
		/// Called automatically by TimerUtils manager.
		/// </summary>
		/// <param name="deltaTime"></param>
		public void Update(float deltaTime)
		{
			if (Enabled)
			{
				if (_duration == 0)
				{
					Loop = false;
					PingPong = false;
					_duration = 0.00001f;
				}
				_progress += _direction * (deltaTime / _duration);
				_progress = Mathf.Clamp01(_progress);
				if (_progress == 0 || _progress == 1) HandleProgressEnd();
				InvokeCallback();
			}
		}
		/// <summary>
		/// Manually set the progress value. This will update the assigned callback function.
		/// </summary>
		/// <param name="progress"></param>
		public void SetProgress(float progress)
		{
			_progress = Mathf.Clamp01(progress);
			InvokeCallback();
		}
		/// <summary>
		/// Start progress toward 1 from the current progress value, scaled such that 0->1 is completed in <paramref name="duration"/> (seconds).
		/// </summary>
		/// <param name="duration"></param>
		public void Play(float duration, bool autoReset = true)
		{
			if (autoReset && _progress == 1) _progress = 0;
			_duration = duration;
			SetDirection(1);
		}
		/// <summary>
		///  Start progress toward 1 from the current progress value, using the previously specified duration (default is 1sec).
		/// </summary>
		public void Play(bool autoReset = true)
		{
			if (autoReset && _progress == 1) _progress = 0;
			SetDirection(1);
		}
		/// <summary>
		/// Start progress toward 0 from the current progress value, scaled such that 1->0 is completed in <paramref name="duration"/> (seconds).
		/// </summary>
		/// <param name="duration"></param>
		public void PlayReverse(float duration, bool autoReset = true)
		{
			if (autoReset && _progress == 0) _progress = 1;
			_duration = duration;
			SetDirection(-1);
		}
		/// <summary>
		///  Start progress toward 0 from the current progress value, using the previously specified duration (default is 1sec).
		/// </summary>
		public void PlayReverse(bool autoReset = true)
		{
			if (autoReset && _progress == 0) _progress = 1;
			SetDirection(-1);
		}
		/// <summary>
		/// Start progress toward 1 from 0, scaled such that 0->1 is completed in <paramref name="duration"/> (seconds).
		/// </summary>
		/// <param name="duration"></param>
		public void PlayFrom0(float duration)
		{
			_progress = 0;
			Play(duration);
		}
		/// <summary>
		/// Start progress toward 1 from 0, using the previously specified duration (default is 1sec).
		/// </summary>
		public void PlayFrom0()
		{
			_progress = 0;
			Play();
		}
		/// <summary>
		/// Start progress toward 0 from 1, scaled such that 0->1 is completed in <paramref name="duration"/> (seconds).
		/// </summary>
		/// <param name="duration"></param>
		public void PlayFrom1(float duration)
		{
			_progress = 1;
			PlayReverse(duration);
		}
		/// <summary>
		/// Start progress toward 0 from 1, using the previously specified duration (default is 1sec).
		/// </summary>
		public void PlayFrom1()
		{
			_progress = 1;
			PlayReverse();
		}
		/// <summary>
		/// Immediately reverse the current direction of play (i.e. 0->1 becomes 1->0).
		/// </summary>
		public void Reverse()
		{
			SetDirection(_direction * -1);
		}
		/// <summary>
		/// Pause progress, retaining the current progress value.
		/// </summary>
		public void Pause()
		{
			SetDirection(0);
		}
		/// <summary>
		/// Toggle pause of progress, retaining current progress value.
		/// </summary>
		public void PauseToggle()
		{
			if (!Paused) SetDirection(0);
			else SetDirection(_lastDirection);
		}
		/// <summary>
		/// Set paused state to (bool) <paramref name="paused"/>.
		/// </summary>
		/// <param name="paused"></param>
		public void SetPaused(bool paused)
		{
			SetDirection(paused ? 0 : _lastDirection);
		}
		/// <summary>
		/// Resume progress, if paused, using all current values.
		/// </summary>
		public void Resume()
		{
			if (!Paused) return;
			SetDirection(_lastDirection);
		}
		/// <summary>
		/// Stop playback and set progress to starting value based on current direction (i.e. if forward, progress will set to 0).
		/// </summary>
		public void Reset()
		{
			int direction = Paused ? _lastDirection : _direction;
			_progress = direction == 1 ? 0 : 1;
			SetDirection(0);
			InvokeCallback();
		}
		/// <summary>
		/// Stop playback, set progress to 0 and play direction to forward.
		/// </summary>
		public void ResetTo0()
		{
			SetDirection(1);
			Reset();
		}
		/// <summary>
		/// Stop playback, set progress to 1 and play direction to reverse.
		/// </summary>
		public void ResetTo1()
		{
			SetDirection(-1);
			Reset();
		}
		void HandleProgressEnd()
		{
			if (PingPong)
			{
				SetDirection(_direction * -1);
			}
			else if (Loop)
			{
				_progress = _direction == 1 ? 0 : 1;
			}
			else
			{
				SetDirection(0);
			}
		}
		void SetDirection(int newDirection)
		{
			if (Enabled)
			{
				if (_direction != 0) _lastDirection = _direction;
				_direction = newDirection;
				if (_direction == 0) TimerUtils.Instance?.ActiveProgress.Remove(this);
				else if (!_active) TimerUtils.Instance?.ActiveProgress.Add(this);

				_active = _direction != 0;
			}
		}
		void InvokeCallback()
		{
			if (Enabled)
			{
				if (_haveCallback)
					_callback.Invoke(_progress);
			}
		}
	}
}
