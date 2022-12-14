/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using Facebook.WitAi.Data.Configuration;
using UnityEngine;

namespace Facebook.WitAi.Windows
{
    public class WitWelcomeWizard : WitScriptableWizard
    {
        protected string serverToken;
        public Action<WitConfiguration> successAction;

        protected override Texture2D HeaderIcon => WitTexts.HeaderIcon;
        protected override GUIContent Title => WitTexts.SetupTitleContent;
        protected override string ButtonLabel => WitTexts.Texts.SetupSubmitButtonLabel;
        protected override string ContentSubheaderLabel => WitTexts.Texts.SetupSubheaderLabel;

        protected override void OnEnable()
        {
            base.OnEnable();
            serverToken = string.Empty;
            WitAuthUtility.ServerToken = serverToken;
        }

        protected override void OnWizardCreate()
        {
            ValidateAndClose();
        }

        protected override bool DrawWizardGUI()
        {
            // Layout base
            base.DrawWizardGUI();
            // True if valid server token
            return WitConfigurationUtility.IsServerTokenValid(serverToken);
        }

        protected override void LayoutFields()
        {
            var serverTokenLabelText = WitTexts.Texts.SetupServerTokenLabel;
            serverTokenLabelText = serverTokenLabelText.Replace(WitStyles.WitLinkKey, WitStyles.WitLinkColor);
            if (GUILayout.Button(serverTokenLabelText, WitStyles.Label))
                Application.OpenURL(WitTexts.GetAppURL("", WitTexts.WitAppEndpointType.Settings));
            var updated = false;
            WitEditorUI.LayoutPasswordField(null, ref serverToken, ref updated);
        }

        protected virtual void ValidateAndClose()
        {
            WitAuthUtility.ServerToken = serverToken;
            if (WitAuthUtility.IsServerTokenValid())
            {
                // Create configuration
                var index = CreateConfiguration(serverToken);
                if (index != -1)
                {
                    // Complete
                    Close();
                    var c = WitConfigurationUtility.WitConfigs[index];
                    if (successAction == null)
                        WitWindowUtility.OpenConfigurationWindow(c);
                    else
                        successAction(c);
                }
            }
            else
            {
                throw new ArgumentException(WitTexts.Texts.SetupSubmitFailLabel);
            }
        }

        protected virtual int CreateConfiguration(string newToken)
        {
            return WitConfigurationUtility.CreateConfiguration(newToken);
        }
    }
}
