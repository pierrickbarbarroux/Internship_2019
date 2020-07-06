using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Ce script doit être placé sur le terrain référent et sera ajouté automatiquement sur chaque autre terrain lors de la génération des terrains.
/// Cette classe génère les forêts.
/// </summary>
public class GenerateForest : MonoBehaviour
{
    [Tooltip("Modèles des arbres pour les forêts.")]
    public GameObject[] myTrees;

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
    Vector2[] uvs;
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

        //if (Input.GetKeyDown("f"))
        //{
        //    StartCoroutine(GetPointsForest());
        //}
        if (Input.GetKeyDown("c") && this.gameObject.GetComponent<MeshCollider>().enabled == false)
        {
            this.gameObject.GetComponent<MeshCollider>().enabled = true;

        }
        //if (Input.GetKeyDown("p"))
        //{
        //    StartCoroutine(SpawnTrees());
        //}

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
    /// Récupère les points délimitant les zones de forêt et crée les mesh correspondant.
    /// </summary>
    /// <param name="mnt">MNT sur lequel on génère les zones de forêt</param>
    /// <returns>Retourne rien</returns>
    public IEnumerator GetPointsForest(GameObject mnt)
    {
        //Pour plus de lisibilité dans la scène lors du run, on regroupe tous les zones de forêt dans un seul objet
        GameObject All_forest_zones;
        if (GameObject.Find("All_forest_zones") == null)
        {
            All_forest_zones = new GameObject();
            All_forest_zones.name = "All_forest_zones";
        }
        else
        {
            All_forest_zones = GameObject.Find("All_forest_zones");
        }

        float alt = Mathf.Max(mnt.GetComponent<Tile>().altitudes);
        string myjson;

        //Téléchargement du fichier json contenant les points délimitant les bâtiments.
        //Le fichier est téléchargé uniquement si le fichier n'existe pas déjà.
        string path = "Assets/Data/Forests/forest_"+typename+"_xbot_" + mnt.GetComponent<Tile>().left_down_x + "_ybot_" + mnt.GetComponent<Tile>().left_down_y + "_xtop_" + mnt.GetComponent<Tile>().right_up_x + "_ytop_" + mnt.GetComponent<Tile>().right_up_y + ".json";
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

        for (int j = 0; j < bigjson["features"].Count; j++)
        {
            if (GameObject.Find(bigjson["features"][j]["properties"]["id"]) == null)
            {
                GameObject new_mesh = new GameObject();
                new_mesh.layer = 8;

                //J'avais ajouté un composant Tile aux mesh délimitant les zones de forêts.
                //Mais ce n'est pas vraiment utile, si j'ai le temps, je nettoierais ça proprement
                //Pour l'instant je préfère laisser 
                new_mesh.AddComponent<Tile>();
                new_mesh.GetComponent<Tile>().is_ref = false;
                new_mesh.GetComponent<Tile>().is_forest_mesh = true;
                new_mesh.AddComponent<Triangulate>();
                new_mesh.name = bigjson["features"][j]["properties"]["id"];

                //On crée un objet comprenant l'essemble des points délimitant la zone de forêt
                GameObject forestPoints = new GameObject("forestPoints");

                //items contient l'ensemble des coordonnées des points délimitant la zone de forêt
                JSONArray items = (JSONArray)bigjson["features"][j]["geometry"]["coordinates"][0][0]; //il faut rester en JSONArray sinon il y a un problème pour lire les valeurs

                List<Vector2> myarray = new List<Vector2>();
                for (int i = 0; i < items.Count; i++)
                {
                    //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    float x = items[i][0] - left_down.Item1;
                    float z = items[i][1] - left_down.Item2;

                    myarray.Add(new Vector2(x, z));
                }

                //Triangularisation et création du mesh
                new_mesh.GetComponent<Triangulate>().CreateShapeTriangulate(myarray, false);

                //Quelques modifications pour placer le mesh au bon endroit
                forestPoints.transform.Rotate(0, -90, 0);
                new_mesh.transform.Rotate(0, -90, 0);
                forestPoints.transform.position = mnt.transform.position;
                forestPoints.transform.Translate(0, 0, -256);
                new_mesh.transform.Translate(mnt.GetComponent<Tile>().position_z * 255, alt + 300, -255 + mnt.GetComponent<Tile>().position_x * 255);
                
                //On supprime ce qui n'est plus utile
                Destroy(forestPoints);

                //On désactive le rendu visuel du mesh, seul le collider nous intéresse
                new_mesh.GetComponent<MeshRenderer>().enabled = false;

                //Pour plus de clarté...
                new_mesh.transform.parent = All_forest_zones.transform;
            }
        }
    }

    /// <summary>
    /// Plante un seul arbre sur le terrain, en prenant en compte les zones de forêt.
    /// Utilise principalement des raycast.
    /// </summary>
    /// <param name="mnt">MNT où l'on génère l'arbre</param>
    void SpawnTree(GameObject mnt)
    {
        //On part d'un point aléatoire situé à l'intérieur du terrain (selon x et z)
        //Par contre on situe le point à une très grande hauteur.
        Vector3 start = new Vector3();
        if (GetComponent<MeshRenderer>() != null)
        {
            Renderer r = mnt.GetComponent<MeshRenderer>(); // assumes the terrain is in a mesh renderer on the same GameObject
            float randomX = Random.Range(r.bounds.min.x, r.bounds.max.x);
            float randomZ = Random.Range(r.bounds.min.z, r.bounds.max.z);
            start = new Vector3(randomX, r.bounds.max.y + 2000f, randomZ);
        }
        else if (GetComponent<Terrain>() != null)
        {
            Terrain t = mnt.GetComponent<Terrain>();
            TerrainData d = t.terrainData;
            float randomX = Random.Range(d.bounds.min.x, d.bounds.max.x);
            float randomZ = Random.Range(d.bounds.min.z, d.bounds.max.z);
            start = new Vector3(randomX + mnt.transform.position.x, d.bounds.max.y + 1000f, randomZ + mnt.transform.position.z);
        }

        RaycastHit hit;
        RaycastHit hit2;
        int layerMask = 1 << 9;
        int layerMask2 = 1 << 8;

        /** On tire deux raycast.
         *  Le premier part du point start désigné plus haut, et est dirigé vers le bas (en -Vector3.up)
         *  Si on touche un mesh correspondant à une zone de forêt, on retire un rayon dans la même direction
         *  afin de pouvoir toucher le terrain. Si on touche bien un terrain, on plante un arbre au niveau de l'impact
         */
        if (Physics.Raycast(start, -Vector3.up, out hit, 3000f, layerMask2))
        {

            if (hit.transform.gameObject.GetComponent<Tile>() != null)
            {
                if (hit.collider.GetComponent<Tile>().is_forest_mesh)
                {
                    if (Physics.Raycast(hit.point, -Vector3.up, out hit2, 1000, layerMask))
                    {
                        if (hit2.transform.gameObject.tag == "Tile_tag" || hit2.transform.gameObject.tag == "MNT_tag" || hit2.transform.gameObject.tag == "Terrain_tag")
                        {
                            GameObject arbre = Instantiate(myTrees[Random.Range(0, myTrees.Length)], hit2.point, Quaternion.identity);
                            arbre.isStatic = true; //Les objets statics sont moins coûteux en terme de puissance de calcul
                        }
                        else if (hit2.transform.gameObject.tag == "Tpzone_tag")
                        {
                            GameObject arbre = Instantiate(myTrees[Random.Range(0, myTrees.Length)], hit2.point - new Vector3(0, 0.05f, 0), Quaternion.identity);
                            arbre.isStatic = true; //Les objets statics sont moins coûteux en terme de puissance de calcul

                        }
                    }

                }
            }
        }
    }

    /// <summary>
    /// On plante plusieurs arbres au niveau d'un terrain.
    /// SpawnTrees appelle juste plusieurs fois la méthode SpawnTree.
    /// </summary>
    /// <param name="mnt">Terrain où l'on souhaite faire pousser des arbres</param>
    public void SpawnTrees(GameObject mnt)
    {
        for (int i = 0; i < 200; i++)
        {
            SpawnTree(mnt);
        }
    }

    /// <summary>
    /// Retire tous les éléments du type (tag) object_to_remove_tag autour des éléments du type (tag) object_to_clean_tag.
    /// Utiliser prinsipalement pour supprimer les arbres dans les bâtiments et sur les routes 
    /// </summary>
    /// <param name="object_to_clean_tag">tag des objest que l'on veut nettoyer</param>
    /// <param name="object_to_remove_tag">tag des objets que l'on veut supprimer</param>
    /// <param name="radius">taille de la sphère qui détecte les colliders autour des objets à nettoyer</param>
    public static void CleanAround(string object_to_clean_tag, string object_to_remove_tag, int radius)
    {
        GameObject[] objets = GameObject.FindGameObjectsWithTag(object_to_clean_tag);
        foreach (GameObject objet in objets)
        {
            if (objet.GetComponent<MeshCollider>() != null)
            {
                //On récupère l'ensemble des colliders entrant en contact avec la sphère de centre le bound center
                //du bâtiment et de rayon 10 
                Collider[] colliders = Physics.OverlapSphere(objet.GetComponent<MeshCollider>().bounds.center, radius);
                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.tag == object_to_clean_tag)
                    {
                        Destroy(collider.gameObject);
                    }
                }
            }

        }
    }

    /// <summary>
    /// Regroupe tous les éléments de type (tag) "Vege_tag" dans un même objet parent.
    /// N'est pas vraiment utilisé.
    /// </summary>
    public static void GroupVege()
    {
        if (Input.GetKeyDown("h"))
        {
            GameObject All_forest;
            if (GameObject.Find("All_vege") == null)
            {
                All_forest = new GameObject();
                All_forest.name = "All_vege";
            }
            else
            {
                All_forest = GameObject.Find("All_vege");
            }
            GameObject[] all_vege = GameObject.FindGameObjectsWithTag("Vege_tag");
            foreach (GameObject vege in all_vege)
            {
                vege.transform.parent = All_forest.transform;
            }
        }
    }

}
