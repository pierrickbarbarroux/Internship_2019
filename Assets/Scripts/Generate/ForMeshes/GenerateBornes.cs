using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Cette classe n'est plus utilisée. Elle servait à la génération des bornes à partir d'un fichier
/// Voir la classe GenerateBorne2 pour la génération de borne.
/// </summary>
public class GenerateBornes : MonoBehaviour
{
    (float, float) left_down;
    (float, float) right_up;

    Vector3 position_in_scene;
    GameObject goodmnt;

    //Saint-Mandé
    //658353
    //6860172

    public string typename;
    public string format;

    public GameObject modele_borne;

    string path;

    // Start is called before the first frame update
    void Start()
    {
        path = "";
        position_in_scene = new Vector3();
        goodmnt = new GameObject();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("n"))
        {
            GetPointsBornes(typename);
        }
    }

    private void GetPointsBornes(string typename)
    {
        GameObject All_bornes_points;
        if (GameObject.Find("All_bornes_points") == null)
        {
            All_bornes_points = new GameObject();
            All_bornes_points.name = "All_bornes_points";
        }
        else
        {
            All_bornes_points = GameObject.Find("All_bornes_points");
        }

        string myjson;
        //string path = "Assets/Data/Bornes/" + "borne_" + typename + "_xbot_" + this.GetComponent<Tile>().left_down_x + "_ybot_" + this.GetComponent<Tile>().left_down_y + "_xtop_" + this.GetComponent<Tile>().right_up_x + "_ytop_" + this.GetComponent<Tile>().right_up_y + ".json";
        //if (!File.Exists(path))
        //{
        //    string url = DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2);
        //    StartCoroutine(DataController.WriteDataFile(url, path));
        //    yield return null;
        //}

        string path = "Assets/Data/Bornes/" + "rbf-all" + ".json";
        StreamReader reader = new StreamReader(path);
        myjson = reader.ReadToEnd();
        var bigjson = JSON.Parse(myjson);
        reader.Close();

        Debug.Log(bigjson.Count);
        Debug.Log(bigjson[0].Count);
        Debug.Log(bigjson[0]["commune"]);
        Debug.Log(bigjson[0]["reperes"][0]["description"]);

        GameObject[] mnts = GameObject.FindGameObjectsWithTag("Tile_tag");

        for (int j = 0; j < bigjson.Count; j++)//(int j = 0; j < bigjson["features"].Count; j++)
        {
            if (bigjson[j]["commune"] == "SAINT-MANDE")
            {
                Debug.Log("saint mandé trouvé !!");
                for (int k = 0; k < bigjson[j]["reperes"].Count; k++)
                {
                    if (GameObject.Find(bigjson[j]["reperes"][k]["id"]) == null)//(GameObject.Find(bigjson["features"][j]["properties"]["id"]) == null)
                    {
                        Debug.Log(bigjson[j]["reperes"][k]["id"]);
                        float x = bigjson[j]["reperes"][k]["x"];
                        float y = bigjson[j]["reperes"][k]["z"];
                        float z = bigjson[j]["reperes"][k]["y"];

                        foreach (GameObject mnt in mnts)
                        {
                            if (x >= mnt.GetComponent<Tile>().left_down_x && x <= mnt.GetComponent<Tile>().right_up_x && z >= mnt.GetComponent<Tile>().left_down_y && z <= mnt.GetComponent<Tile>().right_up_y)
                                //(mnt.GetComponent<Tile>().left_down_x<=x && mnt.GetComponent<Tile>().left_down_y<=z && mnt.GetComponent<Tile>().right_up_x>=x && mnt.GetComponent<Tile>().right_up_y>=y)
                            {
                                goodmnt = mnt;
                                Debug.Log(goodmnt.GetComponent<Tile>().position_x);
                                Debug.Log(goodmnt.GetComponent<Tile>().position_z);
                                Debug.Log(DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2));

                                Debug.Log(goodmnt.transform.position.x);
                                Debug.Log(goodmnt.transform.position.z);

                                position_in_scene.x = goodmnt.transform.position.x;
                                position_in_scene.z = goodmnt.transform.position.z;
                                position_in_scene.x -= z - goodmnt.GetComponent<Tile>().right_up_y;
                                position_in_scene.z += x - goodmnt.GetComponent<Tile>().left_down_x;
                                position_in_scene.y = y;
                                GameObject new_borne = Instantiate(modele_borne, position_in_scene, Quaternion.identity);
                                new_borne.name = bigjson[j]["reperes"][k]["id"];//bigjson["features"][j]["properties"]["id"];
                                //new_borne.transform.parent = All_bornes_points.transform;
                            }
                        }
                      

                        
                    }
                }
            }
            
        }

    }
}
