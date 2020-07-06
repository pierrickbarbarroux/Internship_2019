using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;

/// <summary>
/// Cette classe est bien utilisée dans le projet final.
/// Ce script doit être placé sur le terrain référent et sera ajouté automatiquement sur chaque autre terrain lors de la génération des terrains.
/// Cette classe génère les champs.
/// </summary>
public class GenerateField : MonoBehaviour
{
    [Tooltip("Modèles des plantation pour les champs.")]
    public GameObject[] myFields;

    [Tooltip("Nom de la couche WFS.")]
    public string typename;

    [Tooltip("Format du fichier télécharger. On choisira 'json'.")]
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

    /// <summary>
    /// Récupère les points délimitant les zones de végétation et crée les mesh correspondant.
    /// </summary>
    /// <param name="mnt">MNT sur lequel on génère les zone de végétation</param>
    /// <returns>Retourne rien</returns>
    public IEnumerator GetPointsField(GameObject mnt)
    {
        //Pour plus de lisibilité dans la scène lors du run, on regroupe tous les zones de champs dans un seul objet
        GameObject All_field_zones;
        if (GameObject.Find("All_field_zones") == null)
        {
            All_field_zones = new GameObject();
            All_field_zones.name = "All_field_zones";
        }
        else
        {
            All_field_zones = GameObject.Find("All_field_zones");
        }

        float alt = Mathf.Max(mnt.GetComponent<Tile>().altitudes);
        string myjson;
        string path = "Assets/Data/Fields/field_"+typename+"_xbot_" + mnt.GetComponent<Tile>().left_down_x + "_ybot_" + mnt.GetComponent<Tile>().left_down_y + "_xtop_" + mnt.GetComponent<Tile>().right_up_x + "_ytop_" + mnt.GetComponent<Tile>().right_up_y + ".json";
        
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

        for (int j = 0; j < bigjson["features"].Count; j++)
        {
            if (GameObject.Find(bigjson["features"][j]["id"]) == null)
            {
                GameObject new_mesh = new GameObject();
                new_mesh.name = bigjson["features"][j]["id"];
                new_mesh.tag = "Field_tag";
                new_mesh.layer = 8; //Il n'y a plus de layer dans le projet unity. Cette ligne est obsolète

                //J'avais ajouté un composant Tile aux mesh délimitant les zones de champs.
                //Mais ce n'est pas vraiment utile, si j'ai le temps, je cleanerais ça proprement
                //Pour l'instant je préfère laisser 
                new_mesh.AddComponent<Tile>();
                new_mesh.GetComponent<Tile>().is_ref = false;
                new_mesh.GetComponent<Tile>().is_field_mesh = true;
                new_mesh.AddComponent<Triangulate>();

                //On crée un objet comprenant l'essemble des points délimitant la zone de champs
                GameObject fieldPoints = new GameObject("fieldPoints");

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
                new_mesh.GetComponent<Triangulate>().CreateShapeTriangulate(myarray, true); // besoin de mettre true pour reverse le mesh

                //Quelques modifications pour placer le mesh au bon endroit
                fieldPoints.transform.Rotate(0, -90, 0);
                new_mesh.transform.Rotate(0, -90, 0);
                fieldPoints.transform.position = mnt.transform.position;
                fieldPoints.transform.Translate(0, 0, -256);
                new_mesh.transform.Translate(mnt.GetComponent<Tile>().position_z * 256, alt + 20, -256 + mnt.GetComponent<Tile>().position_x * 256);

                //On fait de la place
                Destroy(fieldPoints);

                //On n'a pas besoin d'afficher le mesh, on veut juste le collider 
                new_mesh.GetComponent<MeshRenderer>().enabled = false;

                //?
                new_mesh.layer = 11;

                //Pour plus de clarté...
                new_mesh.transform.parent = All_field_zones.transform;
            }
        }

    }

    /// <summary>
    /// Génère les plantations dans les zones de champs.
    /// Les plantations sont alignées.
    /// On utilisera principalement des raycast pour placer nos modèles.
    /// </summary>
    /// <param name="crops">liste des modèles des plantations</param>
    /// <returns>Ne retourne rien</returns>
    public static IEnumerator SpawnField(GameObject[] crops)
    {
        //Pour plus de lisibilité dans la scène lors du run, on regroupe toutes les plantations dans un même objet
        GameObject All_fields;
        if (GameObject.Find("All_fields") == null)
        {
            All_fields = new GameObject();
            All_fields.name = "All_fields";
        }
        else
        {
            All_fields = GameObject.Find("All_fields");
        }

        RaycastHit hit;
        RaycastHit hit2;
        RaycastHit hit3;
        Vector3 new_field_pos;
        Vector3 start;
        int layerMask = 1 << 9; //layer du MNT
        int layerMask_Field = 1 << 11; //layer des champs (field)
        GameObject[] tile_myfields = GameObject.FindGameObjectsWithTag("Field_tag");

        /** On va utiliser plusieurs raycast pour avoir un résultat correct.
         */
        foreach (GameObject field in tile_myfields)
        {
            //On se place au centre du mesh (légèrement en-dessous)
            start = field.GetComponent<MeshRenderer>().bounds.center;
            start -= new Vector3(0, 1, 0);

            /** On génère un certain nombre de raycast (ici 50*70)
             *  On part du mesh (situé très haut, au-dessus du terrain)
             *  On tire un raycast vers le terrain, si on touche bien un terrain, on retire un raycast vers le haut
             *  pour vérifier si on touche bien le mesh field. Puis on retire un raycast vers le terrain. L'impact nous
             *  donne la position de la plantation.
             */
            for (int i = -25; i < 25; i++)
            {
                for (int j = -35; j < 35; j++)
                {
                    new_field_pos = start + new Vector3(i * 6, 0, j * 2f);
                    if (Physics.Raycast(new_field_pos, -Vector3.up, out hit, 10000, layerMask))
                    {
                        if (hit.transform.gameObject.GetComponent<Tile>() != null)
                        {
                            if (hit.transform.gameObject.tag == "Tile_tag" || hit.transform.gameObject.tag == "Terrain_tag")
                            {
                                if (Physics.Raycast(hit.point, Vector3.up, out hit2, 10000, layerMask_Field))
                                {
                                    if (Physics.Raycast(hit2.point, -Vector3.up, out hit3, 10000, layerMask))
                                    {
                                        GameObject culture = Instantiate(crops[Random.Range(0, crops.Length - 1)], hit3.point + new Vector3(0, 0.5f, 0), Quaternion.identity, All_fields.transform);
                                        culture.isStatic = true; //Gain de performance avec les objets static
                                    }
                                }
                            }
                        }
                    }
                }
            }
            yield return null;
        }
    }

}
