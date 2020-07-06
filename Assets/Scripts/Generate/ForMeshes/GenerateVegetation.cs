using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;

/// <summary>
/// Ce script doit être placé sur le terrain référent et sera ajouté automatiquement sur chaque autre terrain lors de la génération des terrains.
/// Cette classe génère la végétation (comme les forêts mais en sone urbaine principalement).
/// </summary>
public class GenerateVegetation : MonoBehaviour
{

    [Tooltip("Ensemble des modèles de végétation (arbre, buisson,...)")]
    public GameObject[] myVege;

    [Tooltip("Nom de la couche WFS")]
    public string typename;

    [Tooltip("Format du fichier téléchargé. On utilisera le format 'json'")]
    public string format;

    /** Contour (box) en Lambert 93 de la tuile.
     *  left_down correspond au coin inférieur gauche, right_up au coin supérieur droit.
     */
    (float, float) left_down;
    (float, float) right_up;


    // Start is called before the first frame update
    void Start()
    {
        left_down = (this.GetComponent<Tile>().left_down_x, this.GetComponent<Tile>().left_down_y);
        right_up = (this.GetComponent<Tile>().right_up_x, this.GetComponent<Tile>().right_up_y);
    }

    //BDTOPO_BDD_WLD_WGS84:zone_vegetation

    /// <summary>
    /// Récupère les points délimitant les zones de végétation et crée les mesh correspondant.
    /// Quasiment similaire à GetPointsForest, j'aurais dû faire une seule fonction générique...
    /// </summary>
    /// <param name="mnt">MNT sur lequel on génère les zones de forêt</param>
    /// <returns></returns>
    public IEnumerator GetPointsVege(GameObject mnt)
    {
        GameObject All_vege_zones;
        if (GameObject.Find("All_vege_zone") == null)
        {
            All_vege_zones = new GameObject();
            All_vege_zones.name = "All_vege_zone";
        }
        else
        {
            All_vege_zones = GameObject.Find("All_vege_zone");
        }

        float alt = Mathf.Max(mnt.GetComponent<Tile>().altitudes);
        string myjson;

        string path = "Assets/Data/Vegetation/field_"+typename+"_xbot_" + mnt.GetComponent<Tile>().left_down_x + "_ybot_" + mnt.GetComponent<Tile>().left_down_y + "_xtop_" + mnt.GetComponent<Tile>().right_up_x + "_ytop_" + mnt.GetComponent<Tile>().right_up_y + ".json";
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
        if (bigjson["features"] != null)
        {
            for (int j = 0; j < bigjson["features"].Count; j++)
            {
                if (GameObject.Find(bigjson["features"][j]["properties"]["id"]) == null)
                {
                    GameObject new_mesh = new GameObject();
                    new_mesh.layer = 10;
                    new_mesh.AddComponent<Tile>();
                    new_mesh.GetComponent<Tile>().is_ref = false;
                    new_mesh.GetComponent<Tile>().is_forest_mesh = false;
                    new_mesh.GetComponent<Tile>().is_vege_mesh = true;
                    new_mesh.AddComponent<Triangulate>();
                    new_mesh.name = bigjson["features"][j]["properties"]["id"];

                    GameObject vegePoints = new GameObject("vegePoints");
                    JSONArray items = (JSONArray)bigjson["features"][j]["geometry"]["coordinates"][0][0]; //il faut rester en JSONArray sinon il y a un problème pour lire les valeurs
                    List<Vector2> myarray = new List<Vector2>();
                    for (int i = 0; i < items.Count; i++)
                    {
                        float x = items[i][0] - left_down.Item1;
                        float z = items[i][1] - left_down.Item2;
                        myarray.Add(new Vector2(x, z));
                    }
                    new_mesh.GetComponent<Triangulate>().CreateShapeTriangulate(myarray, false);
                    vegePoints.transform.Rotate(0, -90, 0);
                    new_mesh.transform.Rotate(0, -90, 0);
                    vegePoints.transform.position = mnt.transform.position;
                    vegePoints.transform.Translate(0, 0, -256);
                    new_mesh.transform.Translate(mnt.GetComponent<Tile>().position_z * 255, Mathf.Max(mnt.GetComponent<Tile>().altitudes) + 50, -255 + mnt.GetComponent<Tile>().position_x * 255);
                    Destroy(vegePoints);
                    new_mesh.GetComponent<MeshRenderer>().enabled = false;
                    new_mesh.transform.parent = All_vege_zones.transform;
                }
            }
        }
    }

    /// <summary>
    /// Plante un seul arbre sur le terrain, en prenant en compte les zones de vegetation.
    /// Utilise principalement des raycast.
    /// Similaire à la méthode pour les forêt.
    /// </summary>
    /// <param name="mnt">MNT où l'on génère l'arbre</param>
    void SpawnVege(GameObject mnt)
    {
        Vector3 start = new Vector3();
        if (GetComponent<MeshRenderer>() != null)
        {
            Renderer r = mnt.GetComponent<MeshRenderer>(); // assumes the terrain is in a mesh renderer on the same GameObject
            float randomX = Random.Range(r.bounds.min.x, r.bounds.max.x);
            float randomZ = Random.Range(r.bounds.min.z, r.bounds.max.z);
            start = new Vector3(randomX, r.bounds.max.y + 1000, randomZ);
        }
        else if (GetComponent<Terrain>() != null)
        {
            Terrain t = mnt.GetComponent<Terrain>();
            TerrainData d = t.terrainData;
            float randomX = Random.Range(d.bounds.min.x, d.bounds.max.x);
            float randomZ = Random.Range(d.bounds.min.z, d.bounds.max.z);
            start = new Vector3(randomX + mnt.transform.position.x, d.bounds.max.y + 1000, randomZ + mnt.transform.position.z);
        }

        RaycastHit hit;
        RaycastHit hit2;
        //Vector3 start = new Vector3(randomX, r.bounds.max.y + 75f, randomZ);
        int layerMask = 1 << 9; //Les layers ont disparus du projet, donc ligne obsolète tant qu'ils ne sont pas rajoutés
        int layerMask2 = 1 << 10;

        if (Physics.Raycast(start, -Vector3.up, out hit, 3000, layerMask2))
        {
            if (hit.transform.gameObject.GetComponent<Tile>() != null)
            {
                if (hit.collider.GetComponent<Tile>().is_vege_mesh)
                {
                    //if (Physics.Raycast(origin:, direction:, hitInfo:, maxDistance: DistanceJoint2D, layerMask:) )
                    if (Physics.Raycast(hit.point, -Vector3.up, out hit2, 2000f, layerMask))
                    {
                        if (hit2.transform.gameObject.tag == "Tile_tag" || hit2.transform.gameObject.tag == "MNT_tag" || hit2.transform.gameObject.tag == "Terrain_tag")
                        {
                            GameObject vege = Instantiate(myVege[Random.Range(0, myVege.Length)], hit2.point, Quaternion.identity);
                            vege.isStatic = true;
                        }
                        else if (hit2.transform.gameObject.tag == "Tpzone_tag")
                        {
                            GameObject vege = Instantiate(myVege[Random.Range(0, myVege.Length)], hit2.point - new Vector3(0, 0.05f, 0), Quaternion.identity);
                            vege.isStatic = true;
                        }
                    }

                }
            }
        }
    }
    public void SpawnVeges(GameObject mnt)
    {
        for (int i = 0; i < 100; i++)
        {
            SpawnVege(mnt);
        }
    }
}
