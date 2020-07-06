using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

/// <summary>
/// Classe de test qui n'est plus utilisait dans le projet final
/// </summary>
public class GenerateMeshTestForest : MonoBehaviour
{

    (float, float) left_down;
    (float, float) right_up;

    public string typename;
    public string format;


    Transform old_trans;
    Transform new_trans;

    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;

    Vector2[] myarray;

    // Start is called before the first frame update
    void Start()
    {
        left_down = (this.GetComponent<Tile>().left_down_x, this.GetComponent<Tile>().left_down_y);
        right_up = (this.GetComponent<Tile>().right_up_x, this.GetComponent<Tile>().right_up_y);

        StartCoroutine(GetPointsForest());
        Vector2[] myarray = new Vector2[] {
            new Vector2(0,0),
            new Vector2(0,50),
            new Vector2(50,50),
            new Vector2(50,100),
            new Vector2(0,100),
            new Vector2(0,150),
            new Vector2(150,150),
            new Vector2(150,100),
            new Vector2(100,100),
            new Vector2(100,50),
            new Vector2(150,50),
            new Vector2(150,0),
        };
        CreateShape(myarray);
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateMesh();
    }


    void CreateShape(Vector2[] vertices2D)
    {

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        gameObject.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;

    }


    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 1f);
        }
    }


    private IEnumerator GetPointsForest()
    {
        float alt = Mathf.Max(this.GetComponent<Tile>().altitudes);
        string myjson;
        CoroutineResult cd = new CoroutineResult(this, DataController.GetWfsData(DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2)));
        yield return cd.coroutine;
        myjson = (string)cd.result;
        var bigjson = JSON.Parse(myjson);
        //Debug.Log(DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2));

        GameObject forestPoints = new GameObject("forestPoints");
        JSONArray items = (JSONArray)bigjson["features"][2]["geometry"]["coordinates"][0][0]; //il faut rester en JSONArray sinon il y a un problème pour lire les valeurs
        myarray = new Vector2[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            float x = items[i][0] - left_down.Item1;
            float z = items[i][1] - left_down.Item2;

            myarray[i] = new Vector2(x, z);
        }
    }

}
