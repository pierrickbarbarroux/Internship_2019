using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using UnityEngine;

public class Triangulate : MonoBehaviour
{

    public void CreateShapeTriangulate(List<Vector2> points, bool reverse)
    {
        MeshFilter mf = this.gameObject.AddComponent<MeshFilter>();
        MeshCollider mc = this.gameObject.AddComponent<MeshCollider>();
        this.gameObject.AddComponent<MeshRenderer>();

        Mesh mesh = mf.mesh;
        //Mesh mesh = mf.sharedMesh;

        List<int> indices = null;
        List<Vector3> vertices = null;

        Triangulation.triangulate(points, out indices, out vertices);

        Vector2[] uvs = new Vector2[vertices.ToArray().Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            //uvs[i] = new Vector2(vertices.ToArray()[i].x / vertices.ToArray().Length / Mathf.Sqrt(vertices.ToArray().Length), vertices.ToArray()[i].z / vertices.ToArray().Length / Mathf.Sqrt(vertices.ToArray().Length));
            //uvs[i] = new Vector2(vertices.ToArray()[i].x / (vertices.ToArray().Length * 2), vertices.ToArray()[i].z / (vertices.ToArray().Length * 2));
            uvs[i] = new Vector2(vertices.ToArray()[i].x/4, vertices.ToArray()[i].z/4);
        }

        if (reverse)
        {
            indices.Reverse();
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.uv = uvs;
        mesh.Optimize();
        mesh.RecalculateNormals();

        this.GetComponent<MeshCollider>().sharedMesh = mesh;




    }
}


