/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Oculus.Voice.Core.Bindings.Interfaces;
using UnityEngine;

namespace Oculus.Voice.Core.Bindings.Android
{
    public class AndroidServiceConnection : IConnection
    {
        private readonly string serviceFragmentClass;
        private readonly string serviceGetter;

        /// <summary>
        ///     Creates a connection manager of the given type
        /// </summary>
        /// <param name="serviceFragmentClassName">
        ///     The fully qualified class name of the service fragment that will manage this
        ///     connection
        /// </param>
        /// <param name="serviceGetterMethodName">The name of the method that will return an instance of the service</param>
        /// TODO: We should make the getBlahService simply getService() within each fragment implementation.
        public AndroidServiceConnection(string serviceFragmentClassName, string serviceGetterMethodName)
        {
            serviceFragmentClass = serviceFragmentClassName;
            serviceGetter = serviceGetterMethodName;
        }

        public AndroidJavaObject AssistantServiceConnection { get; private set; }

        public bool IsConnected => null != AssistantServiceConnection;

        public void Connect(string version)
        {
            if (null == AssistantServiceConnection)
            {
                AndroidJNIHelper.debug = true;

                var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

                using (var assistantBackgroundFragment = new AndroidJavaClass(serviceFragmentClass))
                {
                    AssistantServiceConnection =
                        assistantBackgroundFragment.CallStatic<AndroidJavaObject>("createAndAttach", activity, version);
                }
            }
        }

        public void Disconnect()
        {
            AssistantServiceConnection.Call("detach");
        }

        public AndroidJavaObject GetService()
        {
            return AssistantServiceConnection.Call<AndroidJavaObject>(serviceGetter);
        }
    }
}
