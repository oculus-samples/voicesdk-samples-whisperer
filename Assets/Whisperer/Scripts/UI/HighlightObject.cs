/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class HighlightObject : MonoBehaviour
{
    private static readonly HashSet<Mesh> registeredMeshes = new();

    [SerializeField] private Color highlightColor = Color.white;

    [SerializeField] [Range(0f, 10f)] private float outlineWidth = 2f;

    [Header("Optional")]
    [SerializeField]
    [Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
             + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
    private bool precomputeOutline;

    [SerializeField] [HideInInspector] private List<Mesh> bakeKeys = new();

    [SerializeField] [HideInInspector] private List<ListVector3> bakeValues = new();

    private Material highlightMaterial;

    private bool needsUpdate,
        isEnabled;

    private Material outlineFillMaterial;
    private Material outlineMaskMaterial;

    private Renderer[] renderers;

    public Color HighlightColor
    {
        get => highlightColor;
        set
        {
            highlightColor = value;
            needsUpdate = true;
        }
    }

    public float OutlineWidth
    {
        get => outlineWidth;
        set
        {
            outlineWidth = value;
            needsUpdate = true;
        }
    }

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        outlineMaskMaterial = Instantiate(Resources.Load<Material>("OutlineMask"));
        outlineFillMaterial = Instantiate(Resources.Load<Material>("OutlineFill"));
        highlightMaterial = Instantiate(Resources.Load<Material>("Highlight"));

        outlineMaskMaterial.name = "OutlineMask (Instance)";
        outlineFillMaterial.name = "OutlineFill (Instance)";

        LoadSmoothNormals();

        needsUpdate = true;
        enabled = true;
    }

    private void Update()
    {
        if (needsUpdate)
        {
            needsUpdate = false;

            UpdateMaterialProperties();
        }
    }

    private void OnDestroy()
    {
        Destroy(outlineMaskMaterial);
        Destroy(outlineFillMaterial);
    }

    private void OnValidate()
    {
        needsUpdate = true;
        if ((!precomputeOutline && bakeKeys.Count != 0) || bakeKeys.Count != bakeValues.Count)
        {
            bakeKeys.Clear();
            bakeValues.Clear();
        }

        if (precomputeOutline && bakeKeys.Count == 0) Bake();
    }

    public void EnableHighlight(bool enabled)
    {
        if (enabled && !isEnabled)
        {
            isEnabled = true;
            foreach (var renderer in renderers)
            {
                var materials = renderer.sharedMaterials.ToList();

                materials.Add(outlineMaskMaterial);
                materials.Add(outlineFillMaterial);

                materials.Add(highlightMaterial);

                renderer.materials = materials.ToArray();
            }
        }
        else if (!enabled && isEnabled)
        {
            isEnabled = false;
            foreach (var renderer in renderers)
            {
                // Remove outline shaders
                var materials = renderer.sharedMaterials.ToList();

                materials.Remove(outlineMaskMaterial);
                materials.Remove(outlineFillMaterial);

                materials.Remove(highlightMaterial);

                renderer.materials = materials.ToArray();
            }
        }
    }

    private void Bake()
    {
        var bakedMeshes = new HashSet<Mesh>();

        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (!bakedMeshes.Add(meshFilter.sharedMesh)) continue;
            var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

            bakeKeys.Add(meshFilter.sharedMesh);
            bakeValues.Add(new ListVector3 { data = smoothNormals });
        }
    }

    private void LoadSmoothNormals()
    {
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (!registeredMeshes.Add(meshFilter.sharedMesh)) continue;
            var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
            var smoothNormals = index >= 0 ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);
            meshFilter.sharedMesh.SetUVs(3, smoothNormals);
            var renderer = meshFilter.GetComponent<Renderer>();

            if (renderer != null) CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
        }

        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!registeredMeshes.Add(skinnedMeshRenderer.sharedMesh)) continue;
            skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];

            CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
        }
    }

    private List<Vector3> SmoothNormals(Mesh mesh)
    {
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index))
            .GroupBy(pair => pair.Key);

        var smoothNormals = new List<Vector3>(mesh.normals);

        foreach (var group in groups)
        {
            if (group.Count() == 1) continue;

            var smoothNormal = Vector3.zero;

            foreach (var pair in group) smoothNormal += smoothNormals[pair.Value];

            smoothNormal.Normalize();

            foreach (var pair in group) smoothNormals[pair.Value] = smoothNormal;
        }

        return smoothNormals;
    }

    private void CombineSubmeshes(Mesh mesh, Material[] materials)
    {
        if (mesh.subMeshCount == 1) return;

        if (mesh.subMeshCount > materials.Length) return;

        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
    }

    private void UpdateMaterialProperties()
    {
        highlightMaterial.color = highlightColor;

        outlineFillMaterial.SetColor("_OutlineColor", highlightColor);

        outlineMaskMaterial.SetFloat("_ZTest", (float)CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
    }

    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }
}
