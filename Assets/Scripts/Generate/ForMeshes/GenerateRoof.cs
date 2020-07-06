using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe n'est pas utilisée
/// Cette classe ne sert à rien dans le projet final.
/// </summary>
public class GenerateRoof : MonoBehaviour
{

    public void CreateShape(Vector2[] points)
    {
        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(points);
        int [] triangles = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[points.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[vertices.Length - 1 - i] = new Vector3(points[i].x, 0, points[i].y);
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.Optimize();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
