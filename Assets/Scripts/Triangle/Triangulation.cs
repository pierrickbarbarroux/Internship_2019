using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Meshing.Algorithm;

public class Vertex3 : Vertex
{
    public Vertex3(double x, double y, double z)
        : base(x, y)
    {
        this.Z = z;
    }

    public double Z { get; set; }
}

public class Triangulation : MonoBehaviour
{
    /*
    public static bool triangulate(List<Vector2> points, List<List<Vector2>> holes, out List<int> outIndices, out List<Vector3> outVertices)
    {
        outVertices = new List<Vector3>();
        outIndices = new List<int>();
        Polygon poly = new Polygon();

        //Points and segment
        for (int i = 0; i < points.Count; i++)
        {
            poly.Add(new Vertex(points[i].x, points[i].y));

            if (i == points.Count - 1)
            {
                poly.Add(new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[0].x, points[0].y)));
            }
            else
            {
                poly.Add(new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[i + 1].x, points[i + 1].y)));
            }
        }

        //Holes
        for (int i = 0; i < holes.Count; i++)
        {
            List<Vertex> vertices = new List<Vertex>();
            for (int j = 0; j < holes[i].Count; j++)
            {
                vertices.Add(new Vertex(holes[i][j].x, holes[i][j].y));
            }
            poly.Add(new Contour(vertices), true); // true si y a des holes, false sinon
        }

        var mesh = poly.Triangulate();

        foreach (ITriangle t in mesh.Triangles)
        {
            for (int j = 2; j >= 0; j--)
            {
                bool found = false;
                for (int k = 0; k < outVertices.Count; k++)
                {
                    if ((outVertices[k].x == t.GetVertex(j).X) && (outVertices[k].z == t.GetVertex(j).Y))
                    {
                        outIndices.Add(k);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    outVertices.Add(new Vector3((float)t.GetVertex(j).X, 0, (float)t.GetVertex(j).Y));
                    outIndices.Add(outVertices.Count - 1);
                }
            }
        }
        return true;
    }
    */

    public static bool triangulate(List<Vector2> points, out List<int> outIndices, out List<Vector3> outVertices)
    {
        outVertices = new List<Vector3>();
        outIndices = new List<int>();
        Polygon poly = new Polygon();

        //Points and segment
        for (int i = 0; i < points.Count; i++)
        {
            poly.Add(new Vertex(points[i].x, points[i].y));

            if (i == points.Count - 1)
            {
                poly.Add(new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[0].x, points[0].y)));
            }
            else
            {
                poly.Add(new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[i + 1].x, points[i + 1].y)));
            }
        }

        var mesh = poly.Triangulate();

        foreach (ITriangle t in mesh.Triangles)
        {
            for (int j = 2; j >= 0; j--)
            {
                bool found = false;
                for (int k = 0; k < outVertices.Count; k++)
                {
                    if ((outVertices[k].x == t.GetVertex(j).X) && (outVertices[k].z == t.GetVertex(j).Y))
                    {
                        outIndices.Add(k);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    outVertices.Add(new Vector3((float)t.GetVertex(j).X, 0, (float)t.GetVertex(j).Y));
                    outIndices.Add(outVertices.Count - 1);
                }
            }
        }
        return true;
    }
    /*
    public static bool triangulate(List<Vector3> points, out List<int> outIndices, out List<Vector3> outVertices)
    {
        outVertices = new List<Vector3>();
        outIndices = new List<int>();
        //Polygon poly = new Polygon();

        //Points and segment
        /*
        for (int i = 0; i < points.Count; i++)
        {
            poly.Add(new Vertex3(points[i].x, points[i].y, points[i].z));

            if (i == points.Count - 1)
            {
                poly.Add(new Segment(new Vertex3(points[i].x, points[i].y, points[i].z), new Vertex3(points[0].x, points[0].y, points[0].z)));
            }
            else
            {
                poly.Add(new Segment(new Vertex3(points[i].x, points[i].y, points[i].z), new Vertex3(points[i + 1].x, points[i + 1].y, points[i + 1].z)));
            }
        }
        var myvertex = new List<Vertex>();
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p = points[i];
            myvertex.Add(new Vertex3(p.x, p.y, p.z));
        }

        var mesher = new GenericMesher(new Dwyer());

        var mesh = mesher.Triangulate(myvertex);

        foreach (ITriangle t in mesh.Triangles)
        {
            for (int j = 2; j >= 0; j--)
            {
                bool found = false;
                var v = (Vertex3)t.GetVertex(j);
                for (int k = 0; k < outVertices.Count; k++)
                {
                    if ((outVertices[k].x == v.X) && (outVertices[k].z == v.Y) && (outVertices[k].y == v.Z))
                    {
                        outIndices.Add(k);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    outVertices.Add(new Vector3((float)v.X, (float)v.Y, (float)v.Z));
                    outIndices.Add(outVertices.Count - 1);
                }
            }
        }
        return true;
    }
    */
}
