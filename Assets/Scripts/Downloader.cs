using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Classe gérant le téléchargement des fichiers dans le menu principale.
/// </summary>
public class Downloader : MonoBehaviour
{
    [Tooltip("Champs/Input Easting")]
    public TMP_InputField inputfield_long;

    [Tooltip("Champs/Input Northing")]
    public TMP_InputField inputfield_lat;

    [Tooltip("Menu déroulant de la taille")]
    public TMP_InputField inputfield_size;

    //taille de la carte sous forme de 'couche' 
    [HideInInspector]
    public static int Size;
    //nombre de tuile d'une rangée de la carte
    [HideInInspector]
    public static int xSize;

    //Nombre de téléchargement maximal. Utile pour la barre de téléchargement
    float max_loads;

    //Booléen indiquant si la carte a déjà été télchargé ou non
    bool already_download;

    [Tooltip("SLider de la barre de téléchargement. On prendra 'LoadingSlider'")]
    public Slider slider;

    void Start()
    {
        max_loads = 0;
    }

    /// <summary>
    /// On gère le slider dans l'update. On l'active que si on commence un téléchargement. On le désactive
    /// lorsque tous les fichiers sont télchargés. Il se peut que le téléchargement soit fini, mais que certains 
    /// fichiers soient mal passés. Dans ce cas, on attend un peu, puis on lance quand même la map.
    /// </summary>
    void Update()
    {
        //Debug.Log(max_loads);
        //Debug.Log(xSize);
        if (slider.gameObject.activeSelf)
        {
            slider.value = DataController.loading / max_loads;
            if (DataController.loading == max_loads)
            {
                slider.gameObject.SetActive(false);
                DataController.loading = 0;
                Debug.Log("2eme if");
            }
        }
    }

    //Exemple of path_begin : "Assets/Data/Roads/" + "track_"

    /// <summary>
    /// Télécharge le résultat d'une requête WFS dans un fichier.  
    /// </summary>
    /// <param name="path_begin">Le début du path du fichier (ex : "Assets/Data/Roads/" + "roads_")</param>
    /// <param name="typename">Nom de la couche WFS</param>
    /// <param name="format">Format du fichier (on prendra json principalement)</param>
    /// <param name="file_type">La terminaison du fichier ('.json')</param>
    /// <param name="left_down">Coin inférieur gauche de la box</param>
    /// <param name="right_up">Coin supérieur droit de la box</param>
    /// <returns></returns>
    public IEnumerator DownloadJSON(string path_begin, string typename, string format, string file_type, (float, float) left_down, (float, float) right_up)
    {
        string path = path_begin + typename + "_xbot_" + left_down.Item1 + "_ybot_" + left_down.Item2 + "_xtop_" + right_up.Item1 + "_ytop_" + right_up.Item2 + file_type;
        //On ne télécharge le fichier que si ce dernier n'existe pas encore
        if (!File.Exists(path))
        {
            string url = DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2);
            StartCoroutine(DataController.WriteDataFile(url, path));
            //Debug.Log(url);
            yield return null;
        }
        else
        {
            FileInfo fileInfo = new FileInfo(path);
            //Si le fichier est mal télécharger, on le détruit et on recommmence
            if (fileInfo.Length == 0)
            {
                File.Delete(path);
                string url = DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2);
                StartCoroutine(DataController.WriteDataFile(url, path));
                //Debug.Log(url);
                yield return null;
            }
            else
            {
                DataController.loading++;
            }
        }
        //if (typename == "BDGEODESIQUE_BDD_WLD_WGS84G_20190719:site")
        //{
        //    Debug.Log(path);
        //}
    }

    /// <summary>
    /// Télécharge le résultat d'une requête WMS dans un fichier.  
    /// </summary>
    /// <param name="path_begin">Le début du path du fichier (ex : "Assets/Data/Roads/" + "roads_")</param>
    /// <param name="typename">Nom de la couche WMS</param>
    /// <param name="format">Format du fichier (on prendra json principalement)</param>
    /// <param name="file_type">La terminaison du fichier ('.json')</param>
    /// <param name="left_down">Coin inférieur gauche de la box</param>
    /// <param name="right_up">Coin supérieur droit de la box</param>
    /// <returns></returns>
    public IEnumerator DownloadWMS(string path_begin, string typename, string format, string file_type, (float, float) left_down, (float, float) right_up, string height, string width)
    {
        string path = path_begin + typename + "_xbot_" + left_down.Item1 + "_ybot_" + left_down.Item2 + "_xtop_" + right_up.Item1 + "_ytop_" + right_up.Item2 + file_type;
        if (!File.Exists(path))
        {
            string url = DataController.GetWmsRequest(typename, format, height, width, left_down, right_up);
            StartCoroutine(DataController.WriteDataFile(url, path));
            //Debug.Log(url);
            yield return null;
        }
        else
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Length == 0)
            {
                File.Delete(path);
                string url = DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2);
                StartCoroutine(DataController.WriteDataFile(url, path));
                //Debug.Log(url);
                yield return null;
            }
            else
            {
                DataController.loading++;
            }
        }

    }

    /// <summary>
    /// Télécharge tous les fichiers nécessaires à la création de la carte.
    /// Cette méthode est appelée dans l'event system du bouton 'Télécharger la carte'
    /// Version basique.
    /// </summary>
    public void DownloadAll()
    {
        StartCoroutine(DownloadAllCoroutine());
    }

    /// <summary>
    /// Télécharge tous les fichiers nécessaires à la création dela carte.
    /// Version Coroutine.
    /// </summary>
    /// <returns></returns>
    IEnumerator DownloadAllCoroutine()
    {
        ParameterManager.Long = float.Parse(inputfield_long.text);
        ParameterManager.Lat = float.Parse(inputfield_lat.text);
        

        int diff = 256;
        Size = ParameterManager.Size;
        xSize = 2 * Size - 1;
        (float, float) left_down_bas_gauche = (ParameterManager.Long - ((Size - 1) * diff), ParameterManager.Lat - ((Size - 1) * diff));

        //Box de la tuile
        (float, float) left_down;
        (float, float) right_up;

        //On initialise le téléchargement à 0 (pour la barre de téléchargement)
        DataController.loading = 0;
        already_download = false;

        //On active la barre de téléchargement
        slider.gameObject.SetActive(true);
        max_loads = (xSize * xSize * 12 + 2 * (xSize * 4 * 3 + 3 * 3 * 4));


        //Les -3 et +3 viennent des tuiles que l'on rajoute en bordure de la carte. Ces tuiles ne sont là uniquement 
        //pour éviter que le joueur ne se retrouve au bord du vide. Seul le MT et l'ortho ne sont appliqués à es tuiles
        for (int z = -3; z < xSize + 3; z++)
        {
            for (int x = -3; x < xSize + 3; x++)
            {
                //On se place dans la bonne box et on télécharge l'ensemble des fichiers désirés
                left_down = (left_down_bas_gauche.Item1 + (x * diff), left_down_bas_gauche.Item2 + (z * diff));
                right_up = (left_down.Item1 + 256, left_down.Item2 + 256);
                if (z < 0 || z >= xSize || x < 0 || x >= xSize)
                {
                    StartCoroutine(DownloadWMS("Assets/Data/Textures/ortho_", "ORTHOIMAGERY.ORTHOPHOTOS", "image/jpeg", ".jpeg", left_down, right_up, "1280", "1280"));
                    yield return null;

                    StartCoroutine(DownloadWMS("Assets/Data/MNTs/mnt_", "RGEALTI-MNT_PYR-ZIP_FXX_LAMB93_WMS", "image/x-bil;bits=32", ".bil", left_down, right_up, "256", "256"));
                    yield return null;
                }
                else
                {
                    //MNTs
                    StartCoroutine(DownloadWMS("Assets/Data/MNTs/mnt_", "RGEALTI-MNT_PYR-ZIP_FXX_LAMB93_WMS", "image/x-bil;bits=32", ".bil", left_down, right_up, "256", "256"));
                    //string path = "Assets/Data/MNTs/mnt_"+layer+"_xbot_"+ left_down_x + "_ybot_" + left_down_y + "_xtop_" + right_up_x + "_ytop_" + right_up_y + ".bil";
                    yield return null;

                    ////Ortho i.e textures
                    StartCoroutine(DownloadWMS("Assets/Data/Textures/ortho_", "ORTHOIMAGERY.ORTHOPHOTOS", "image/jpeg", ".jpeg", left_down, right_up, "1280", "1280"));
                    ////string path = "Assets/Data/Textures/ortho_ORTHOIMAGERY.ORTHOPHOTOS_xbot_" + left_down_x + "_ybot_" + left_down_y + "_xtop_" + mnt.GetComponent<Tile>().right_up_x + "_ytop_" + mnt.GetComponent<Tile>().right_up_y + ".jpeg";
                    yield return null;

                    ////Batiments
                    StartCoroutine(DownloadJSON("Assets/Data/Batiments/bat_", "BDTOPO_BDD_WLD_WGS84G:bati_indifferencie", "json", ".json", left_down, right_up));
                    yield return null;

                    StartCoroutine(DownloadJSON("Assets/Data/Batiments/bat_", "BDTOPO_BDD_WLD_WGS84G:bati_industriel", "json", ".json", left_down, right_up));

                    yield return null;

                    StartCoroutine(DownloadJSON("Assets/Data/Batiments/bat_", "BDTOPO_BDD_WLD_WGS84G:bati_remarquable", "json", ".json", left_down, right_up));
                    ////string path = "Assets/Data/Batiments/" + "bat_" + typename + "_xbot_" + this.GetComponent<Tile>().left_down_x + "_ybot_" + this.GetComponent<Tile>().left_down_y + "_xtop_" + this.GetComponent<Tile>().right_up_x + "_ytop_" + this.GetComponent<Tile>().right_up_y + ".json";
                    yield return null;

                    ////Forests
                    StartCoroutine(DownloadJSON("Assets/Data/Forests/forest_", "LANDCOVER.FORESTINVENTORY.V2:bdforetv2", "json", ".json", left_down, right_up));
                    ////string path = "Assets/Data/Forests/" + "forest_xbot_" + mnt.GetComponent<Tile>().left_down_x + "_ybot_" + mnt.GetComponent<Tile>().left_down_y + "_xtop_" + mnt.GetComponent<Tile>().right_up_x + "_ytop_" + mnt.GetComponent<Tile>().right_up_y + ".json";
                    yield return null;

                    ////Fields
                    StartCoroutine(DownloadJSON("Assets/Data/Fields/field_", "RPG.2017:parcelles_graphiques", "json", ".json", left_down, right_up));
                    ////string path = "Assets/Data/Fields/" + "field_xbot_" + mnt.GetComponent<Tile>().left_down_x + "_ybot_" + mnt.GetComponent<Tile>().left_down_y + "_xtop_" + mnt.GetComponent<Tile>().right_up_x + "_ytop_" + mnt.GetComponent<Tile>().right_up_y + ".json";
                    yield return null;

                    ////Vegetation
                    StartCoroutine(DownloadJSON("Assets/Data/Vegetation/field_", "BDTOPO_BDD_WLD_WGS84G:zone_vegetation", "json", ".json", left_down, right_up));
                    //string path = "Assets/Data/Vegetation/" + "field_xbot_" + mnt.GetComponent<Tile>().left_down_x + "_ybot_" + mnt.GetComponent<Tile>().left_down_y + "_xtop_" + mnt.GetComponent<Tile>().right_up_x + "_ytop_" + mnt.GetComponent<Tile>().right_up_y + ".json";
                    yield return null;

                    //Roads
                    StartCoroutine(DownloadJSON("Assets/Data/Roads/road_", "BDTOPO_BDD_WLD_WGS84G:route", "json", ".json", left_down, right_up));
                    //string path = "Assets/Data/Roads/" + "road_" + typename + "_xbot_" + this.GetComponent<Tile>().left_down_x + "_ybot_" + this.GetComponent<Tile>().left_down_y + "_xtop_" + this.GetComponent<Tile>().right_up_x + "_ytop_" + this.GetComponent<Tile>().right_up_y + ".json";
                    yield return null;

                    //Tracks
                    StartCoroutine(DownloadJSON("Assets/Data/Roads/track_", "BDTOPO_BDD_WLD_WGS84G:troncon_voie_ferree", "json", ".json", left_down, right_up));
                    //string path = "Assets/Data/Roads/" + "track_" + typename_tracks + "_xbot_" + this.GetComponent<Tile>().left_down_x + "_ybot_" + this.GetComponent<Tile>().left_down_y + "_xtop_" + this.GetComponent<Tile>().right_up_x + "_ytop_" + this.GetComponent<Tile>().right_up_y + ".json";
                    yield return null;

                    //Hydro
                    StartCoroutine(DownloadJSON("Assets/Data/Hydro/hydro_", "BDTOPO_BDD_WLD_WGS84G:surface_eau", "json", ".json", left_down, right_up));
                    //string path = "Assets/Data/Hydro/" + "hydro_" + typename + "_xbot_" + this.GetComponent<Tile>().left_down_x + "_ybot_" + this.GetComponent<Tile>().left_down_y + "_xtop_" + this.GetComponent<Tile>().right_up_x + "_ytop_" + this.GetComponent<Tile>().right_up_y + ".json";
                    yield return null;

                    //Bornes
                    StartCoroutine(DownloadJSON("Assets/Data/Bornes/borne_", "BDGEODESIQUE_BDD_WLD_WGS84G_20190719:site", "json", ".json", left_down, right_up));
                    //string path = "Assets/Data/Bornes/" + "borne_" + typename + "_xbot_" + left_down.Item1 + "_ybot_" + left_down.Item2 + "_xtop_" + right_up.Item1 + "_ytop_" + right_up.Item1 + ".json";

                    yield return null;
                }
            }
        }
        //On attends une petite seconde entre chaque téléchargement d'une tuile
        yield return new WaitForSeconds(1f);
        already_download = true;
    }
}


