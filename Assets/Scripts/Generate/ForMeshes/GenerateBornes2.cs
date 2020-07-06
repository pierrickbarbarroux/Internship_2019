using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Ce script doit être attaché au terrain référent et est ajouté par la suite aux autres terrains par copie
/// Cette classe génère les bornes géodésique
/// </summary>
public class GenerateBornes2 : MonoBehaviour
{
    [Tooltip("Prefab de la borne telle qu'elle apparaitra dans le jeu")]
    public GameObject modele_borne;

    [Tooltip("Nom de la couche WFS")]
    public string typename;
    [Tooltip("Format du fichier téléchargé. On choisira 'json'.")]
    public string format;

    /** Contour (box) en Lambert 93 de la tuile.
     *  left_down correspond au coin inférieur gauche, right_up au coin supérieur droit.
     */
    (float, float) left_down;
    (float, float) right_up;

    /** Position de la borne dans la scène
     * On ne place qu'une seule borne dans la scène : celle que l'on sélectionne dans le menu principal
     */
    Vector3 position_in_scene;

    //Altitude maximale de la tuile
    float maxalt;


    // Start is called before the first frame update
    void Start()
    {
        left_down = (this.GetComponent<Tile>().left_down_x, this.GetComponent<Tile>().left_down_y);
        right_up = (this.GetComponent<Tile>().right_up_x, this.GetComponent<Tile>().right_up_y);
        maxalt = -100;
    }

    public IEnumerator GetGeodesiePoints()
    {
        //Pour plus de lisibilité dans la scène lors du run, on regroupe toutes les bornes dans un objet 
        //Cela ne sert pas à grand chose vu qu'on a finalement décidé de ne placer qu'une seule borne...
        GameObject All_geo;
        if (GameObject.Find("All_geo") == null)
        {
            All_geo = new GameObject();
            All_geo.name = "All_geo";
        }
        else
        {
            All_geo = GameObject.Find("All_geo");
        }

        if (maxalt < 0)
        {
            maxalt = Mathf.Max(GetComponent<Tile>().altitudes);
        }

        string myjson;
        //Téléchargement du fichier json contenant les donneés relative aux bornes de la tuile.
        //Le fichier est téléchargé uniquement si le fichier n'existe pas.
        string path = "Assets/Data/Bornes/borne_" + typename + "_xbot_" + left_down.Item1 + "_ybot_" + left_down.Item2 + "_xtop_" + right_up.Item1 + "_ytop_" + right_up.Item2 + ".json";
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

        RaycastHit hit;
        for (int j = 0; j < bigjson["features"].Count; j++)
        {
            //Debug.Log(DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2));
            if (GameObject.Find(bigjson["features"][j]["properties"]["id"]) == null && int.Parse(bigjson["features"][j]["properties"]["id"]) == ParameterManager.BorneNumber)
            {
                float x = bigjson["features"][j]["geometry"]["coordinates"][0];
                float z = bigjson["features"][j]["geometry"]["coordinates"][1];

                position_in_scene.x = transform.position.x;
                position_in_scene.z = transform.position.z;
                position_in_scene.x -= z - right_up.Item2;
                position_in_scene.z += x - left_down.Item1;

                position_in_scene.y = maxalt;

                //Raycast pour placer la borne au niveau du sol
                //(en tout cas, dès que le raycast touche un objet, on place la borne à l'endroit de l'impact)
                if (Physics.Raycast(position_in_scene, -Vector3.up, out hit))
                {
                    GameObject new_borne = Instantiate(modele_borne, hit.point, Quaternion.identity);
                    new_borne.name = bigjson["features"][j]["properties"]["id"];
                    new_borne.transform.parent = All_geo.transform;
                }
            }
        }
    }
}
