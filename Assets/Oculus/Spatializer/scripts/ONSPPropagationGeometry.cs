/************************************************************************************
Filename    :   ONSPPropagationGeometry.cs
Content     :   Geometry Functions
                Attach to a game object with meshes and material scripts to create geometry
                NOTE: ensure that Oculus Spatialization is enabled for AudioSource components
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

#define INCLUDE_TERRAIN_TREES

using System;
using System.Collections.Generic;
using System.IO;
using Oculus.Spatializer.Propagation;
using UnityEngine;

public class ONSPPropagationGeometry : MonoBehaviour
{
    public const string GEOMETRY_FILE_EXTENSION = "ovramesh";
    public static string GeometryAssetDirectory = "AudioGeometry";

    //-------
    // PUBLIC STATIC
    public static int OSPSuccess = 0;

    //
    // FUNCTIONS FOR UPLOADING MESHES VIA GAME OBJECT
    //

    private static readonly int terrainDecimation = 4;
    //-------
    // PUBLIC

    /// The path to the serialized mesh file that holds the preprocessed mesh geometry.
    public string filePathRelative = "";

    public bool fileEnabled;

    public bool includeChildMeshes = true;

    //-------
    // PRIVATE
    private IntPtr geometryHandle = IntPtr.Zero;
    public static string GeometryAssetPath => Application.streamingAssetsPath + "/" + GeometryAssetDirectory;
    public string filePath => GeometryAssetPath + "/" + filePathRelative;

    /// <summary>
    ///     If script is attached to a gameobject, it will try to create geometry
    /// </summary>
    private void Awake()
    {
        CreatePropagationGeometry();
    }

    /// <summary>
    ///     Update the world transform (TODO)
    /// </summary>
    private void Update()
    {
        if (geometryHandle == IntPtr.Zero)
            return;

        var m = transform.localToWorldMatrix;
        // Note: flip Z to convert from left-handed (+Z forward) to right-handed (+Z backward)
        float[] matrix =
        {
            m[0, 0], m[1, 0], -m[2, 0], m[3, 0],
            m[0, 1], m[1, 1], -m[2, 1], m[3, 1],
            m[0, 2], m[1, 2], -m[2, 2], m[3, 2],
            m[0, 3], m[1, 3], -m[2, 3], m[3, 3]
        };

        ONSPPropagation.Interface.AudioGeometrySetTransform(geometryHandle, matrix);
    }

    /// <summary>
    ///     Call when destroyed
    /// </summary>
    private void OnDestroy()
    {
        // DESTROY GEOMETRY
        if (geometryHandle != IntPtr.Zero &&
            ONSPPropagation.Interface.DestroyAudioGeometry(geometryHandle) != OSPSuccess)
            throw new Exception("Unable to destroy geometry");

        geometryHandle = IntPtr.Zero;
    }

    private static string GetPath(Transform current)
    {
        if (current.parent == null)
            return current.gameObject.scene.name + "/" + current.name;
        return GetPath(current.parent) + "-" + current.name;
    }

    /// <summary>
    ///     Call this function to create geometry handle
    /// </summary>
    private void CreatePropagationGeometry()
    {
        // Create Geometry
        if (ONSPPropagation.Interface.CreateAudioGeometry(out geometryHandle) != OSPSuccess)
            throw new Exception("Unable to create geometry handle");

        // Upload Mesh
        if (filePath != null && filePath.Length != 0 && fileEnabled && Application.isPlaying)
        {
            if (!ReadFile())
            {
                Debug.LogError("Failed to read file, attempting to regenerate audio geometry");

                // We should not try to upload data dynamically if data already exists
                UploadGeometry();
            }
        }
        else
        {
            UploadGeometry();
        }
    }

    private static void traverseMeshHierarchy(GameObject obj, ONSPPropagationMaterial[] currentMaterials,
        bool includeChildren,
        List<MeshMaterial> meshMaterials, List<TerrainMaterial> terrainMaterials, bool ignoreStatic,
        ref int ignoredMeshCount)
    {
        if (!obj.activeInHierarchy)
            return;

        var meshes = obj.GetComponents<MeshFilter>();
        var terrains = obj.GetComponents<Terrain>();
        var materials = obj.GetComponents<ONSPPropagationMaterial>();

        // Initialize the current material array to a new array if there are any new materials.
        if (materials != null && materials.Length > 0)
        {
            // Determine the length of the material array.
            var maxLength = materials.Length;
            if (currentMaterials != null)
                maxLength = Math.Max(maxLength, currentMaterials.Length);

            var newMaterials = new ONSPPropagationMaterial[maxLength];

            // Copy the previous materials into the new array.
            if (currentMaterials != null)
                for (var i = materials.Length; i < maxLength; i++)
                    newMaterials[i] = currentMaterials[i];
            currentMaterials = newMaterials;

            // Copy the current materials.
            for (var i = 0; i < materials.Length; i++)
                currentMaterials[i] = materials[i];
        }

        // Gather the meshes.
        foreach (var meshFilter in meshes)
        {
            var mesh = meshFilter.sharedMesh;
            if (mesh == null)
                continue;

            if (ignoreStatic && !mesh.isReadable)
            {
                Debug.LogWarning("Mesh: " + meshFilter.gameObject.name + " not readable, cannot be static.",
                    meshFilter.gameObject);
                ++ignoredMeshCount;
                continue;
            }

            var m = new MeshMaterial();
            m.meshFilter = meshFilter;
            m.materials = currentMaterials;
            meshMaterials.Add(m);
        }

        // Gather the terrains.
        foreach (var terrain in terrains)
        {
            var m = new TerrainMaterial();
            m.terrain = terrain;
            m.materials = currentMaterials;
            terrainMaterials.Add(m);
        }

        // Traverse to the child objects.
        if (includeChildren)
            foreach (Transform child in obj.transform)
                if (child.GetComponent<ONSPPropagationGeometry>() ==
                    null) // skip children which have their own component
                    traverseMeshHierarchy(child.gameObject, currentMaterials, includeChildren, meshMaterials,
                        terrainMaterials, ignoreStatic, ref ignoredMeshCount);
    }

    //
    // CALL THIS ON GAME OBJECT THAT HAS GEOMETRY ATTACHED TO IT
    //
    private int uploadMesh(IntPtr geometryHandle, GameObject meshObject, Matrix4x4 worldToLocal)
    {
        var unused = 0;
        return uploadMesh(geometryHandle, meshObject, worldToLocal, false, ref unused);
    }

    private int uploadMesh(IntPtr geometryHandle, GameObject meshObject, Matrix4x4 worldToLocal, bool ignoreStatic,
        ref int ignoredMeshCount)
    {
        // Get the child mesh objects.
        var meshes = new List<MeshMaterial>();
        var terrains = new List<TerrainMaterial>();
        traverseMeshHierarchy(meshObject, null, includeChildMeshes, meshes, terrains, ignoreStatic,
            ref ignoredMeshCount);

        //***********************************************************************
        // Count the number of vertices and indices.

        var totalVertexCount = 0;
        uint totalIndexCount = 0;
        var totalFaceCount = 0;
        var totalMaterialCount = 0;

        foreach (var m in meshes)
            updateCountsForMesh(ref totalVertexCount, ref totalIndexCount, ref totalFaceCount, ref totalMaterialCount,
                m.meshFilter.sharedMesh);

        // TODO: expose tree material
        var treeMaterials = new ONSPPropagationMaterial[1];

        for (var i = 0; i < terrains.Count; ++i)
        {
            var t = terrains[i];
            var terrain = t.terrain.terrainData;

#if UNITY_2019_3_OR_NEWER
            var w = terrain.heightmapResolution;
            var h = terrain.heightmapResolution;
#else
            int w = terrain.heightmapWidth;
            int h = terrain.heightmapHeight;
#endif
            var wRes = (w - 1) / terrainDecimation + 1;
            var hRes = (h - 1) / terrainDecimation + 1;
            var vertexCount = wRes * hRes;
            var indexCount = (wRes - 1) * (hRes - 1) * 6;

            totalMaterialCount++;
            totalVertexCount += vertexCount;
            totalIndexCount += (uint)indexCount;
            totalFaceCount += indexCount / 3;

#if INCLUDE_TERRAIN_TREES
            var treePrototypes = terrain.treePrototypes;

            if (treePrototypes.Length != 0)
            {
                if (treeMaterials[0] == null)
                {
                    // Create the tree material
                    treeMaterials[0] = gameObject.AddComponent<ONSPPropagationMaterial>();
#if true
                    treeMaterials[0].SetPreset(ONSPPropagationMaterial.Preset.Foliage);
#else
                    // Custom material that is highly transmissive
                    treeMaterials[0].absorption.points = new List<ONSPPropagationMaterial.Point>{
                                    new ONSPPropagationMaterial.Point(125f,  .03f),
                        new ONSPPropagationMaterial.Point(250f,  .06f),
                        new ONSPPropagationMaterial.Point(500f,  .11f),
                        new ONSPPropagationMaterial.Point(1000f, .17f),
                        new ONSPPropagationMaterial.Point(2000f, .27f),
                        new ONSPPropagationMaterial.Point(4000f, .31f) };

                    treeMaterials[0].scattering.points = new List<ONSPPropagationMaterial.Point>{
                                    new ONSPPropagationMaterial.Point(125f,  .20f),
                        new ONSPPropagationMaterial.Point(250f,  .3f),
                        new ONSPPropagationMaterial.Point(500f,  .4f),
                        new ONSPPropagationMaterial.Point(1000f, .5f),
                        new ONSPPropagationMaterial.Point(2000f, .7f),
                        new ONSPPropagationMaterial.Point(4000f, .8f) };

                    treeMaterials[0].transmission.points = new List<ONSPPropagationMaterial.Point>(){
                                    new ONSPPropagationMaterial.Point(125f,  .95f),
                        new ONSPPropagationMaterial.Point(250f,  .92f),
                        new ONSPPropagationMaterial.Point(500f,  .87f),
                        new ONSPPropagationMaterial.Point(1000f, .81f),
                        new ONSPPropagationMaterial.Point(2000f, .71f),
                        new ONSPPropagationMaterial.Point(4000f, .67f) };
#endif
                }

                t.treePrototypeMeshes = new Mesh[treePrototypes.Length];

                // assume the sharedMesh with the lowest vertex is the lowest LOD
                for (var j = 0; j < treePrototypes.Length; ++j)
                {
                    var prefab = treePrototypes[j].prefab;
                    var meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
                    var minVertexCount = int.MaxValue;
                    var index = -1;
                    for (var k = 0; k < meshFilters.Length; ++k)
                    {
                        var count = meshFilters[k].sharedMesh.vertexCount;
                        if (count < minVertexCount)
                        {
                            minVertexCount = count;
                            index = k;
                        }
                    }

                    t.treePrototypeMeshes[j] = meshFilters[index].sharedMesh;
                }

                var trees = terrain.treeInstances;
                foreach (var tree in trees)
                    updateCountsForMesh(ref totalVertexCount, ref totalIndexCount, ref totalFaceCount,
                        ref totalMaterialCount, t.treePrototypeMeshes[tree.prototypeIndex]);

                terrains[i] = t;
            }
#endif
        }

        //***********************************************************************
        // Copy the mesh data.

        var tempVertices = new List<Vector3>();
        var tempIndices = new List<int>();

        var groups = new MeshGroup[totalMaterialCount];
        var vertices = new float[totalVertexCount * 3];
        var indices = new int[totalIndexCount];

        var vertexOffset = 0;
        var indexOffset = 0;
        var groupOffset = 0;

        foreach (var m in meshes)
        {
            var meshFilter = m.meshFilter;

            // Compute the combined transform to go from mesh-local to geometry-local space.
            var matrix = worldToLocal * meshFilter.gameObject.transform.localToWorldMatrix;

            uploadMeshFilter(tempVertices, tempIndices, groups, vertices, indices, ref vertexOffset, ref indexOffset,
                ref groupOffset, meshFilter.sharedMesh, m.materials, matrix);
        }

        foreach (var t in terrains)
        {
            var terrain = t.terrain.terrainData;

            // Compute the combined transform to go from mesh-local to geometry-local space.
            var matrix = worldToLocal * t.terrain.gameObject.transform.localToWorldMatrix;

#if UNITY_2019_3_OR_NEWER
            var w = terrain.heightmapResolution;
            var h = terrain.heightmapResolution;
#else
            int w = terrain.heightmapWidth;
            int h = terrain.heightmapHeight;
#endif
            var tData = terrain.GetHeights(0, 0, w, h);

            var meshScale = terrain.size;
            meshScale = new Vector3(meshScale.x / (w - 1) * terrainDecimation, meshScale.y,
                meshScale.z / (h - 1) * terrainDecimation);
            var wRes = (w - 1) / terrainDecimation + 1;
            var hRes = (h - 1) / terrainDecimation + 1;
            var vertexCount = wRes * hRes;
            var triangleCount = (wRes - 1) * (hRes - 1) * 2;

            // Initialize the group.
            groups[groupOffset].faceType = FaceType.TRIANGLES;
            groups[groupOffset].faceCount = (UIntPtr)triangleCount;
            groups[groupOffset].indexOffset = (UIntPtr)indexOffset;

            if (t.materials != null && 0 < t.materials.Length)
            {
                t.materials[0].StartInternal();
                groups[groupOffset].material = t.materials[0].materialHandle;
            }
            else
            {
                groups[groupOffset].material = IntPtr.Zero;
            }

            // Build vertices and UVs
            for (var y = 0; y < hRes; y++)
                for (var x = 0; x < wRes; x++)
                {
                    var offset = (vertexOffset + y * wRes + x) * 3;
                    var v = matrix.MultiplyPoint3x4(Vector3.Scale(meshScale,
                        new Vector3(y, tData[x * terrainDecimation, y * terrainDecimation], x)));
                    vertices[offset + 0] = v.x;
                    vertices[offset + 1] = v.y;
                    vertices[offset + 2] = v.z;
                }

            // Build triangle indices: 3 indices into vertex array for each triangle
            for (var y = 0; y < hRes - 1; y++)
                for (var x = 0; x < wRes - 1; x++)
                {
                    // For each grid cell output two triangles
                    indices[indexOffset + 0] = vertexOffset + y * wRes + x;
                    indices[indexOffset + 1] = vertexOffset + (y + 1) * wRes + x;
                    indices[indexOffset + 2] = vertexOffset + y * wRes + x + 1;

                    indices[indexOffset + 3] = vertexOffset + (y + 1) * wRes + x;
                    indices[indexOffset + 4] = vertexOffset + (y + 1) * wRes + x + 1;
                    indices[indexOffset + 5] = vertexOffset + y * wRes + x + 1;
                    indexOffset += 6;
                }

            vertexOffset += vertexCount;
            groupOffset++;

#if INCLUDE_TERRAIN_TREES
            var trees = terrain.treeInstances;
            foreach (var tree in trees)
            {
                var pos = Vector3.Scale(tree.position, terrain.size);
                var treeLocalToWorldMatrix = t.terrain.gameObject.transform.localToWorldMatrix;
                treeLocalToWorldMatrix.SetColumn(3,
                    treeLocalToWorldMatrix.GetColumn(3) + new Vector4(pos.x, pos.y, pos.z, 0.0f));
                // TODO: tree rotation
                var treeMatrix = worldToLocal * treeLocalToWorldMatrix;
                uploadMeshFilter(tempVertices, tempIndices, groups, vertices, indices, ref vertexOffset,
                    ref indexOffset, ref groupOffset, t.treePrototypeMeshes[tree.prototypeIndex], treeMaterials,
                    treeMatrix);
            }
#endif
        }

        // Upload mesh data
        return ONSPPropagation.Interface.AudioGeometryUploadMeshArrays(geometryHandle,
            vertices, totalVertexCount,
            indices, indices.Length,
            groups, groups.Length);
    }

    private static void uploadMeshFilter(List<Vector3> tempVertices, List<int> tempIndices, MeshGroup[] groups,
        float[] vertices, int[] indices,
        ref int vertexOffset, ref int indexOffset, ref int groupOffset, Mesh mesh, ONSPPropagationMaterial[] materials,
        Matrix4x4 matrix)
    {
        // Get the mesh vertices.
        tempVertices.Clear();
        mesh.GetVertices(tempVertices);

        // Copy the Vector3 vertices into a packed array of floats for the API.
        var meshVertexCount = tempVertices.Count;
        for (var i = 0; i < meshVertexCount; i++)
        {
            // Transform into the parent space.
            var v = matrix.MultiplyPoint3x4(tempVertices[i]);
            var offset = (vertexOffset + i) * 3;
            vertices[offset + 0] = v.x;
            vertices[offset + 1] = v.y;
            vertices[offset + 2] = v.z;
        }

        // Copy the data for each submesh.
        for (var i = 0; i < mesh.subMeshCount; i++)
        {
            var topology = mesh.GetTopology(i);

            if (topology == MeshTopology.Triangles || topology == MeshTopology.Quads)
            {
                // Get the submesh indices.
                tempIndices.Clear();
                mesh.GetIndices(tempIndices, i);
                var subMeshIndexCount = tempIndices.Count;

                // Copy and adjust the indices.
                for (var j = 0; j < subMeshIndexCount; j++)
                    indices[indexOffset + j] = tempIndices[j] + vertexOffset;

                // Initialize the group.
                if (topology == MeshTopology.Triangles)
                {
                    groups[groupOffset + i].faceType = FaceType.TRIANGLES;
                    groups[groupOffset + i].faceCount = (UIntPtr)(subMeshIndexCount / 3);
                }
                else if (topology == MeshTopology.Quads)
                {
                    groups[groupOffset + i].faceType = FaceType.QUADS;
                    groups[groupOffset + i].faceCount = (UIntPtr)(subMeshIndexCount / 4);
                }

                groups[groupOffset + i].indexOffset = (UIntPtr)indexOffset;

                if (materials != null && materials.Length != 0)
                {
                    var matIndex = i;
                    if (matIndex >= materials.Length)
                        matIndex = materials.Length - 1;
                    materials[matIndex].StartInternal();
                    groups[groupOffset + i].material = materials[matIndex].materialHandle;
                }
                else
                {
                    groups[groupOffset + i].material = IntPtr.Zero;
                }

                indexOffset += subMeshIndexCount;
            }
        }

        vertexOffset += meshVertexCount;
        groupOffset += mesh.subMeshCount;
    }

    private static void updateCountsForMesh(ref int totalVertexCount, ref uint totalIndexCount, ref int totalFaceCount,
        ref int totalMaterialCount, Mesh mesh)
    {
        totalMaterialCount += mesh.subMeshCount;
        totalVertexCount += mesh.vertexCount;

        for (var i = 0; i < mesh.subMeshCount; i++)
        {
            var topology = mesh.GetTopology(i);
            if (topology == MeshTopology.Triangles || topology == MeshTopology.Quads)
            {
                var meshIndexCount = mesh.GetIndexCount(i);
                totalIndexCount += meshIndexCount;

                if (topology == MeshTopology.Triangles)
                    totalFaceCount += (int)meshIndexCount / 3;
                else if (topology == MeshTopology.Quads)
                    totalFaceCount += (int)meshIndexCount / 4;
            }
        }
    }

    //***********************************************************************
    // UploadGeometry

    public void UploadGeometry()
    {
        var ignoredMeshCount = 0;
        if (uploadMesh(geometryHandle, gameObject, gameObject.transform.worldToLocalMatrix, true,
                ref ignoredMeshCount) != OSPSuccess)
            throw new Exception("Unable to upload audio mesh geometry");

        if (ignoredMeshCount != 0)
            Debug.LogError(
                "Failed to upload meshes, " + ignoredMeshCount +
                " static meshes ignored. Turn on \"File Enabled\" to process static meshes offline", gameObject);
    }

#if UNITY_EDITOR
    //***********************************************************************
    // WriteFile - Write the serialized mesh file.

    public bool WriteFile()
    {
        if (filePathRelative == "")
        {
            filePathRelative = GetPath(transform);
            var modifier = "";
            var counter = 0;
            while (File.Exists(filePath + modifier))
            {
                modifier = "-" + counter;
                ++counter;

                if (counter > 10000)
                    // sanity check to prevent hang
                    throw new Exception("Unable to find sutiable file name");
            }

            filePathRelative = filePathRelative + modifier;
            Debug.Log("No file path specified, autogenerated: " + filePathRelative);
        }

        // Create the directory
        var directoryName = filePathRelative.Substring(0, filePathRelative.LastIndexOf('/'));
        Directory.CreateDirectory(GeometryAssetPath + "/" + directoryName);

        // Create a temporary geometry.
        var tempGeometryHandle = IntPtr.Zero;
        if (ONSPPropagation.Interface.CreateAudioGeometry(out tempGeometryHandle) != OSPSuccess)
            throw new Exception("Failed to create temp geometry handle");

        // Upload the mesh geometry.
        if (uploadMesh(tempGeometryHandle, gameObject, gameObject.transform.worldToLocalMatrix) != OSPSuccess)
        {
            Debug.LogError("Error uploading mesh " + gameObject.name);
            return false;
        }

        // Write the mesh to a file.
        if (ONSPPropagation.Interface.AudioGeometryWriteMeshFile(tempGeometryHandle, filePath) != OSPSuccess)
        {
            Debug.LogError("Error writing mesh file " + filePath);
            return false;
        }

        // Destroy the geometry.
        if (ONSPPropagation.Interface.DestroyAudioGeometry(tempGeometryHandle) != OSPSuccess)
            throw new Exception("Failed to destroy temp geometry handle");

        return true;
    }
#endif

    //***********************************************************************
    // ReadFile - Read the serialized mesh file.

    public bool ReadFile()
    {
        if (filePath == null || filePath.Length == 0)
        {
            Debug.LogError("Invalid mesh file path");
            return false;
        }

        if (ONSPPropagation.Interface.AudioGeometryReadMeshFile(geometryHandle, filePath) != OSPSuccess)
        {
            Debug.LogError("Error reading mesh file " + filePath);
            return false;
        }

        return true;
    }

    public bool WriteToObj()
    {
        // Create a temporary geometry.
        var tempGeometryHandle = IntPtr.Zero;
        if (ONSPPropagation.Interface.CreateAudioGeometry(out tempGeometryHandle) != OSPSuccess)
            throw new Exception("Failed to create temp geometry handle");

        // Upload the mesh geometry.
        if (uploadMesh(tempGeometryHandle, gameObject, gameObject.transform.worldToLocalMatrix) != OSPSuccess)
        {
            Debug.LogError("Error uploading mesh " + gameObject.name);
            return false;
        }

        // Write the mesh to a .obj file.
        if (ONSPPropagation.Interface.AudioGeometryWriteMeshFileObj(tempGeometryHandle, filePath + ".obj") !=
            OSPSuccess)
        {
            Debug.LogError("Error writing .obj file " + filePath + ".obj");
            return false;
        }

        // Destroy the geometry.
        if (ONSPPropagation.Interface.DestroyAudioGeometry(tempGeometryHandle) != OSPSuccess)
            throw new Exception("Failed to destroy temp geometry handle");

        return true;
    }

    private struct MeshMaterial
    {
        public MeshFilter meshFilter;
        public ONSPPropagationMaterial[] materials;
    }

    private struct TerrainMaterial
    {
        public Terrain terrain;
        public ONSPPropagationMaterial[] materials;
        public Mesh[] treePrototypeMeshes;
    }
}
