using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///<summary>
/// Ce script doit être placé sur un objet unique de la scène.
/// Cette classe gère la génération des tuiles au lancement.
/// Cette classe génère l'ensemble de la carte dans laquelle sera placé le joueur, c'est-à-dire 
/// le MNT, l'orhto, les bâtiments, les routes, les zones de forêt,...
/// La carte sera toujours de forme carré. 
/// </summary>
public class TilesGenerator : MonoBehaviour
{
    /** Terrain référent pour toutes les tuiles. Cet objet possède l'ensemble des scripts nécessaire pour
     * générer l'ensemble des éléments de la carte. Les autres tuiles seront des copies modifiées de 
     * cet objet.
     */
     [Tooltip("Terrain référent de la scène. Il s'agit de l'objet Terrain_Referent")]
    public GameObject terrain_ref;

    /** Taille de la carte. Correspond au nombre de "couches" de tuiles. Si Size==1 alors on aura
     * qu'une seule tuile. Si Size = 2, on aura 3*3 tuiles ( on rajoute des tuiles autour de la 
     * première tuile au centre)
     */
    [Tooltip("Taille de la carte. Entier entre 1 et 20. Correspond aux 'couches' de tuiles entourant la tuile au centre de la carte. Une taille de 3 semble être le bon compromis entre performance et taille de la carte.")]
    public int Size;

    [Tooltip("Ensemble des modèles de plantation pour les champs.")]
    public GameObject[] myCrops;
    [Tooltip("Ensemble des modèles de tombes pour les cimetierre.")]
    public GameObject[] myGraves;

    [Tooltip("Matériel par défaut des terrains.")]
    public Material default_mat;

    [Tooltip("Choisir Géoroom_real_axes")]
    public GameObject georoom;

    [Tooltip("Ecran de transition placé devant la caméra VR")]
    public GameObject transition_screen;

    /** Altitudes maximale et minimale de l'ensemble des tuiles
     */
    public static float alt_max;
    public static float alt_min;

    /** Objet terrain qui sera placé dans le coin inférieur gauche de la carte. Sa position est donc X=0 Z=0
     */
    GameObject terrain_bas_gauche;


    /** Tailles des côtés d'une tuile. Normalement, ces valeurs doivent rester égale à 256. 
     */
    float lon_diff;
    float lat_diff;

    /** Nombre de tuile le long d'un côté de la carte.
     * 
     */
    int xSize;

    /// <summary>
    /// La fonction Awake() est appelée avant la fonction start dès que le script est activé. 
    /// Ici, on récupère la variable globale Parameter.Size définie dans le menu principale.
    /// ParameterManager.Size correspond à la taille de la carte que l'on va générer.
    /// </summary>
    void Awake()
    {
        Size = ParameterManager.Size;
    }

    /// <summary>
    /// La fonction Start() est appelé dès que le scipt est activé.
    /// </summary>
    void Start()
    {
        //Valeurs par défaut de alt_max et alt_min. Nécessaire pour la recherche des extrema
        alt_max = -10;
        alt_min = 10000;

        lon_diff = terrain_ref.GetComponent<Tile>().right_up_x - terrain_ref.GetComponent<Tile>().left_down_x;
        lat_diff = terrain_ref.GetComponent<Tile>().right_up_y - terrain_ref.GetComponent<Tile>().left_down_y;

        xSize = 2 * Size - 1;

        //terrain-ref est toujours situé au centre de la carte
        terrain_ref.GetComponent<Tile>().position_x = Size - 1;
        terrain_ref.GetComponent<Tile>().position_z = Size - 1;
        terrain_ref.name = "Terrain_X:" + (Size - 1) + "_Z:" + (Size - 1);

        //On place terrain_ref au centre de la carte
        terrain_ref.transform.position = new Vector3(0, 0, 0);
        terrain_ref.transform.Translate(new Vector3(-(Size - 1) * int.Parse(terrain_ref.GetComponent<Tile>().height), 0, (Size - 1) * int.Parse(terrain_ref.GetComponent<Tile>().width)));

        /** Création du terrain situé dans le coin inférieur gauche de la carte. 
         *  On aurait pu tout aussi bien travailler avec le terrain au centre, mais je trouver plus explicit
         *  de se baser dans un coin de la carte.
         */
        terrain_bas_gauche = Instantiate(terrain_ref, new Vector3(0, 0, 0), Quaternion.identity);
        terrain_bas_gauche.GetComponent<Terrain>().terrainData = new TerrainData();
        terrain_bas_gauche.GetComponent<Terrain>().materialTemplate = default_mat;
        terrain_bas_gauche.GetComponent<TerrainGenerator>().width = terrain_ref.GetComponent<TerrainGenerator>().width;
        terrain_bas_gauche.GetComponent<TerrainGenerator>().depth = terrain_ref.GetComponent<TerrainGenerator>().depth;
        terrain_bas_gauche.GetComponent<TerrainGenerator>().height = terrain_ref.GetComponent<TerrainGenerator>().height;
        terrain_bas_gauche.GetComponent<TerrainGenerator>().scale = terrain_ref.GetComponent<TerrainGenerator>().scale;
        terrain_bas_gauche.GetComponent<Tile>().is_ref = false;
        terrain_bas_gauche.GetComponent<Tile>().left_down_x = terrain_ref.GetComponent<Tile>().left_down_x - ((Size - 1) * lon_diff);
        terrain_bas_gauche.GetComponent<Tile>().left_down_y = terrain_ref.GetComponent<Tile>().left_down_y - ((Size - 1) * lat_diff);
        terrain_bas_gauche.GetComponent<Tile>().right_up_x = terrain_ref.GetComponent<Tile>().right_up_x - ((Size - 1) * lon_diff);
        terrain_bas_gauche.GetComponent<Tile>().right_up_y = terrain_ref.GetComponent<Tile>().right_up_y - ((Size - 1) * lat_diff);
        terrain_bas_gauche.GetComponent<Tile>().position_z = 0;
        terrain_bas_gauche.GetComponent<Tile>().position_x = 0;
        terrain_bas_gauche.name = "Terrain_X:" + 0 + "_Z:" + 0;
        StartCoroutine(terrain_bas_gauche.GetComponent<Tile>().GetAlt());

        // On génère l'ensemble de la carte.
        StartCoroutine(CreateAll(3.2f)); 
        //StartCoroutine(CreateTiles());
    }

    /// <summary>
    /// La fonction Update() est appelé à chaque frame, donc 60 fois par secondes.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            //Terrain + MNT
            StartCoroutine(GenerateAllTerrain());
        }
        if (Input.GetKeyDown("s"))
        {
            //On lisse les bords de chaque tuile pour avoir un meilleur rendu.
            StartCoroutine(SmoothAll());
        }

        if (Input.GetKeyDown("t"))
        {
            StartCoroutine(GenerateAllOrtho());
            StartCoroutine(GenerateAllHydro());
        }

        if (Input.GetKeyDown("f"))
        {
            //Création des zones de champs
            StartCoroutine(GenerateAllField());

            //Création des zones de forêts
            StartCoroutine(GenerateAllForest());
        }

        if (Input.GetKeyDown("v"))
        {
            //Création des zones de végétation
            StartCoroutine(GenerateAllVege());
        }
        if (Input.GetKeyDown("p"))
        {
            //Une fois les zones de forêt générées, on peut faire apparaitre des forêts
            StartCoroutine(SpawnAllForest());
        }

        if (Input.GetKeyDown("o"))
        {
            //Une fois les zones de végétation générées, on peut faire apparaitre de la végétation
            StartCoroutine(SpawnAllVege());
        }

        if (Input.GetKeyDown("u"))
        {
            //Une fois les zones de champ générées, on peut faire apparaitre nos plantations
            StartCoroutine(GenerateField.SpawnField(myCrops));
        }

        if (Input.GetKeyDown("b"))
        {
            StartCoroutine(SpawnAllBornes());
        }


    }

    /// <summary>
    /// Parcours tous les objest terrains et applique le MNT.
    /// Initialise les voisins de chaque terrain.
    /// Génère aussi les terrains en bordure de la carte.
    /// </summary>
    /// <returns>Ne retourne rien.</returns>
    IEnumerator GenerateAllTerrain()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            mnt.GetComponent<TerrainGenerator>().GenerateTerrainPublic();
            yield return null;
        }
        TerrainGenerator.FindAndSetNeighbors(xSize);

        MNTs = GameObject.FindGameObjectsWithTag("Decor_tag");
        foreach (GameObject mnt in MNTs)
        {
            mnt.GetComponent<TerrainGenerator>().GenerateTerrainPublic();
            yield return null;
        }

        yield return null;
    }
    /// <summary>
    /// Lisse les bords de chaque tuile pour un meilleur rendu.
    /// </summary>
    /// <returns>Ne retourne rien.</returns>
    IEnumerator SmoothAll()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            mnt.GetComponent<TerrainGenerator>().SmouthEdge();
            yield return null;
        }
        yield return null;
    }

    /// <summary>
    /// Applique l'ortho sur chaque tuile.
    /// </summary>
    /// <returns></returns>
    IEnumerator GenerateAllOrtho()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<Tile>().UpdateSkinTerrain(mnt, default_mat));
            yield return null;
        }

        MNTs = GameObject.FindGameObjectsWithTag("Decor_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<Tile>().UpdateSkinTerrain(mnt, default_mat));
            yield return null;
        }
    }

    /// <summary>
    /// Génère les zones de champs pour chaque tuile.
    /// </summary>
    /// <returns></returns>
    IEnumerator GenerateAllField()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateField>().GetPointsField(mnt));
            yield return null;
        }
    }

    /// <summary>
    /// Génère les zones de forêt pour chaque tuile.
    /// </summary>
    /// <returns></returns>
    IEnumerator GenerateAllForest()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateForest>().GetPointsForest(mnt));
            yield return null;
        }
    }

    /// <summary>
    /// Génère les zones de végétation pour chaque tuile.
    /// </summary>
    /// <returns></returns>
    IEnumerator GenerateAllVege()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateVegetation>().GetPointsVege(mnt));
            yield return null;
        }
    }

    /// <summary>
    /// Fait pousser les forêts.
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnAllForest()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            //Debug.Log(mnt.name);
            mnt.GetComponent<GenerateForest>().SpawnTrees(mnt);
            yield return null;
        }
    }

    /// <summary>
    /// Fait pousser la végétation.
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnAllVege()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            mnt.GetComponent<GenerateVegetation>().SpawnVeges(mnt);
            yield return null;
        }
    }
    /// <summary>
    /// Génère les surfaces d'eau.
    /// La fonction ne devrait pas être utilisée. Il y a encore un bug non résolu.
    /// Le problème est que les points délimitant la surface ne sont pas tous à la même altitude.
    /// Or l'algorithme de triangularisation que j'utilise ne produit que des surfaces planes. On se
    /// retrouve donc avec des surfaces d'eau décolant du sol ou rentrant dans le sol. 
    /// </summary>
    /// <returns></returns>
    IEnumerator GenerateAllHydro()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateHydro>().GetPointsHydro("BDTOPO_BDD_WLD_WGS84G:surface_eau"));
            yield return null;
        }
    }

    /// <summary>
    /// Génère les bornes.
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnAllBornes()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateBornes2>().GetGeodesiePoints());
            yield return null;
        }
    }
    /// <summary>
    /// Génère l'ensembles des bâtiments sur toutes les tuiles.
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnAllBat()
    {
        string typename1 = "";
        string typename2 = "";
        string typename3 = "";
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            if (typename1 == "")
            {
                typename1 = mnt.GetComponent<GenerateBat>().typename1;
                typename2 = mnt.GetComponent<GenerateBat>().typename2;
                typename3 = mnt.GetComponent<GenerateBat>().typename3;
            }
            StartCoroutine(mnt.GetComponent<GenerateBat>().GetPointsBat(typename1));
            StartCoroutine(mnt.GetComponent<GenerateBat>().GetPointsBat(typename2));
            StartCoroutine(mnt.GetComponent<GenerateBat>().GetPointsBat(typename3));
            yield return null;
        }
    }

    /// <summary>
    /// Génère l'ensemble des routes sur toutes les bornes.
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnAllRoads()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateRoad>().GetPointsRoad());
            StartCoroutine(mnt.GetComponent<GenerateRoad>().GetPointsTracks());
            yield return null;
        }
    }

    /// <summary>
    /// Créer les terrain (tuiles) et initialise leurs données (longueur, largeur, altitude, ...).
    /// Pour l'instant les terrains n'ont pas de relief (MNT).
    /// </summary>
    /// <returns></returns>
    IEnumerator CreateTiles()
    {
        for (int z = -3; z < xSize+3; z++)
        {
            for (int x = -3; x < xSize+3; x++)
            {

                if (z == Size - 1 && x == Size - 1)
                {
                    StartCoroutine(terrain_ref.GetComponent<Tile>().GetAlt());
                    //terrain_ref.GetComponent<GenerateMesh>().enabled = true;
                    
                }
                else if (x != 0 || z != 0)
                {
                    GameObject new_terrain = Instantiate(terrain_bas_gauche, new Vector3(-z * (int.Parse(terrain_bas_gauche.GetComponent<Tile>().height)), 0, x * (int.Parse(terrain_bas_gauche.GetComponent<Tile>().width))) + terrain_bas_gauche.transform.position, Quaternion.identity);
                    if (z < 0 || z >= xSize || x < 0 || x >= xSize)
                    {
                        new_terrain.tag = "Decor_tag";
                    }
                    else
                    {
                        new_terrain.tag = "Terrain_tag";
                    }
                    new_terrain.GetComponent<Terrain>().terrainData = new TerrainData();
                    new_terrain.GetComponent<Terrain>().materialTemplate = default_mat;
                    new_terrain.GetComponent<Tile>().layer = terrain_ref.GetComponent<Tile>().layer;
                    new_terrain.GetComponent<Tile>().height = terrain_ref.GetComponent<Tile>().height;
                    new_terrain.GetComponent<Tile>().width = terrain_ref.GetComponent<Tile>().width;
                    new_terrain.GetComponent<Tile>().format = terrain_ref.GetComponent<Tile>().format;
                    new_terrain.GetComponent<Tile>().is_ref = false;
                    new_terrain.GetComponent<Tile>().left_down_x = terrain_bas_gauche.GetComponent<Tile>().left_down_x + (x * lon_diff);
                    new_terrain.GetComponent<Tile>().left_down_y = terrain_bas_gauche.GetComponent<Tile>().left_down_y + (z * lat_diff);
                    new_terrain.GetComponent<Tile>().right_up_x = terrain_bas_gauche.GetComponent<Tile>().right_up_x + (x * lon_diff);
                    new_terrain.GetComponent<Tile>().right_up_y = terrain_bas_gauche.GetComponent<Tile>().right_up_y + (z * lat_diff);
                    new_terrain.GetComponent<Tile>().position_z = ((int)new_terrain.transform.localPosition.z) / (int.Parse(terrain_ref.GetComponent<Tile>().width) - 1);
                    new_terrain.GetComponent<Tile>().position_x = -((int)new_terrain.transform.localPosition.x) / (int.Parse(terrain_ref.GetComponent<Tile>().height) - 1);
                    StartCoroutine(new_terrain.GetComponent<Tile>().GetAlt());
                    new_terrain.name = "Terrain_X:" + new_terrain.GetComponent<Tile>().position_x + "_Z:" + new_terrain.GetComponent<Tile>().position_z;
                    yield return null;
                }
            }
        }

        //Point d'appartion du joueur lorsque ce dernier se retrouve aux commandes d'un drone.
        GameObject mon_point_de_teleportation = new GameObject();
        mon_point_de_teleportation.transform.position = terrain_ref.transform.position;
        mon_point_de_teleportation.transform.Translate(Vector3.up * 100);
        GameObject.Find("OpenableBox").GetComponent<OpenableBoxController>().point_tp_transition = mon_point_de_teleportation.transform;
    }

    /// <summary>
    /// Appelle les fonctions de génération les unes après les autres.
    /// </summary>
    /// <param name="time">Temps entre chaque appel de fonction.</param>
    /// <returns></returns>
    IEnumerator CreateAll(float time)
    {
        StartCoroutine(CreateTiles());
        yield return new WaitForSeconds(time*2);
        StartCoroutine(GenerateAllTerrain());
        yield return new WaitForSeconds(time*2);
        StartCoroutine(SmoothAll());
        yield return new WaitForSeconds(time*2);
        StartCoroutine(GenerateAllOrtho());
        yield return new WaitForSeconds(time);

        //On place le géoroom 
        georoom.GetComponent<ChangePositionGeoroom>().ChangePositionGeo();
        georoom.GetComponent<ChangePositionGeoroom>().ChangePositionPlayer();

        StartCoroutine(SpawnAllBat());
        yield return new WaitForSeconds(time);
        StartCoroutine(SpawnAllRoads());
        yield return new WaitForSeconds(time);
        StartCoroutine(GenerateAllField());
        yield return new WaitForSeconds(time);
        StartCoroutine(GenerateAllForest());
        yield return new WaitForSeconds(time);
        StartCoroutine(GenerateAllVege());
        yield return new WaitForSeconds(time);
        StartCoroutine(SpawnAllForest());
        yield return new WaitForSeconds(time);
        StartCoroutine(SpawnAllVege());
        yield return new WaitForSeconds(time);
        StartCoroutine(GenerateField.SpawnField(myCrops));
        yield return new WaitForSeconds(time);
        StartCoroutine(SpawnAllBornes());

        yield return new WaitForSeconds(2f);

        transition_screen.GetComponent<Animator>().Play("Transition_At_Begin");
        ChangePositionGeoroom.DisableFarRenderer(georoom.transform);
        Destroy(GameObject.Find("All_forest_zones"));
        Destroy(GameObject.Find("All_vege_zone"));
        Destroy(GameObject.Find("All_field_zones"));

        alt_min = GetAltitudeExtrema().Item1;
        alt_max = GetAltitudeExtrema().Item2;
    }

    /// <summary>
    /// Calcule l'altitude maximale et l'altitude minimale de la carte.
    /// </summary>
    /// <returns>Renvoie un couple (min,max)</returns>
    public static (float, float) GetAltitudeExtrema()
    {
        float[] altitudes;
        float altmin = 50000;
        float altmax = -100;
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject MNT in MNTs)
        {
            altitudes = MNT.GetComponent<Tile>().altitudes;
            foreach (float alt in altitudes)
            {
                if (alt >= altmax)
                {
                    altmax = alt;
                }
                if (alt <= altmin)
                {
                    altmin = alt;
                }
            }
        }
        return (altmin, altmax);
    }
}
