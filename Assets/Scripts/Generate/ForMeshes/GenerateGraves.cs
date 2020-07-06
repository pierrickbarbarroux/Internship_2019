using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;

/// <summary>
/// Ce script doit être placé sur le terrain référent et sera ajouté automatiquement sur chaque autre terrain lors de la génération des terrains.
/// Cette classe génère les cimetierres.
/// </summary>
public class GenerateGraves : MonoBehaviour
{
    [Tooltip("Nom de la couche WFS.")]
    public string typename;

    [Tooltip("Format du fichier télécharger. On choisira 'json'.")]
    public string format;

    /** Contour (box) en Lambert 93 de la tuile.
     *  left_down correspond au coin inférieur gauche, right_up au coin supérieur droit.
     */
    (float, float) left_down;
    (float, float) right_up;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    // Start is called before the first frame update
    void Start()
    {
        left_down = (this.GetComponent<Tile>().left_down_x, this.GetComponent<Tile>().left_down_y);
        right_up = (this.GetComponent<Tile>().right_up_x, this.GetComponent<Tile>().right_up_y);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("g"))
        {
            StartCoroutine(GetPointsGraves());
        }
    }

    //Non utilisée
    void CreateShape(Vector2[] points)
    {
        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(points);
        triangles = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[points.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[vertices.Length - 1 - i] = new Vector3(points[i].x, 0, points[i].y);
        }

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.Optimize();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    //Non utilisée
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        //mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Récupère les points délimitant les zones de cimetierre et crée les mesh correspondant.
    /// </summary>
    /// <param name="mnt">MNT sur lequel on génère les zones de cimetierre</param>
    /// <returns>Retourne rien</returns>
    private IEnumerator GetPointsGraves()
    {
        //Pour plus de lisibilité dans la scène lors du run, on regroupe tous les zones de forêt dans un seul objet
        GameObject All_graves_zones;
        if (GameObject.Find("All_graves_zones") == null)
        {
            All_graves_zones = new GameObject();
            All_graves_zones.name = "All_graves_zones";
        }
        else
        {
            All_graves_zones = GameObject.Find("All_graves_zones");
        }

        float alt = Mathf.Max(this.GetComponent<Tile>().altitudes);
        string myjson;

        string path = "Assets/Data/Graveyard/graveyard_" + typename + "_xbot_" + left_down.Item1 + "_ybot_" + left_down.Item2 + "_xtop_" + right_up.Item1 + "_ytop_" + right_up.Item2 + ".json";
        //Téléchargement du fichier json contenant les points délimitant les bâtiments.
        //Le fichier est téléchargé uniquement si le fichier n'existe pas déjà.
        if (!File.Exists(path))
        {
            string url = DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2);
            StartCoroutine(DataController.WriteDataFile(url, path));
            yield return null;
        }
        StreamReader reader = new StreamReader(path);
        myjson = reader.ReadToEnd();
        var bigjson = JSON.Parse(myjson);
        reader.Close();

        //CoroutineResult cd = new CoroutineResult(this, DataController.GetWfsData(DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2)));
        //yield return cd.coroutine;
        //myjson = (string)cd.result;
        //var bigjson = JSON.Parse(myjson);

        for (int j = 0; j < bigjson["features"].Count; j++)
        {
            if (GameObject.Find(bigjson["features"][j]["properties"]["id"]) == null)
            {
                Debug.Log(DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2));
                GameObject new_mesh = new GameObject();
                new_mesh.layer = 8;
                new_mesh.AddComponent<Tile>();
                new_mesh.GetComponent<Tile>().is_ref = false;
                new_mesh.GetComponent<Tile>().is_grave_mesh = true;
                new_mesh.AddComponent<Triangulate>();
                new_mesh.name = bigjson["features"][j]["properties"]["id"];
                new_mesh.tag = "Graveyard_tag";
                new_mesh.layer = 12;

                //On crée un objet comprenant l'essemble des points délimitant la zone de cimetière
                GameObject gravePoints = new GameObject("gravePoints");

                //items contient l'ensemble des coordonnées des points délimitant la zone de champs
                JSONArray items = (JSONArray)bigjson["features"][j]["geometry"]["coordinates"][0][0]; //il faut rester en JSONArray sinon il y a un problème pour lire les valeurs
                List<Vector2> myarray = new List<Vector2>();
                for (int i = 0; i < items.Count; i++)
                {
                    float x = items[i][0] - left_down.Item1;
                    float z = items[i][1] - left_down.Item2;

                    myarray.Add(new Vector2(x, z));
                }

                //Triangularisation et création du mesh
                new_mesh.GetComponent<Triangulate>().CreateShapeTriangulate(myarray, true);

                //Quelques modifications pour placer le mesh au bon endroit
                gravePoints.transform.Rotate(0, -90, 0);
                new_mesh.transform.Rotate(0, -90, 0);
                gravePoints.transform.position = transform.position;
                gravePoints.transform.Translate(0, 0, -256);
                new_mesh.transform.Translate(GetComponent<Tile>().position_z * 255, alt + 20, -255 + GetComponent<Tile>().position_x * 255);

                //On supprime ce qui n'est plus utile, le mesh étant déjà généré
                Destroy(gravePoints);


                new_mesh.GetComponent<MeshRenderer>().enabled = false;
                new_mesh.transform.parent = All_graves_zones.transform;
            }
        }

    }

    /// <summary>
    /// Dispose plusieurs tombes sur le terrain, en prenant en compte les zones de cimetière.
    /// Utilise principalement des raycast.
    /// </summary>
    /// <param name="mnt">MNT où l'on génère l'arbre</param>
    public static void SpawnGrave(GameObject[] graves)
    {
        GameObject All_graves;
        if (GameObject.Find("All_graves") == null)
        {
            All_graves = new GameObject();
            All_graves.name = "All_graves";
        }
        else
        {
            All_graves = GameObject.Find("All_graves");
        }

        RaycastHit hit;
        RaycastHit hit2;
        RaycastHit hit3;
        Vector3 new_grave_pos;
        Vector3 start;
        int layerMask = 1 << 9; //layer du MNT (bizarrement, les layers ont disparu du projet....)
        int layerMask_grave = 1 << 12; //layer des zones de cimetière 

        GameObject[] tile_mygraves = GameObject.FindGameObjectsWithTag("Graveyard_tag");

        /** On tire 3 raycast. On construit un quadrillage centrée sur la zone de cimetière. Ce quadrillage nous permet
         * de tirer un grand nombre de raycast vers le terrain. Pour ceux qui touche bien un terrain, un autre raycast est tiré
         * vers le haut pour vérifier que ce raycast est bien dans la zone de cimetière. Si tel est le cas, alors on tire un dernier
         * raycast vers le terrain. On disposela tombe au point d'impact.
         */
        foreach (GameObject grave in tile_mygraves)
        {
            start = grave.GetComponent<MeshRenderer>().bounds.center;
            start -= new Vector3(0, 1, 0);
            for (int i = -25; i < 25; i++)
            {
                for (int j = -35; j < 35; j++)
                {
                    new_grave_pos = start + new Vector3(i * 6, 0, j * 2f);
                    if (Physics.Raycast(new_grave_pos, -Vector3.up, out hit, 10000, layerMask))
                    {
                        if (hit.transform.gameObject.GetComponent<Tile>() != null)
                        {
                            if (hit.transform.gameObject.tag == "Tile_tag")
                            {
                                if (Physics.Raycast(hit.point, Vector3.up, out hit2, 10000, layerMask_grave))
                                {
                                    if (Physics.Raycast(hit2.point, -Vector3.up, out hit3, 10000, layerMask))
                                    {
                                        GameObject tombe = Instantiate(graves[Random.Range(0, graves.Length - 1)], hit3.point - new Vector3(0, 0.4f, 0), Quaternion.identity, All_graves.transform);
                                        tombe.isStatic = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}