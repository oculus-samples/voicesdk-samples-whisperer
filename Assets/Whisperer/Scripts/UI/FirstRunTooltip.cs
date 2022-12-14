/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;

namespace Whisperer
{
    public class FirstRunTooltip : ListenableUI
    {
        [Header("First Run Tooltip References")] [SerializeField]
        protected RectTransform _tooltipRectTransform;

        [SerializeField] protected GameObject _tooltipText;
        [SerializeField] protected CanvasGroup _firstRunTooltipCanvasGroup;

        protected Progress _firstRunTooltipFadeProgress;

        private void Awake()
        {
            _tooltipText.SetActive(false);
            _firstRunTooltipFadeProgress = new Progress(SetOpacity);
        }

        protected override void Reset()
        {
            base.Reset();
            _tooltipRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 256);
        }

        public void Show()
        {
            _firstRunTooltipFadeProgress.Play(_fadeDuration);
        }

        public void Expand()
        {
            _tooltipRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1460);
            _tooltipText.SetActive(true);
        }

        public void Hide()
        {
            _firstRunTooltipFadeProgress.PlayReverse(_fadeDuration);
        }

        protected override void OnMinimumWakeThresholdHit()
        {
            if (!_listenable.IsSelected)
                return;

            Hide();
            base.OnMinimumWakeThresholdHit();
        }

        protected override void SetOpacity(float opacity)
        {
            base.SetOpacity(opacity);
            _firstRunTooltipCanvasGroup.alpha = opacity;
        }
    }
}
