/************************************************************************************
Filename    :   ONSPVersion.cs
Content     :   Get version number of plug-in
Copyright   :   Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Licensed under the Oculus SDK Version 3.5 (the "License");
you may not use the Oculus SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.5/

Unless required by applicable law or agreed to in writing, the Oculus SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
************************************************************************************/

using System.Runtime.InteropServices;
using UnityEngine;

public class ONSPVersion : MonoBehaviour
{
    // Import functions
    public const string strONSPS = "AudioPluginOculusSpatializer";

    /// <summary>
    ///     Awake this instance.
    /// </summary>
    private void Awake()
    {
        var major = 0;
        var minor = 0;
        var patch = 0;

        ONSP_GetVersion(ref major, ref minor, ref patch);

        var version = string.Format
            ("ONSP Version: {0:F0}.{1:F0}.{2:F0}", major, minor, patch);

        Debug.Log(version);
    }

    /// <summary>
    ///     Start this instance.
    /// </summary>
    private void Start()
    {
    }

    /// <summary>
    ///     Update this instance.
    /// </summary>
    private void Update()
    {
    }

    [DllImport(strONSPS)]
    private static extern void ONSP_GetVersion(ref int Major, ref int Minor, ref int Patch);
}
