/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using UnityEngine.InputSystem;

namespace Whisperer
{
	/// <summary>
	/// For tracking/animating UI panels bound to the XR Rig
	/// </summary>
	public class UXTrackedDisplay : MonoBehaviour
	{
		public enum TrackNode { Head, Rig }

		[SerializeField] protected GameObject _toggleObject;
		[SerializeField] protected InputActionReference _inputToggleButton;
		[SerializeField] protected bool _usesHandsRaycast;

		protected Transform _heightTrackTransform;
		protected Transform _positionTrackTransform;
		protected Transform _rotationTrackTransform;

		[Header("Position Settings")]
		[SerializeField] protected bool _continuousHeightTrack = true;
		[SerializeField] protected bool _continuousPositionTrack;
		[SerializeField] protected TrackNode _heightTrack;
		[SerializeField] protected TrackNode _positionTrack;
		[SerializeField] protected float _heightOffset;
		[SerializeField] protected float _minHeight = 0.5f;
		[SerializeField] protected float _forwardOffset = 1.25f;

		[Header("Rotation Settings")]
		[SerializeField] protected bool _continuousRotationTrack = true;
		[SerializeField] protected TrackNode _rotationTrack;
		[SerializeField] protected float _rotateTrackSpeed = 3;
		[SerializeField] protected float _rotateDotThreshold = 0.975f;

		public Transform _playerRootTransform { get; set; }
		public bool IsDisplayed { get => _isDisplayed; }
		public bool IsSceneDisplay { get; set; }
		public bool UsesHandsRaycast { get => _usesHandsRaycast; }

		protected RigHandsControl _hands;
		protected Quaternion _rotationTarget;
		protected Progress _fader;
		[SerializeField] protected bool _isDisplayed;

		protected Transform _pivot;

		protected virtual void Awake()
		{
			_toggleObject.SetActive(false);
		}

		private void OnDisable()
		{
			SetDisplayed(false);
		}

		private void OnDestroy()
		{
			if (_inputToggleButton)
				_inputToggleButton.action.performed -= ButtonPressed;

			UXManager.Instance?.RemoveDisplay(this);
		}

		protected virtual void Start()
		{
			_hands = FindObjectOfType<RigHandsControl>();

			if (_inputToggleButton)
				_inputToggleButton.action.performed += ButtonPressed;

			if (LevelLoader.Instance.IsLoading)
				UXManager.Instance?.AddDisplay(this);
		}

		public virtual void Setup(Transform playerRoot)
		{
			_playerRootTransform = playerRoot;

			_pivot = new GameObject(gameObject.name + " [pivot]").transform;
			_pivot.parent = transform.parent;
			transform.parent = _pivot;

			UpdateValues();
			Recenter();

			SetDisplayed(_isDisplayed);
		}

		protected virtual void ButtonPressed(InputAction.CallbackContext obj)
		{
			UXManager.Instance.SetDisplay(this, !_toggleObject.activeSelf);
		}

		protected virtual void Update()
		{
			if (!_toggleObject.activeSelf || _playerRootTransform is null) return;

			UpdateValues();

			if (_continuousPositionTrack)
				SetPosition();

			if (_continuousHeightTrack)
				SetHeight();

			if (_continuousRotationTrack)
			{
				float dot = Quaternion.Dot(_pivot.rotation, ViewRotation());

				if (dot < _rotateDotThreshold)
					_rotationTarget = ViewRotation();

				_pivot.rotation = Quaternion.Lerp(_pivot.rotation, _rotationTarget, Time.deltaTime * _rotateTrackSpeed);
			}
		}

		protected virtual void UpdateValues()
		{
			_heightTrackTransform = _heightTrack == TrackNode.Rig ? _playerRootTransform : Camera.main.transform;
			_positionTrackTransform = _positionTrack == TrackNode.Rig ? _playerRootTransform : Camera.main.transform;
			_rotationTrackTransform = _rotationTrack == TrackNode.Rig ? _playerRootTransform : Camera.main.transform;
		}

		protected virtual void SetHeight()
		{
			Vector3 localHeight = _pivot.InverseTransformPoint(_heightTrackTransform.position);
			transform.localPosition = new Vector3(transform.localPosition.x, localHeight.y + _heightOffset, transform.localPosition.z);
		}

		protected virtual void SetPosition()
		{
			Vector3 tPos = _positionTrackTransform.position;
			_pivot.position = new Vector3(tPos.x, _heightTrackTransform.position.y, tPos.z);
			transform.localPosition = new Vector3(0, transform.localPosition.y, _forwardOffset);
		}

		protected virtual Quaternion ViewRotation()
		{
			return Quaternion.LookRotation(new Vector3(_rotationTrackTransform.forward.x, 0, _rotationTrackTransform.forward.z));
		}

		[ContextMenu("Recenter")]
		public virtual void Recenter()
		{
			_pivot.rotation = ViewRotation();
			_rotationTarget = _pivot.rotation;

			SetPosition();
			SetHeight();
		}

		public virtual void SetDisplayed(bool set)
		{
			if (set != _toggleObject.activeSelf)
			{
				set = enabled ? set : false;

				_isDisplayed = set;
				_toggleObject.gameObject.SetActive(set);

				if (set) Recenter();

				/// Controllers
				if (_usesHandsRaycast && !_hands.Hidden)
				{
					if (set)
						_hands.SetRay();
					else
						_hands.SetSpeak();
				}
			}
		}

		public virtual void SetDisplayEnabled(bool set)
		{
			enabled = set;
			if (!enabled) SetDisplayed(false);
		}
	}
}

