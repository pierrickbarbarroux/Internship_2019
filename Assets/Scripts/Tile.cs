using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Classe regroupant les méthodes et les attributs liés aux tuiles. Avec plus de 99 références, c'est la classe la plus
/// utilisée du projet. Beaucoup des méthodes écrites dans cette classe ne sont pas utilisées dans le projet finale.
/// </summary>
public class Tile : MonoBehaviour
{
    //string key = "j0rmqdk335hqz204nk0awcpn";

    //Le composnat 'Tile' du terrain de référence
    public static Tile the_ref;

    [HideInInspector] //Altitudes du terrain pour le MNT
    public float[] altitudes;

    [Tooltip("position_x désigne la position du terrain par rapport aux autres terrains. Ainsi le terrain dans le coin inférieur gauche (en s'orientant selon le Nord) sera le terrain en position X=0 Z=0")]
    public int position_x;

    [Tooltip("position_z désigne la position du terrain par rapport aux autres terrains. Ainsi le terrain dans le coin inférieur gauche (en s'orientant selon le Nord) sera le terrain en position X=0 Z=0")]
    public int position_z;

    [Tooltip("True si le terrain est le terrain référent, false sinon")]
    public bool is_ref;

    [Tooltip("True si le mesh est une surface de forêt, false sinon")]
    public bool is_forest_mesh;

    [Tooltip("True si le mesh est une surface de végétation, false sinon")]
    public bool is_vege_mesh;

    [Tooltip("True si le mesh est une surface de cimetière, false sinon")]
    public bool is_grave_mesh;

    [Tooltip("True si le mesh est une surface de champs, false sinon")]
    public bool is_field_mesh;

    [Tooltip("True si le mesh est une surface d'eau, false sinon")]
    public bool is_hydro_mesh;

    [Tooltip("Nom de la couche WMS du MNT")]
    public string layer;
    [Tooltip("Longeur de la tuile en mètre")]
    public string height;
    [Tooltip("Largeur de la tuile en mètre")]
    public string width;

    [Tooltip("Easting du coin inférieur gauche (Lambert 93)")]
    public float left_down_x;
    [Tooltip("Northing du coin inférieur gauche (Lambert 93)")]
    public float left_down_y;
    [Tooltip("Easting du coin supérieur droit (Lambert 93)")]
    public float right_up_x;
    [Tooltip("Northing du coin supérieur droit (Lambert 93)")]
    public float right_up_y;

    //Vieux paramètres qui ne servent plus vraiment
    public string tileMatrixset;
    public string tileMatrix;
    public string tileRow;
    public string tileCol;

    [Tooltip("Format de la requête WMS pour le MNT. On prendra 'image/x-bil;bits=32'")]
    public string format;


    //string myURL = "https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.ELEVATIONGRIDCOVERAGE.HIGHRES&TILEMATRIXSET=WGS84G&TILEMATRIX=14&TILEROW=3878&TILECOL=16681&STYLE=normal&FORMAT=image/x-bil;bits=32";

    /// <summary>
    /// Si le terrain en question est le terrain référent, on change ses paramètres pour qu'ils coïncident
    /// avec les paramètres choisis dans le menu principal
    /// </summary>
    void Awake()
    {
        if (is_ref)
        {
            left_down_x = ParameterManager.Long;
            left_down_y = ParameterManager.Lat;
            right_up_x = left_down_x + 256;
            right_up_y = left_down_y + 256;
        }
    }

    //// Start is called before the first frame update
    //void Start()
    //{
    //    //StartCoroutine(GetAlt());
    //}

    /// <summary>
    /// Récupère les points d'altitudes de la tuile. Si le fichier n'existe pas, alors on le télécharge
    /// puis on récupère les points d'altitudes qui seront stockés dans la classe.
    /// </summary>
    /// <returns></returns>
    public IEnumerator GetAlt()
    {
        string path = "Assets/Data/MNTs/mnt_" + layer + "_xbot_" + left_down_x + "_ybot_" + left_down_y + "_xtop_" + right_up_x + "_ytop_" + right_up_y + ".bil";
        if (!File.Exists(path))
        {
            string url = DataController.GetWmsRequest(layer, format, height, width, (left_down_x, left_down_y), (right_up_x, right_up_y));
            StartCoroutine(DataController.WriteDataFile(url, path));
            Debug.Log(url);
            yield return null;
        }
        float[] altitude = DataController.GetMNTFromFile(path, 65536);
        altitudes = altitude;
    }

    /// <summary>
    /// Autre version de GetAlt(). Exactement le même principe, sauf peut être le path.
    /// Cette méthode n'est utilisée que pour des texts.
    /// </summary>
    public void GetAlt2()
    {

        string path = "Assets/Data/MNTs/" + "mnt_xbot_" + left_down_x + "_ybot_" + left_down_y + "_xtop_" + right_up_x + "_ytop_" + right_up_y + ".bil";
        if (!File.Exists(path))
        {
            string url = DataController.GetWmsRequest(layer, format, height, width, (left_down_x, left_down_y), (right_up_x, right_up_y));
            StartCoroutine(DataController.WriteDataFile(url, path));
            Debug.Log(url);
        }
        float[] altitude = DataController.GetMNTFromFile(path, 65536);
        altitudes = altitude;
    }

    /// <summary>
    /// Cette fonction permet de déplacer les tuiles pour qu'elle soit situées au bon endroit dans la scène.
    /// Néanmoins, elle n'est pas utilisée car j'ai ajouté ces lignes directement dans TilesGenerator et GenerateTIles
    /// </summary>
    /// <param name="reference"></param>
    void MoveTile(Tile reference)
    {
        if (this.is_ref)
        {
            this.position_x = 0;
            this.position_z = 0;
        }
        else
        {
            this.position_z = int.Parse(this.tileRow) - int.Parse(reference.tileRow);
            this.position_x = int.Parse(this.tileCol) - int.Parse(reference.tileCol);

        }

    }




     /// <summary>
     /// Renvoie un tableau contenant les tuiles voisines (clones) de cette tuile
     /// Cette fonction ne récupère pas les objets donc, mais en crée des clones, ce n'est pas optimal
     /// Mais si le nombre de tuiles dans la scène est très grand, cette méthode est peut être plus adapté.
     /// 
     /// Cette fonction n'est pas utilisé pour les terrains, mais seulement pour les mesh
     /// 
     /// </summary>
     /// <returns>Tableau contenant des les voisins de cette tuile </returns>
    public Dictionary<string, Tile> TilesAround()
    {
        Tile tile = GetComponent<Tile>();
        Dictionary<string, Tile> ret = new Dictionary<string, Tile>();
        int i = 0;
        int xdiff;
        int zdiff;

        GameObject[] all_mesh = GameObject.FindGameObjectsWithTag("Tile_tag");
        foreach (GameObject mesh in all_mesh)
        {
            //Debug.Log("on traite position x:"+ mesh.GetComponent<Tile>().position_x + "position z" + mesh.GetComponent<Tile>().position_z);
            Tile neighboor = mesh.GetComponent<Tile>();
            xdiff = neighboor.position_x - tile.position_x;
            zdiff = neighboor.position_z - tile.position_z;

            if (xdiff == -1 && zdiff == 0)
            {
                ret.Add("down", neighboor);
                i++;
            }
            if (xdiff == 0 && zdiff == -1)
            {
                ret.Add("left", neighboor);
                i++;
            }
            if (xdiff == 0 && zdiff == 1)
            {
                ret.Add("right", neighboor);
                i++;
            }
            if (xdiff == 1 && zdiff == 0)
            {
                ret.Add("up", neighboor);
                i++;
            }

        }
        return ret;
    }

    /// <summary>
    /// Méthode permettant de lisser le contour des tuiles. Cette méthode n'est utilisée que pour les mesh.
    /// Elle n'est donc pas utiliser pour la version final du projet.
    /// </summary>
    public void Smooth()
    {
        Debug.Log("smooth");
        Dictionary<string, Tile> tiles_around = TilesAround();
        Tile tile;
        Debug.Log("On examine la tuile de x = " + position_x + "   et de z = " + position_z);
        int size = GetComponent<GenerateMesh>().xSize;
        Debug.Log("On applique smooth pour x : " + position_x + " et z : " + position_z);
        for (int i = 0; i < size; i++)
        {
            if (tiles_around.TryGetValue("left", out tile)) // up devient left
            {
                //Debug.Log("la tuile x = "+ tile.position_x+"  et z = " + tile.position_z + "se trouve UP de la tuile précédente");
                float moy = (tile.altitudes[size - 1 + i * size] + altitudes[i * size]) / 2;

                //Debug.Log(moy);
                tile.altitudes[size - 1 + i * size] = moy;
                altitudes[i * size] = moy;
            }
            if (tiles_around.TryGetValue("up", out tile)) // left devient up
            {
                //Debug.Log("la tuile x = " + tile.position_x + "  et z = " + tile.position_z + "se trouve LEFT de la tuile précédente");
                float moy2 = (tile.altitudes[(size * size) - 1 - (size - 1) + i] + altitudes[i]) / 2;
                tile.altitudes[(size * size) - 1 - (size - 1) + i] = moy2;
                altitudes[i] = moy2;
            }
            if (tiles_around.TryGetValue("down", out tile)) // right devient down
            {
                //Debug.Log("la tuile x = " + tile.position_x + "  et z = " + tile.position_z + "se trouve RIGHT de la tuile précédente");
                float moy3 = (tile.altitudes[i] + altitudes[(size * size) - 1 - (size - 1) + i]) / 2;
                tile.altitudes[i] = moy3;
                altitudes[(size * size) - 1 - (size - 1) + i] = moy3;
            }
            if (tiles_around.TryGetValue("right", out tile)) //down devient right 
            {
                //Debug.Log("la tuile x = " + tile.position_x + "  et z = " + tile.position_z + "se trouve DOWN de la tuile précédente");

                float moy4 = (tile.altitudes[i * size] + altitudes[(size - 1) + i * size]) / 2;
                tile.altitudes[i * size] = moy4;
                altitudes[(size - 1) + i * size] = moy4;
            }

        }
    }

    /// <summary>
    /// Autre version de Smooth(). Dans le doute, je n'utilise pas cette version dans le projet final 
    /// car on utilise des terrains et non des meshes
    /// </summary>
    public void Smooth2()
    {
        Dictionary<string, Tile> tiles_around = TilesAround();
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Tile neighboor;
        int size = GetComponent<GenerateMesh>().xSize;
        if (tiles_around.TryGetValue("left", out neighboor))
        {
            Mesh neighboor_mesh = neighboor.gameObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] neighboor_vertices = neighboor_mesh.vertices;
            int plus_grand_nb_vertices;
            int plus_petit_nb_vertices;
            int rapport;
            if (neighboor_vertices.Length <= vertices.Length)
            {
                plus_grand_nb_vertices = (int)Mathf.Sqrt((mesh.vertices.Length));
                plus_petit_nb_vertices = (int)Mathf.Sqrt((neighboor_mesh.vertices.Length));
                rapport = (plus_grand_nb_vertices - 1) / (plus_petit_nb_vertices - 1);
                int neighboor_vertices_index = 0;
                for (int i = 0; i < plus_grand_nb_vertices; i++)
                {
                    vertices[i].y = neighboor_mesh.vertices[neighboor_vertices.Length - plus_petit_nb_vertices + neighboor_vertices_index].y;
                    if (i % rapport == 0)
                    {
                        neighboor_vertices_index += 1;
                    }
                    //else
                    //{
                    //    vertices[i].x = vertices[neighboor_vertices_index * rapport].x;
                    //    vertices[i].z = vertices[neighboor_vertices_index * rapport].z;
                    //}
                }
            }
        }
        if (tiles_around.TryGetValue("up", out neighboor))
        {
            Mesh neighboor_mesh = neighboor.gameObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] neighboor_vertices = neighboor_mesh.vertices;

            int plus_grand_nb_vertices;
            int plus_petit_nb_vertices;
            int rapport;
            if (neighboor_mesh.vertices.Length <= mesh.vertices.Length)
            {
                plus_grand_nb_vertices = (int)Mathf.Sqrt((mesh.vertices.Length));
                plus_petit_nb_vertices = (int)Mathf.Sqrt((neighboor_mesh.vertices.Length));
                rapport = (plus_grand_nb_vertices - 1) / (plus_petit_nb_vertices - 1);
                int neighboor_vertices_index = 0;
                //Debug.Log("neighboor_vertices.Length : " + neighboor_vertices.Length);
                //Debug.Log(rapport);
                for (int i = 0; i < plus_grand_nb_vertices; i++)
                {
                    Debug.Log("plus_petit_nb_vertices + neighboor_vertices_index* plus_petit_nb_vertices : " + (plus_petit_nb_vertices + neighboor_vertices_index * plus_petit_nb_vertices));

                    vertices[i * plus_grand_nb_vertices].y = neighboor_vertices[plus_petit_nb_vertices - 1 + neighboor_vertices_index * plus_petit_nb_vertices].y;
                    if (i % rapport == 0)
                    {
                        neighboor_vertices_index += 1;
                    }
                    //else
                    //{
                    //    vertices[i * plus_grand_nb_vertices].x = vertices[neighboor_vertices_index * rapport * plus_grand_nb_vertices].x;
                    //    vertices[i * plus_grand_nb_vertices].z = vertices[neighboor_vertices_index * rapport * plus_grand_nb_vertices].z;
                    //}
                }
            }
        }
        if (tiles_around.TryGetValue("right", out neighboor))
        {
            Mesh neighboor_mesh = neighboor.gameObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] neighboor_vertices = neighboor_mesh.vertices;

            int plus_grand_nb_vertices;
            int plus_petit_nb_vertices;
            int rapport;
            if (neighboor_mesh.vertices.Length <= mesh.vertices.Length)
            {
                plus_grand_nb_vertices = (int)Mathf.Sqrt((mesh.vertices.Length));
                plus_petit_nb_vertices = (int)Mathf.Sqrt((neighboor_mesh.vertices.Length));
                rapport = (plus_grand_nb_vertices - 1) / (plus_petit_nb_vertices - 1);
                int neighboor_vertices_index = 0;
                for (int i = 0; i < plus_grand_nb_vertices; i++)
                {
                    vertices[mesh.vertices.Length - plus_grand_nb_vertices + i].y = neighboor_vertices[neighboor_vertices_index].y;
                    if (i % rapport == 0)
                    {
                        neighboor_vertices_index += 1;
                    }
                    //else
                    //{
                    //    vertices[mesh.vertices.Length - plus_grand_nb_vertices + i].x = vertices[mesh.vertices.Length - plus_grand_nb_vertices + neighboor_vertices_index * rapport].x;
                    //    vertices[mesh.vertices.Length - plus_grand_nb_vertices + i].z = vertices[mesh.vertices.Length - plus_grand_nb_vertices + neighboor_vertices_index * rapport].z;
                    //}
                }
            }
        }
        if (tiles_around.TryGetValue("down", out neighboor))
        {
            Mesh neighboor_mesh = neighboor.gameObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] neighboor_vertices = neighboor_mesh.vertices;
            int plus_grand_nb_vertices;
            int plus_petit_nb_vertices;
            int rapport;
            if (neighboor_mesh.vertices.Length <= mesh.vertices.Length)
            {
                plus_grand_nb_vertices = (int)Mathf.Sqrt((mesh.vertices.Length));
                plus_petit_nb_vertices = (int)Mathf.Sqrt((neighboor_mesh.vertices.Length));
                rapport = (plus_grand_nb_vertices - 1) / (plus_petit_nb_vertices - 1);
                int neighboor_vertices_index = 0;
                for (int i = 0; i < plus_grand_nb_vertices; i++)
                {
                    vertices[plus_grand_nb_vertices - 1 + i * plus_grand_nb_vertices].y = neighboor_vertices[neighboor_vertices_index * plus_petit_nb_vertices].y;
                    if (i % rapport == 0)
                    {
                        neighboor_vertices_index += 1;
                    }
                    //else
                    //{
                    //    vertices[plus_grand_nb_vertices - 1 + i * plus_grand_nb_vertices].x = vertices[plus_grand_nb_vertices - 1 + neighboor_vertices_index * rapport * plus_grand_nb_vertices].x;
                    //    vertices[plus_grand_nb_vertices - 1 + i * plus_grand_nb_vertices].z = vertices[plus_grand_nb_vertices - 1 + neighboor_vertices_index * rapport * plus_grand_nb_vertices].z;
                    //}
                }
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Ajoute l'ortho au MNT.Si le fichier n'est pas télécharger, le télécharge.
    /// </summary>
    /// <param name="mnt">Terrain que l'on veut modifier</param>
    /// <param name="mat">Material de base appliqué au MNT auqeul on rajoute la texture</param>
    /// <returns></returns>
    public IEnumerator UpdateSkinTerrain(GameObject mnt, Material mat)
    {
        string path = "Assets/Data/Textures/ortho_ORTHOIMAGERY.ORTHOPHOTOS_xbot_" + left_down_x + "_ybot_" + left_down_y + "_xtop_" + mnt.GetComponent<Tile>().right_up_x + "_ytop_" + mnt.GetComponent<Tile>().right_up_y + ".jpeg";
        if (!File.Exists(path))
        {
            Debug.Log(DataController.GetWmsRequest("ORTHOIMAGERY.ORTHOPHOTOS", "image/jpeg", "1280", "1280", (left_down_x, left_down_y), (right_up_x, right_up_y)));
            string url = DataController.GetWmsRequest("ORTHOIMAGERY.ORTHOPHOTOS", "image/jpeg", "1280", "1280", (left_down_x, left_down_y), (right_up_x, right_up_y));
            StartCoroutine(DataController.WriteDataFile(url, path));
            yield return null;
        }
        Texture2D res = new Texture2D(2560, 2560);
        res.LoadImage(File.ReadAllBytes(path));
        res = GenerateMesh.rotateTexture(res, false);
        Material newmat = new Material(mat);
        mnt.GetComponent<Terrain>().materialTemplate = newmat;
        mnt.GetComponent<Terrain>().materialTemplate.mainTexture = res;
        mnt.GetComponent<Terrain>().materialTemplate.name = "Texture_Ortho_";
    }

}
