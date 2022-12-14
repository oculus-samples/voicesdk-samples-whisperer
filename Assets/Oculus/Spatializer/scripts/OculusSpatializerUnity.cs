/************************************************************************************
Filename    :   OculusSpatializerUnity.cs
Content     :   Interface into real-time geometry reflection engine for native Unity
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

using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

public class OculusSpatializerUnity : MonoBehaviour
{
    // * * * * * * * * * * * * *
    // Import functions
    public delegate void AudioRaycastCallback(Vector3 origin, Vector3 direction,
        out Vector3 point, out Vector3 normal,
        IntPtr data);

    private const int HIT_COUNT = 2048;

    private const string strOSP = "AudioPluginOculusSpatializer";


    private static LayerMask gLayerMask = -1;
    public LayerMask layerMask = -1;
    public bool visualizeRoom = true;

    public int raysPerSecond = 256;
    public float roomInterpSpeed = 0.9f;
    public float maxWallDistance = 50.0f;
    public int rayCacheSize = 512;

    public bool dynamicReflectionsEnabled = true;
    private readonly float[] coefs = new float[6];

    private readonly float[] dims = new float[3] { 1.0f, 1.0f, 1.0f };
    private readonly Vector3[] normals = new Vector3[HIT_COUNT];
    private readonly float particleOffset = 0.1f;
    private readonly ParticleSystem.Particle[] particles = new ParticleSystem.Particle[HIT_COUNT];
    private readonly float particleSize = 0.2f;

    private readonly Vector3[] points = new Vector3[HIT_COUNT];

    private GameObject room;
    private bool roomVisualizationInitialized;

    private ParticleSystem sys;
    private readonly Renderer[] wallRenderer = new Renderer[6];

    private void Start()
    {
        OSP_Unity_AssignRaycastCallback(AudioRaycast, IntPtr.Zero);
    }

    private void Update()
    {
        if (dynamicReflectionsEnabled)
            OSP_Unity_AssignRaycastCallback(AudioRaycast, IntPtr.Zero);
        else
            OSP_Unity_AssignRaycastCallback(IntPtr.Zero, IntPtr.Zero);

        OSP_Unity_SetDynamicRoomRaysPerSecond(raysPerSecond);
        OSP_Unity_SetDynamicRoomInterpSpeed(roomInterpSpeed);
        OSP_Unity_SetDynamicRoomMaxWallDistance(maxWallDistance);
        OSP_Unity_SetDynamicRoomRaysRayCacheSize(rayCacheSize);

        gLayerMask = layerMask;
        OSP_Unity_UpdateRoomModel(1.0f);

        if (visualizeRoom)
        {
            if (!roomVisualizationInitialized)
            {
                inititalizeRoomVisualization();
                roomVisualizationInitialized = true;
            }

            Vector3 pos;
            OSP_Unity_GetRoomDimensions(dims, coefs, out pos);

            pos.z *= -1; // swap to left-handed

            var size = new Vector3(dims[0], dims[1], dims[2]);

            var magSqrd = size.sqrMagnitude;

            if (!float.IsNaN(magSqrd) && 0.0f < magSqrd && magSqrd < 1000000.0f) transform.localScale = size * 0.999f;

            transform.position = pos;

            OSP_Unity_GetRaycastHits(points, normals, HIT_COUNT);

            for (var i = 0; i < HIT_COUNT; ++i)
            {
                if (points[i] == Vector3.zero)
                    points[i].y = -10000.0f; // hide it

                // swap to left-handed
                points[i].z *= -1;
                normals[i].z *= -1;

                particles[i].position = points[i] + normals[i] * particleOffset;

                if (normals[i] != Vector3.zero)
                    particles[i].rotation3D = Quaternion.LookRotation(normals[i]).eulerAngles;

                particles[i].startSize = particleSize;
                particles[i].startColor = new Color(208 / 255f, 38 / 255f, 174 / 255f, 1.0f);
            }

            for (var wall = 0; wall < 6; ++wall)
            {
                var color = Color.Lerp(Color.red, Color.green, coefs[wall]);
                wallRenderer[wall].material.SetColor("_TintColor", color);
            }

            sys.SetParticles(particles, particles.Length);
        }
    }

    private void OnDestroy()
    {
        OSP_Unity_AssignRaycastCallback(IntPtr.Zero, IntPtr.Zero);
    }

    private static Vector3 swapHandedness(Vector3 vec)
    {
        return new Vector3(vec.x, vec.y, -vec.z);
    }

    [MonoPInvokeCallback(typeof(AudioRaycastCallback))]
    private static void AudioRaycast(Vector3 origin, Vector3 direction, out Vector3 point, out Vector3 normal,
        IntPtr data)
    {
        point = Vector3.zero;
        normal = Vector3.zero;

        RaycastHit hitInfo;
        if (Physics.Raycast(swapHandedness(origin), swapHandedness(direction), out hitInfo, 1000.0f, gLayerMask.value))
        {
            point = swapHandedness(hitInfo.point);
            normal = swapHandedness(hitInfo.normal);
        }
    }

    private void inititalizeRoomVisualization()
    {
        Debug.Log("Oculus Audio dynamic room estimation visualization enabled");
        transform.position = Vector3.zero; // move to the origin otherwise things are displaced

        // Create a particle system to visualize the ray cast hits
        var decalManager = new GameObject("DecalManager");
        decalManager.transform.parent = transform;
        sys = decalManager.AddComponent<ParticleSystem>();
        {
            var main = sys.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = false;
            main.playOnAwake = false;
            var emission = sys.emission;
            emission.enabled = false;
            var shape = sys.shape;
            shape.enabled = false;
            var renderer = sys.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.material.shader = Shader.Find("Particles/Additive");

            Texture2D decalTex;
            {
                const int SIZE = 64;
                const int RING_COUNT = 2;

                decalTex = new Texture2D(SIZE, SIZE);
                const int HALF_SIZE = SIZE / 2;
                for (var i = 0; i < SIZE / 2; ++i)
                for (var j = 0; j < SIZE / 2; ++j)
                {
                    // distance from center
                    float deltaX = HALF_SIZE - i;
                    float deltaY = HALF_SIZE - j;
                    var dist = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
                    var t = RING_COUNT * dist / HALF_SIZE;

                    var alpha = dist < HALF_SIZE ? Mathf.Clamp01(Mathf.Sin(Mathf.PI * 2.0f * t)) : 0.0f;
                    var col = new Color(1.0f, 1.0f, 1.0f, alpha);

                    // Two way symmetry
                    decalTex.SetPixel(i, j, col);
                    decalTex.SetPixel(SIZE - i, j, col);
                    decalTex.SetPixel(i, SIZE - j, col);
                    decalTex.SetPixel(SIZE - i, SIZE - j, col);
                }

                decalTex.Apply();
            }

            renderer.material.mainTexture = decalTex;
            // Make a quad
            var m = new Mesh();
            m.name = "ParticleQuad";
            const float size = 0.5f;
            m.vertices = new[]
            {
                new(-size, -size, 0.0f),
                new Vector3(size, -size, 0.0f),
                new Vector3(size, size, 0.0f),
                new Vector3(-size, size, 0.0f)
            };
            m.uv = new[]
            {
                new(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0)
            };
            m.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            m.RecalculateNormals();
            renderer.mesh = m;
        }
        sys.Emit(HIT_COUNT);

        // Construct the visual representation of the room
        room = new GameObject("RoomVisualizer");
        room.transform.parent = transform;
        room.transform.localPosition = Vector3.zero;

        Texture2D wallTex;
        {
            const int SIZE = 32;
            wallTex = new Texture2D(SIZE, SIZE);

            var transparent = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            for (var i = 0; i < SIZE; ++i)
            for (var j = 0; j < SIZE; ++j)
                wallTex.SetPixel(i, j, transparent);

            for (var i = 0; i < SIZE; ++i)
            {
                var color1 = Color.white * 0.125f;

                wallTex.SetPixel(SIZE / 4, i, color1);
                wallTex.SetPixel(i, SIZE / 4, color1);

                wallTex.SetPixel(3 * SIZE / 4, i, color1);
                wallTex.SetPixel(i, 3 * SIZE / 4, color1);

                color1 *= 2.0f;

                wallTex.SetPixel(SIZE / 2, i, color1);
                wallTex.SetPixel(i, SIZE / 2, color1);

                color1 *= 2.0f;

                wallTex.SetPixel(0, i, color1);
                wallTex.SetPixel(i, 0, color1);
            }

            wallTex.Apply();
        }

        for (var wall = 0; wall < 6; ++wall)
        {
            var m = new Mesh();
            m.name = "Plane" + wall;
            const float size = 0.5f;
            var verts = new Vector3[4];

            var axis = wall / 2;
            var sign = wall % 2 == 0 ? 1 : -1;

            for (var i = 0; i < 4; ++i)
            {
                verts[i][axis] = sign * size;
                verts[i][(axis + 1) % 3] = size * (i == 1 || i == 2 ? 1 : -1);
                verts[i][(axis + 2) % 3] = size * (i == 2 || i == 3 ? 1 : -1);
            }

            m.vertices = verts;

            m.uv = new[]
            {
                new(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0)
            };

            m.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            m.RecalculateNormals();
            var go = new GameObject("Wall_" + wall);
            go.AddComponent<MeshFilter>().mesh = m;
            var renderer = go.AddComponent<MeshRenderer>();
            wallRenderer[wall] = renderer;
            renderer.material.shader = Shader.Find("Particles/Additive");
            renderer.material.mainTexture = wallTex;
            renderer.material.mainTextureScale = new Vector2(8, 8);
            go.transform.parent = room.transform;
            room.transform.localPosition = Vector3.zero;
        }
    }

    [DllImport(strOSP)]
    private static extern int OSP_Unity_AssignRaycastCallback(AudioRaycastCallback callback, IntPtr data);

    [DllImport(strOSP)]
    private static extern int OSP_Unity_AssignRaycastCallback(IntPtr callback, IntPtr data);

    [DllImport(strOSP)]
    private static extern int OSP_Unity_SetDynamicRoomRaysPerSecond(int RaysPerSecond);

    [DllImport(strOSP)]
    private static extern int OSP_Unity_SetDynamicRoomInterpSpeed(float InterpSpeed);

    [DllImport(strOSP)]
    private static extern int OSP_Unity_SetDynamicRoomMaxWallDistance(float MaxWallDistance);

    [DllImport(strOSP)]
    private static extern int OSP_Unity_SetDynamicRoomRaysRayCacheSize(int RayCacheSize);

    [DllImport(strOSP)]
    private static extern int OSP_Unity_UpdateRoomModel(float wetLevel); // call from main thread!!

    [DllImport(strOSP)]
    private static extern int OSP_Unity_GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs,
        out Vector3 position);

    [DllImport(strOSP)]
    private static extern int OSP_Unity_GetRaycastHits(Vector3[] points, Vector3[] normals, int length);
}
