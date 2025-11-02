using UnityEngine;
using System.Collections.Generic;

public class Chunk
{
    public GameObject chunkObject;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public int[,,] blocks;
    private int sizeX, sizeY, sizeZ;
    private Vector3Int chunkOrigin; // posición global del chunk

    public Chunk(int x, int y, int z, Vector3Int origin)
    {
        sizeX = x;
        sizeY = y;
        sizeZ = z;
        blocks = new int[sizeX, sizeY, sizeZ];
        chunkOrigin = origin;
    }

    public void RegenerateMesh(BlockType[] bloquesDisponibles, System.Func<Vector3Int, int> getBlockAtWorldPos, int lod)
    {
        if (meshFilter == null || meshRenderer == null || meshCollider == null) return;
        if (bloquesDisponibles == null || bloquesDisponibles.Length == 0) return;

        int step = (int)Mathf.Pow(2, lod); // LOD aplicado
        int submeshCount = bloquesDisponibles.Length * 6;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int>[] submeshTriangles = new List<int>[submeshCount];
        for (int i = 0; i < submeshCount; i++) submeshTriangles[i] = new List<int>();

        int vertexOffset = 0;

        for (int x = 0; x < sizeX; x += step)
        {
            for (int y = 0; y < sizeY; y += step)
            {
                for (int z = 0; z < sizeZ; z += step)
                {
                    int tipo = blocks[x, y, z];
                    if (tipo <= 0 || tipo > bloquesDisponibles.Length) continue;

                    Vector3 blockPos = new Vector3(x, y, z);

                    for (int face = 0; face < 6; face++)
                    {
                        Vector3Int worldPos = chunkOrigin + new Vector3Int(x, y, z);
                        Vector3Int worldNeighbor = GetNeighborPos(worldPos.x, worldPos.y, worldPos.z, face);

                        int tipoVecino = getBlockAtWorldPos(worldNeighbor);
                        if (tipoVecino > 0) continue; // ← corregido para evitar parpadeo

                        Vector3[] faceVerts = GetFaceVertices(face, blockPos);
                        vertices.AddRange(faceVerts);

                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(1, 0));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(0, 1));

                        int submeshIndex = (tipo - 1) * 6 + face;

                        submeshTriangles[submeshIndex].Add(vertexOffset + 0);
                        submeshTriangles[submeshIndex].Add(vertexOffset + 1);
                        submeshTriangles[submeshIndex].Add(vertexOffset + 2);
                        submeshTriangles[submeshIndex].Add(vertexOffset + 0);
                        submeshTriangles[submeshIndex].Add(vertexOffset + 2);
                        submeshTriangles[submeshIndex].Add(vertexOffset + 3);

                        vertexOffset += 4;
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = (vertexOffset > 65000) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.subMeshCount = submeshCount;
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);

        for (int i = 0; i < submeshCount; i++)
        {
            mesh.SetTriangles(submeshTriangles[i], i, true);
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;

        Material fallback = GetFallbackMaterial(bloquesDisponibles);
        Material[] mats = new Material[submeshCount];
        for (int t = 0; t < bloquesDisponibles.Length; t++)
        {
            Material[] faces = bloquesDisponibles[t].faceMaterials;
            for (int face = 0; face < 6; face++)
            {
                int idx = t * 6 + face;
                mats[idx] = (faces != null && faces.Length > face && faces[face] != null) ? faces[face] : fallback;
            }
        }
        if (ChunksController.debugLod == 1)
        {
            // 🎨 Visualización del LOD por color
            Color lodColor = lod == 0 ? Color.white : (lod == 1 ? Color.yellow : Color.red);
            foreach (var mat in mats)
                if (mat != null) mat.color = lodColor;
        }
        
        meshRenderer.sharedMaterials = mats; 
    }

    public void RequestMeshGeneration(BlockType[] bloquesDisponibles, System.Func<Vector3Int, int> getBlockAtWorldPos, int lod)
    {
        RegenerateMesh(bloquesDisponibles, getBlockAtWorldPos, lod);
    }

    private Material GetFallbackMaterial(BlockType[] bloquesDisponibles)
    {
        foreach (var bloque in bloquesDisponibles)
        {
            foreach (var mat in bloque.faceMaterials)
                if (mat != null) return mat;
        }
        var fallback = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        fallback.color = Color.gray;
        return fallback;
    }

    private Vector3Int GetNeighborPos(int x, int y, int z, int face)
    {
        switch (face)
        {
            case 0: return new Vector3Int(x, y, z + 1);
            case 1: return new Vector3Int(x, y, z - 1);
            case 2: return new Vector3Int(x - 1, y, z);
            case 3: return new Vector3Int(x + 1, y, z);
            case 4: return new Vector3Int(x, y + 1, z);
            case 5: return new Vector3Int(x, y - 1, z);
            default: return new Vector3Int(x, y, z);
        }
    }

    private Vector3[] GetFaceVertices(int faceIndex, Vector3 o)
    {
        switch (faceIndex)
        {
            case 0: return new Vector3[] { o + new Vector3(0,0,1), o + new Vector3(1,0,1), o + new Vector3(1,1,1), o + new Vector3(0,1,1) };
            case 1: return new Vector3[] { o + new Vector3(1,0,0), o + new Vector3(0,0,0), o + new Vector3(0,1,0), o + new Vector3(1,1,0) };
            case 2: return new Vector3[] { o + new Vector3(0,0,0), o + new Vector3(0,0,1), o + new Vector3(0,1,1), o + new Vector3(0,1,0) };
            case 3: return new Vector3[] { o + new Vector3(1,0,1), o + new Vector3(1,0,0), o + new Vector3(1,1,0), o + new Vector3(1,1,1) };
            case 4: return new Vector3[] { o + new Vector3(0,1,1), o + new Vector3(1,1,1), o + new Vector3(1,1,0), o + new Vector3(0,1,0) };
            case 5: return new Vector3[] { o + new Vector3(0,0,0), o + new Vector3(1,0,0), o + new Vector3(1,0,1), o + new Vector3(0,0,1) };
            default: return new Vector3[4];
        }
    }
}
