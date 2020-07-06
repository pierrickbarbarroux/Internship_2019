using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Cette classe permet de générer les pilonnes électriques. Elle n'est pas utilisée et n'a pas étét bien testée.
/// Mais sur le papier, elle est censé marchée.
/// </summary>
public class GeneratePylon : MonoBehaviour
{

    public GameObject modele_pylon;
    public string typename;
    public string format;

    (float, float) left_down;
    (float, float) right_up;

    Vector3 position_in_scene;
    GameObject goodmnt;





    string path;
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
        GameObject All_pil;
        if (GameObject.Find("All_pil") == null)
        {
            All_pil = new GameObject();
            All_pil.name = "All_pil";
        }
        else
        {
            All_pil = GameObject.Find("All_pil");
        }

        if (maxalt < 0)
        {
            maxalt = Mathf.Max(GetComponent<Tile>().altitudes);
        }

        string myjson;
        path = "Assets/Data/Pilones/pylon_" + typename + "_xbot_" + left_down.Item1 + "_ybot_" + left_down.Item2 + "_xtop_" + right_up.Item1 + "_ytop_" + right_up.Item2 + ".json";
        if (!File.Exists(path))
        {
            string url = DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2);
            StartCoroutine(DataController.WriteDataFile(url, path));
            yield return null;
        }
        Debug.Log(path);
        StreamReader reader = new StreamReader(path);
        myjson = reader.ReadToEnd();
        var bigjson = JSON.Parse(myjson);
        reader.Close();

        int layermask = 1 << 9;
        RaycastHit hit;

        for (int j = 0; j < bigjson["features"].Count; j++)
        {
            if (GameObject.Find(bigjson["features"][j]["properties"]["id"]) == null)
            {
                //Debug.Log(bigjson[j]["reperes"][k]["id"]);
                float x = bigjson["features"][j]["geometry"]["coordinates"][0];
                float z = bigjson["features"][j]["geometry"]["coordinates"][1];

                position_in_scene.x = transform.position.x;
                position_in_scene.z = transform.position.z;
                position_in_scene.x -= z - right_up.Item2;
                position_in_scene.z += x - left_down.Item1;

                position_in_scene.y = maxalt;

                if (Physics.Raycast(position_in_scene, -Vector3.up, out hit, 10000, layermask))
                {
                    GameObject new_borne = Instantiate(modele_pylon, hit.point, Quaternion.identity);
                    new_borne.name = bigjson["features"][j]["properties"]["id"];
                    new_borne.transform.parent = All_pil.transform;
                }
            }
        }
    }
}
