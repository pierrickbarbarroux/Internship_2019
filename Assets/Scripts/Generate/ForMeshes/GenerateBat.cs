using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// Ce script doit être placé sur le terrain référent et sera ajouté automatiquement sur chaque autre terrain.
/// Cette classe génère les bâtiments.
/// </summary>
public class GenerateBat : MonoBehaviour
{
    //http://wxs.ign.fr/bmv1bzgge2e6zl1i3bj4qr2q/geoportail/wfs?SERVICE=WFS&VERSION=1.1.0&REQUEST=GetFeature&TYPENAME=BDTOPO_BDD_WLD_WGS84G:bati_remarquable&SRSNAME=EPSG:2154&BBOX=650769.6754608535,6862000.895466741,651281.6754608535,6862512.895466741,EPSG:2154&STARTINDEX=0&MAXFEATURES=1000
    //http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wfs?SERVICE=WFS&VERSION=1.1.0&REQUEST=GetFeature&TYPENAME=BDTOPO_BDD_WLD_WGS84G:bati_remarquable&SRSNAME=EPSG:2154&BBOX=930937,6258510,931193,6258766,EPSG:2154&STARTINDEX=0&MAXFEATURES=1000&OUTPUTFORMAT=json

    [Tooltip("BDTOPO_BDD_WLD_WGS84G:bati_indifferencie")]
    public string typename1; //BDTOPO_V3_BETA:batiment.title  BDTOPO_BDD_WLD_WGS84G:bati_indifferencie

    [Tooltip("BDTOPO_BDD_WLD_WGS84G:bati_industriel")]
    public string typename2; //BDTOPO_BDD_WLD_WGS84G:bati_industriel

    [Tooltip("BDTOPO_BDD_WLD_WGS84G:bati_remarquable")]
    public string typename3; //BDTOPO_BDD_WLD_WGS84G:bati_remarquable

    [HideInInspector]
    public string typename4; //BDTOPO_BDD_WLD_WGS84G:terrain_sport

    [Tooltip("Format du fichier télécharger. On utilisera 'json'.")]
    public string format;

    [Tooltip("Matériel des toits des bâtiments indifférenciés (habitations,...).")]
    public Material roofMatIndif;

    [Tooltip("Matériel des bâtiments indifférenciés (habitations,...).")]
    public Material batMatIndif;

    [Tooltip("Matériel des toits des bâtiments industriels.")]
    public Material roofMatIndus;

    [Tooltip("Matériel des bâtiments industriels.")]
    public Material batMatIndus;

    [Tooltip("Matériel des toits des bâtiments remarquables.")]
    public Material roofMatAutre;

    [Tooltip("Matériel des bâtiments remarquables.")]
    public Material batMatAutre;

    [HideInInspector]
    public Material roofMatSport;
    [HideInInspector]
    public Material batMatSport;

    /** Contour (box) en Lambert 93 de la tuile.
     *  left_down correspond au coin inférieur gauche, right_up au coin supérieur droit.
     */
    (float, float) left_down;
    (float, float) right_up;

    // Start is called before the first frame update
    void Start()
    {
        //Initialisation de la box.
        left_down = (this.GetComponent<Tile>().left_down_x, this.GetComponent<Tile>().left_down_y);
        right_up = (this.GetComponent<Tile>().right_up_x, this.GetComponent<Tile>().right_up_y);
    }

    // Update is called once per frame
    void Update()
    {
        //Lors des tests, il était préférable de lancer la génération des bâtiment avec une touche du clavier
        //Dans la version finale, cela se fait automatiquement au lancement.
        //Donc ce bout de code est obsolète.
        if (Input.GetKeyDown("b"))
        {
            StartCoroutine(GetPointsBat(typename1));
            StartCoroutine(GetPointsBat(typename2));
            StartCoroutine(GetPointsBat(typename3));
            //StartCoroutine(GetPointsBat(typename4));
        }
        //if (Input.GetKeyDown("c"))
        //{
        //    Clean();
        //}
    }

    //Bug : léger décalage
    /// <summary>
    /// Récupère les points des bâtiments et génère les bâtiments et les toits.
    /// </summary>
    /// <param name="typename">Nom de la couche WFS</param>
    /// <returns>Ne retourne rien</returns>
    public IEnumerator GetPointsBat(string typename)
    {
        //Pour plus de lisibilité dans la scène lors du run, on regroupe tous les bâtiments 
        //dans un objet All_bats
        GameObject All_bats;
        if (GameObject.Find("All_bats") == null)
        {
            All_bats = new GameObject();
            All_bats.name = "All_bats";
        }
        else
        {
            All_bats = GameObject.Find("All_bats");
        }

        GameObject[] mesh_MNTs = GameObject.FindGameObjectsWithTag("Tile_tag");
        foreach (var item in mesh_MNTs)
        {
            item.GetComponent<MeshCollider>().enabled = true;
        }
       
        string myjson;
        //Téléchargement du fichier json contenant les points délimitant les bâtiments.
        //Le fichier est téléchargé uniquement si le fichier n'existe pas déjà.
        string path = "Assets/Data/Batiments/" + "bat_"+typename+"_xbot_" + this.GetComponent<Tile>().left_down_x + "_ybot_" + this.GetComponent<Tile>().left_down_y + "_xtop_" + this.GetComponent<Tile>().right_up_x + "_ytop_" + this.GetComponent<Tile>().right_up_y + ".json";
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

        float y_de_secours=1;
        for (int j = 0; j < bigjson["features"].Count; j++)
        {
            if (GameObject.Find(bigjson["features"][j]["id"]) == null )
            {
                //Création du LOD pour le bâtiment et pour le toit
                GameObject bat_lod_parent = new GameObject();
                bat_lod_parent.name = "LOD_bat_"+bigjson["features"][j]["id"];
                GameObject toit_lod_parent = new GameObject();
                toit_lod_parent.name = "LOD_toit_"+bigjson["features"][j]["id"];

                //Objet regroupant un bâtiment et son toit (pour plus de lisibilité dans la scène...)
                GameObject ens_toit_bat = new GameObject();
                ens_toit_bat.name = "Bat_toit" + bigjson["features"][j]["id"];

                //Création du toit et du bâtiment
                GameObject new_roof = new GameObject();
                GameObject new_bat = new GameObject();
                new_roof.name = "Toit de " + bigjson["features"][j]["id"];
                new_bat.name = bigjson["features"][j]["id"];
                new_roof.AddComponent<Triangulate>();
                new_bat.AddComponent<Triangulate>();

                //new_roof.AddComponent<RendererTimer>();
                //new_bat.AddComponent<RendererTimer>();

                //Les objets static demandent moins de puissance de calcul pour l'affichage
                new_roof.isStatic = true;
                new_bat.isStatic = true;

                //Contiendra l'ensemble des points définisant le bâtiment
                GameObject batPoints = new GameObject();

                //Items = ensembles des points d'un bâtiments
                JSONArray items = (JSONArray)bigjson["features"][j]["geometry"]["coordinates"][0][0]; //il faut rester en JSONArray sinon il y a un problème pour lire les valeurs
                List<Vector2> myarray = new List<Vector2>();
                
                //Coordonnées d'un point
                float x;
                float y;
                float z;
                for (int i = 0; i < items.Count; i++)
                {
                    //On crée une sphère pour un point, mais on aurait pu prendre un Empty Object. 
                    //La seule chose qui nous intéresse ici est le transform. 
                    //Mais lors des tests, travailler avec des sphères était plus commode.
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    x = (items[i][0] - left_down.Item1);
                    if (items[i][2]!=9999) //lorsque l'altitude est inconnue, la valeur par défaut est 9999
                    {
                        y = items[i][2];
                        y_de_secours = y; 
                    }
                    else 
                    {
                        y = y_de_secours;
                    }
                    z = (items[i][1] - left_down.Item2);
                    myarray.Add(new Vector2(x, z));

                    //
                    if (GetComponent<GenerateMesh>() != null)
                    {
                        sphere.transform.position = new Vector3(x, y, z);
                    }
                    else if (GetComponent<TerrainGenerator>() != null)
                    {
                        sphere.transform.position = new Vector3(x, y - GetComponent<TerrainGenerator>().minalt, z);
                    }
                    sphere.transform.parent = batPoints.transform; //batpoints sera détruit par la suite, donc la sphère aussi
                    //Destroy(sphere);
                }
                //On génère les surfaces du toit et du bâtiment. Celle du bâtiment sera extrudée plus tard.
                new_roof.GetComponent<Triangulate>().CreateShapeTriangulate(myarray,false);
                new_bat.GetComponent<Triangulate>().CreateShapeTriangulate(myarray,false);

                //Petites modifications pour que les bâtiments dans le bon sens et au bon endroit
                new_roof.transform.Rotate(0, -90, 0);
                new_bat.transform.Rotate(0, -90, 0);
                batPoints.transform.Rotate(0, -90, 0);
                batPoints.transform.position = transform.position;
                if (GetComponent<GenerateMesh>()!=null)
                {
                    batPoints.transform.Translate(0, 0, -GetComponent<GenerateMesh>().xSize);
                }
                else if (GetComponent<TerrainGenerator>() != null)
                {
                    batPoints.transform.Translate(0, 0, -GetComponent<TerrainGenerator>().width);
                }

                //Necessaire pour la recherche d'un min et d'un max
                float hauteur_bat = 1;
                float alt_sol = 500000;

                //Raycast pour avoir la bonne hauteur du bâtiment
                RaycastHit hit;
                foreach (Transform child in batPoints.transform)
                {
                    if (Physics.Raycast(child.transform.position, -Vector3.up, out hit))
                    {
                        if (hit.distance != 9999)
                        {
                            if (hauteur_bat <= hit.distance)
                            {
                                hauteur_bat = hit.distance;
                            }
                        }
                        
                        if (alt_sol >= hit.point.y)
                        {
                            alt_sol = hit.point.y;
                        }
                    }
                }
                //On place le bâtiment et le toit selon leur altitude et on extrude le bâtiment
                new_roof.transform.Translate(GetComponent<Tile>().position_z * 256, alt_sol + hauteur_bat + 0.001f, -256 + GetComponent<Tile>().position_x * 256);
                new_bat.transform.Translate(GetComponent<Tile>().position_z * 256, alt_sol, -256 + GetComponent<Tile>().position_x * 256);
                MeshExtrusion.Edge[] precomEdges = MeshExtrusion.BuildManifoldEdges(new_bat.GetComponent<MeshFilter>().sharedMesh);
                Matrix4x4[] sections = new Matrix4x4[3];
                sections[0] = transform.worldToLocalMatrix * Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
                sections[1] = transform.worldToLocalMatrix * Matrix4x4.TRS(transform.position + hauteur_bat / 2 * Vector3.up, Quaternion.identity, Vector3.one);
                sections[2] = transform.worldToLocalMatrix * Matrix4x4.TRS(transform.position + hauteur_bat * Vector3.up, Quaternion.identity, Vector3.one);
                MeshExtrusion.ExtrudeMesh(new_bat.GetComponent<MeshFilter>().sharedMesh, new_bat.GetComponent<MeshFilter>().mesh, sections, precomEdges, true);

                //Application des materials selon la couche WFS choisie
                if (typename==typename1)
                {
                    new_roof.GetComponent<MeshRenderer>().material = roofMatIndif;
                    new_bat.GetComponent<MeshRenderer>().material = batMatIndif;
                }
                else if (typename==typename2)
                {
                    new_roof.GetComponent<MeshRenderer>().material = roofMatIndus;
                    new_bat.GetComponent<MeshRenderer>().material = batMatIndus;
                }
                else if (typename==typename3)
                {
                    new_roof.GetComponent<MeshRenderer>().material = roofMatAutre;
                    new_bat.GetComponent<MeshRenderer>().material = batMatAutre;
                }
                else if (typename==typename4)
                {
                    new_roof.GetComponent<MeshRenderer>().material = roofMatSport;
                    new_bat.GetComponent<MeshRenderer>().material = batMatSport;
                }

                //LOD bat
                bat_lod_parent.transform.position = new_bat.GetComponent<MeshRenderer>().bounds.center;
                bat_lod_parent.AddComponent<LODGroup>();
                LODGroup mylodgroup = bat_lod_parent.GetComponent<LODGroup>();
                LOD[] lods = new LOD[1];
                float[] srth = { 0.05f }; //screenRelativeTransitionHeight

                for (int i = 0; i < lods.Length; i++)
                {
                    if (i == 0)
                    {
                        new_bat.transform.transform.parent = bat_lod_parent.transform;
                        MeshRenderer[] bat_renderer = new MeshRenderer[1];
                        bat_renderer[0] = new_bat.GetComponent<MeshRenderer>();
                        lods[i] = new LOD(srth[i], bat_renderer);
                    }
                }
                mylodgroup.SetLODs(lods);
                mylodgroup.RecalculateBounds();
                
                //LOD toit
                toit_lod_parent.transform.position = new_roof.GetComponent<MeshRenderer>().bounds.center;
                toit_lod_parent.AddComponent<LODGroup>();
                LODGroup mylodgroup_toit = toit_lod_parent.GetComponent<LODGroup>();
                LOD[] lods_toit = new LOD[1];
                float[] srth_toit = { 0.05f }; //screenRelativeTransitionHeight

                for (int i = 0; i < lods_toit.Length; i++)
                {
                    if (i == 0)
                    {
                        new_roof.transform.transform.parent = toit_lod_parent.transform;
                        MeshRenderer[] toit_renderer = new MeshRenderer[1];
                        toit_renderer[0] = new_roof.GetComponent<MeshRenderer>();
                        lods_toit[i] = new LOD(srth_toit[i], toit_renderer);
                    }
                }
                mylodgroup_toit.SetLODs(lods_toit);
                mylodgroup_toit.RecalculateBounds();


                toit_lod_parent.transform.parent = ens_toit_bat.transform;
                bat_lod_parent.transform.parent = ens_toit_bat.transform;
                ens_toit_bat.transform.parent = All_bats.transform;
                Destroy(batPoints);
            }
            
            
            //Destroy(GameObject.Find("bati_indifferencie.1658549"));
            //Destroy(GameObject.Find("bati_indifferencie.1658555"));
            //Destroy(GameObject.Find("Toit de bati_indifferencie.1658549"));
        }
        //batiments.transform.parent = All_bat.transform;
        //toits.transform.parent = All_roof.transform;
        //Suppression des bâtiments de l'IGN car ceux-ci sont placés manuellement
        GameObject.Find("bati_indifferencie.1658549").GetComponent<MeshRenderer>().enabled = false;
        GameObject.Find("bati_indifferencie.1658549").GetComponent<MeshCollider>().enabled = false;
        GameObject.Find("Toit de bati_indifferencie.1658549").GetComponent<MeshRenderer>().enabled = false;
        GameObject.Find("bati_indifferencie.1658555").GetComponent<MeshRenderer>().enabled = false;
        GameObject.Find("bati_indifferencie.1658555").GetComponent<MeshCollider>().enabled = false;
        GameObject.Find("Toit de bati_indifferencie.1658555").GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(0.1f);
    }

    //Inutilisée
    void Clean()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Sphere_tag");
        foreach (GameObject item in items)
        {
            Destroy(item);
        }
    }

    //Ancienne fonction permettant de grouper les bâtiments dans un parent.
    public static void GroupBat()
    {
        if (Input.GetKeyDown("h"))
        {
            GameObject All_bat;
            if (GameObject.Find("All_bat") == null)
            {
                All_bat = new GameObject();
                All_bat.name = "All_bat";
            }
            else
            {
                All_bat = GameObject.Find("All_bat");
            }
            GameObject[] all_bat_non_tries = GameObject.FindGameObjectsWithTag("Batiment_tag");
            foreach (GameObject bat in all_bat_non_tries)
            {
                bat.transform.parent = All_bat.transform;
            }
        }

    }
}
