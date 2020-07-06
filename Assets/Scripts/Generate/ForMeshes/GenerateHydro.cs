using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityStandardAssets.Water;

/// <summary>
/// Ce script doit être placé sur le terrain référent et sera ajouté automatiquement sur chaque autre terrain lors de la génération des terrains.
/// Cette classe génère les surfaces hydraulique.
/// Néanmoins, cette classe n'est pas totalement terminée. En effet, les points délimitants les zones d'eau 
/// possèdent une altitude. Or l'algorithme de triangularisation que j'utilise ne peut traiter que des surfaces planes .
/// Les surfaces d'eau rentrent donc parfois dans le sol ou sont légèrements sur élevées.
/// </summary>
public class GenerateHydro : MonoBehaviour
{
    [Tooltip("Material pour la surface d'eau. On prendra 'WaterProDaytime'")]
    public Material waterMat;

    [Tooltip("Nom de la couche WFS.")]
    public string typename1;

    [Tooltip("Format du fichier télécharger. On choisira 'json'.")]
    public string format;
    //BDTOPO_BDD_WLD_WGS84G:point_eau
    //BDTOPO_BDD_WLD_WGS84G:reservoir_eau
    //BDTOPO_BDD_WLD_WGS84G:surface_eau X
    //BDTOPO_BDD_WLD_WGS84G:canalisation_eau
    //BDTOPO_BDD_WLD_WGS84G:troncon_cours_eau X
    //BDTOPO_BDD_WLD_WGS84G:troncon_laisse
    //BDTOPO_V3_BETA:troncon_hydrographique

    /** Contour (box) en Lambert 93 de la tuile.
     *  left_down correspond au coin inférieur gauche, right_up au coin supérieur droit.
     */
    (float, float) left_down;
    (float, float) right_up;


    //931611
    //6260266

    // Start is called before the first frame update
    void Start()
    {
        left_down = (this.GetComponent<Tile>().left_down_x, this.GetComponent<Tile>().left_down_y);
        right_up = (this.GetComponent<Tile>().right_up_x, this.GetComponent<Tile>().right_up_y);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("b"))
        {
            StartCoroutine(GetPointsHydro(typename1));
        }
        
    }

    /// <summary>
    /// Récupère les points délimitant les zones d'eau et crée les mesh correspondant.
    /// </summary>
    /// <param name="typename">nom de la couche WFS</param>
    /// <returns>Ne retourne rien</returns>
    public IEnumerator GetPointsHydro(string typename)
    {
        //Pour plus de lisibilité dans la scène lors du run, on regroupe tous les zones d'eau dans un seul objet
        GameObject All_hydro_zones;
        if (GameObject.Find("All_hydro_zones") == null)
        {
            All_hydro_zones = new GameObject();
            All_hydro_zones.name = "All_hydro_zones";
        }
        else
        {
            All_hydro_zones = GameObject.Find("All_hydro_zones");
        }

        string myjson;
        string path = "Assets/Data/Hydro/" + "hydro_"+typename+"_xbot_" + this.GetComponent<Tile>().left_down_x + "_ybot_" + this.GetComponent<Tile>().left_down_y + "_xtop_" + this.GetComponent<Tile>().right_up_x + "_ytop_" + this.GetComponent<Tile>().right_up_y + ".json";
        
        //Téléchargement du fichier json contenant les points délimitant les surfaces d'eau.
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

        float alt = -1000;
        for (int j = 0; j < bigjson["features"].Count; j++)
        {
            if (GameObject.Find(bigjson["features"][j]["properties"]["id"]) == null)
            {
                Debug.Log(DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2));

                //Futur mesh correspondant à la surface que l'on va générer
                GameObject new_mesh = new GameObject();
                new_mesh.name = bigjson["features"][j]["properties"]["id"];
                new_mesh.layer = 8; //les layers ont l'air d'avoir disparus du projet... Cette ligne est potentiellement obsolète
                new_mesh.AddComponent<Triangulate>();
                new_mesh.isStatic = true; //Les objets static demande moins de puissance de calcul

                //On crée un objet comprenant l'essemble des points délimitant la zone d'eau
                GameObject hydroPoints = new GameObject("hydroPoints");

                //items contient l'ensemble des coordonnées des points délimitant la zone d'eau
                JSONArray items = (JSONArray)bigjson["features"][j]["geometry"]["coordinates"][0][0]; //il faut rester en JSONArray sinon il y a un problème pour lire les valeurs
                List<Vector2> myarray = new List<Vector2>();
                for (int i = 0; i < items.Count; i++)
                {
                    float x = items[i][0] - left_down.Item1;
                    float z = items[i][1] - left_down.Item2;
                    //Dans certains cas, l'altitudes des points vaut 9999, il faut donc traiter ce cas à part
                    //Script modifié vers la fin du stage : ces quelques lignes ne sont peut être pas utile
                    //car la valeur de alt est donnée un peu plus loin
                    if (items[i][2]==9999)
                    {
                        alt = 1.3f;
                    }
                    else if (alt <=items[i][2]+1.3f)
                    {
                        alt = items[i][2];//+1.45f;
                    }

                    alt = bigjson["features"][j]["properties"]["z_moyen"]; 
                    myarray.Add(new Vector2(x, z));
                }
                //Triangularisation et création du mesh
                new_mesh.GetComponent<Triangulate>().CreateShapeTriangulate(myarray, false);

                //Quelques modifications pour placer le mesh au bon endroit
                hydroPoints.transform.Rotate(0, -90, 0);
                new_mesh.transform.Rotate(0, -90, 0);
                hydroPoints.transform.position = transform.position;
                hydroPoints.transform.Translate(0, 0, -256);
                new_mesh.transform.Translate(GetComponent<Tile>().position_z * 256, alt, -256 + GetComponent<Tile>().position_x * 256);
                new_mesh.AddComponent<Water>();
                new_mesh.GetComponent<Water>().waterMode = 0; //Simple
                new_mesh.GetComponent<MeshRenderer>().material = waterMat;

                //On supprime les points qui ne sont plus utiles
                Destroy(hydroPoints);

                //On affiche le meshrenderer et on désactive le collider
                new_mesh.GetComponent<MeshRenderer>().enabled = true;
                new_mesh.GetComponent<MeshCollider>().enabled = false;

                //On regroupe les surfaces d'eau dans le même parent
                new_mesh.transform.parent = All_hydro_zones.transform;
            }
        }
    }

    /// <summary>
    /// Génère les hydro linaires (petits cours d'eau, etc...)
    /// Cette fonction n'a pas été testée et présente sûrement des défauts
    /// </summary>
    /// <param name="typename">nom de la couche WFS</param>
    /// <returns></returns>
    public IEnumerator GetPointsHydroLinear(string typename)
    {
        GameObject All_hydro_linear;
        if (GameObject.Find("All_hydro_linear") == null)
        {
            All_hydro_linear = new GameObject();
            All_hydro_linear.name = "All_hydro_linear";
        }
        else
        {
            All_hydro_linear = GameObject.Find("All_hydro_linear");
        }

        string myjson;
        string path = "Assets/Data/Hydro/" + "hydro_linear_" + typename + "_xbot_" + this.GetComponent<Tile>().left_down_x + "_ybot_" + this.GetComponent<Tile>().left_down_y + "_xtop_" + this.GetComponent<Tile>().right_up_x + "_ytop_" + this.GetComponent<Tile>().right_up_y + ".json";
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

        float alt = -1000;
        for (int j = 0; j < bigjson["features"].Count; j++)
        {
            if (GameObject.Find(bigjson["features"][j]["properties"]["id"]) == null)
            {
                Debug.Log(DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2));
                GameObject new_mesh = new GameObject();
                new_mesh.layer = 8;
                new_mesh.AddComponent<Triangulate>();
                new_mesh.name = bigjson["features"][j]["properties"]["id"];
                new_mesh.isStatic = true;


                GameObject hydroPoints = new GameObject("hydroPoints");
                JSONArray items = (JSONArray)bigjson["features"][j]["geometry"]["coordinates"][0][0]; //il faut rester en JSONArray sinon il y a un problème pour lire les valeurs
                List<Vector2> myarray = new List<Vector2>();
                for (int i = 0; i < items.Count; i++)
                {
                    float x = items[i][0] - left_down.Item1;
                    float z = items[i][1] - left_down.Item2;
                    if (items[i][2] == 9999)
                    {
                        alt = 1.3f;
                    }
                    else if (alt <= items[i][2] + 1.3f)
                    {
                        alt = items[i][2];//+1.45f;
                    }
                    alt = bigjson["features"][j]["properties"]["z_moyen"];
                    myarray.Add(new Vector2(x, z));
                }
                new_mesh.GetComponent<Triangulate>().CreateShapeTriangulate(myarray, false);

                hydroPoints.transform.Rotate(0, -90, 0);
                new_mesh.transform.Rotate(0, -90, 0);
                hydroPoints.transform.position = transform.position;
                hydroPoints.transform.Translate(0, 0, -256);
                new_mesh.transform.Translate(GetComponent<Tile>().position_z * 256, alt, -256 + GetComponent<Tile>().position_x * 256);
                new_mesh.AddComponent<Water>();
                new_mesh.GetComponent<Water>().waterMode = 0; //Simple
                new_mesh.GetComponent<MeshRenderer>().material = waterMat;
                Destroy(hydroPoints);
                new_mesh.GetComponent<MeshRenderer>().enabled = true;
                new_mesh.GetComponent<MeshCollider>().enabled = false;
                new_mesh.transform.parent = All_hydro_linear.transform;
            }
        }
    }

}
