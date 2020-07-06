using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.IO;

/// <summary>
/// Ce script doit être placé sur le terrain référent et sera ajouté automatiquement sur chaque autre terrain lors de la génération des terrains.
/// Cette classe génère les routes.
/// </summary>
public class GenerateRoad : MonoBehaviour
{
    [Tooltip("Nom de la couche WFS pour les routes.")]
    public string typename;

    [Tooltip("Format du fichier télécharger pour les routes. On choisira 'json'.")]
    public string format;

    [Tooltip("Materials des routes")]
    public Material[] roadMat;

    [Tooltip("Couleur des routes")]
    public Color colorRoad;

    [Tooltip("Nom de la couche WFS pour les rails.")]
    public string typename_tracks;

    [Tooltip("Format du fichier télécharger pour les rails. On choisira 'json'.")]
    public string format_tracks;

    [Tooltip("Materials des rails. Il doit y avoir 2 éléments : le premier correspond aux graviers sous les rails, le deuxième aux rails")]
    public Material[] trackMat;

    [Tooltip("Modèle des poutres des rails du train. On choisira le modèle 'Poutre'")]
    public GameObject poutre;

    /** Eléments nécessaires pour l'ajout de voitures et de trains. Il s'agit du chemin (point par point)
     * que doivent suivrent les voitures et les trains. Mais cette fonctionnalité n'a pas été terminée.
     */
    public static Vector3[][] roadPath;
    public static Vector3[][] trackPath;

    /** Contour (box) en Lambert 93 de la tuile.
     *  left_down correspond au coin inférieur gauche, right_up au coin supérieur droit.
     */
    (float, float) left_down;
    (float, float) right_up;

    void Start()
    {
        left_down = (this.GetComponent<Tile>().left_down_x, this.GetComponent<Tile>().left_down_y);
        right_up = (this.GetComponent<Tile>().right_up_x, this.GetComponent<Tile>().right_up_y);
    }

    void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            StartCoroutine(GetPointsRoad());
            StartCoroutine(GetPointsTracks());
        }
    }

    /// <summary>
    /// Génère les routes
    /// </summary>
    /// <returns></returns>
    public IEnumerator GetPointsRoad()
    {
        //Pour plus de lisibilité dans la scène lors du run, on regroupe toutes les routes dans un seul objet
        GameObject All_roads;
        if (GameObject.Find("All_roads") == null)
        {
            All_roads = new GameObject();
            All_roads.name = "All_roads";
        }
        else
        {
            All_roads = GameObject.Find("All_roads");
        }

        string myjson;
        string path = "Assets/Data/Roads/" + "road_" + typename + "_xbot_" + this.GetComponent<Tile>().left_down_x + "_ybot_" + this.GetComponent<Tile>().left_down_y + "_xtop_" + this.GetComponent<Tile>().right_up_x + "_ytop_" + this.GetComponent<Tile>().right_up_y + ".json";

        //Téléchargement du fichier json contenant les points délimitant les surfaces d'eau.
        //Le fichier est téléchargé uniquement si le fichier n'existe pas déjà.
        if (!File.Exists(path))
        {
            string url = DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2);
            StartCoroutine(DataController.WriteDataFile(url, path));
            Debug.Log(url);
            yield return null;
        }
        StreamReader reader = new StreamReader(path);
        myjson = reader.ReadToEnd();
        var bigjson = JSON.Parse(myjson);
        reader.Close();

        int layermask = 1 << 9; //les layers ont disparus du projet, ligne potentiellement obsolète
        RaycastHit hit;

        roadPath = new Vector3[bigjson["features"].Count][];
        for (int j = 0; j < bigjson["features"].Count; j++)
        {
            if (GameObject.Find(bigjson["features"][j]["id"]) == null)
            {
                //Debug.Log(DataController.GetWfsRequest(typename, format, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2));
                GameObject route = new GameObject();
                route.name = bigjson["features"][j]["id"];

                //Objet qui contiendra l'ensemble des points pour tracer la route
                GameObject roadPoints = new GameObject();

                //items contient l'ensemble des coordonnées des points de la route
                JSONArray items = (JSONArray)bigjson["features"][j]["geometry"]["coordinates"][0]; //il faut rester en JSONArray sinon il y a un problème pour lire les valeurs
                for (int i = 0; i < items.Count; i++)
                {
                    //J'ai utilisé une sphère pour représenter le point. Un transform simple (empty gameobject) aurait pu suffire.
                    //Néanmoins, lors des tests, pour vérifier visuellement était un plus.
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    float x = items[i][0] - left_down.Item1;
                    //float y = items[i][2];
                    float y = 0;
                    float z = items[i][1] - left_down.Item2;

                    if (y < 4000)
                    {
                        sphere.transform.position = new Vector3(x, y, z);

                        sphere.transform.parent = roadPoints.transform;
                        sphere.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                        sphere.GetComponent<Renderer>().material.color = Color.red;
                    }
                    Destroy(sphere);
                }

                //Petites modfications pour remettre les points au bon endroit
                roadPoints.transform.Rotate(0, -90, 0);
                roadPoints.transform.position = transform.position;
                if (GetComponent<GenerateMesh>()!=null)
                {
                    roadPoints.transform.Translate(0, 0, -GetComponent<GenerateMesh>().xSize *GetComponent<GenerateMesh>().rapport_réduction);
                }
                else if (GetComponent<TerrainGenerator>()!=null)
                {
                    roadPoints.transform.Translate(0, 0, -GetComponent<TerrainGenerator>().depth);
                }

                //Nombre de points nécessaire pour construire la route
                int nb_points = items.Count;

                Transform old_trans;
                Transform mid_trans;
                Transform new_trans;

                float distance_old_mid;
                float distance_mid_new;

                Vector3 between_old_mid;
                Vector3 between_mid_new;

                //Ici on va créer plus de point (des repères en gros) entre les points déjà existant
                //Cela nous permettra d'avoir un meilleur rendu lorsqu'on collera la route sur le MNT
                for (int i = 0; i < nb_points - 1; i++)
                {
                    //Point par point, on regarde déjà si la disatnce entre les deux points est assez grande pour 
                    //subdiviser davantage
                    old_trans = roadPoints.transform.GetChild(i);
                    mid_trans = roadPoints.transform.GetChild(i + 1);
                    between_old_mid = old_trans.position - mid_trans.position;
                    distance_old_mid = between_old_mid.magnitude;
                    if (distance_old_mid >= 3)
                    {
                        //Si tel est le cas, on rajoute un point entre les deux 
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        sphere.transform.position = (old_trans.position + mid_trans.position) / 2;
                        sphere.transform.parent = roadPoints.transform;
                        //On n'oublie pas de décaler les indices et donc d'intercaler le nouveau point entre les deux précédent
                        sphere.transform.SetSiblingIndex(i + 1);
                        nb_points += 1;
                        i--;
                        Destroy(sphere);
                    }

                }

                roadPath[j] = new Vector3[nb_points];
                int index_roadPath = 0;

                //Plaquage de la route sur le MNT à l'aide de raycast
                foreach (Transform t in roadPoints.transform)
                {
                    if (Physics.Raycast(t.position + new Vector3(0, 500, 0), Vector3.down, out hit, 10000, layermask))
                    {
                        if (hit.transform.gameObject.GetComponent<Tile>() != null)
                        {
                            t.position = hit.point;
                        }
                    }
                }

                //Il s'agit du path pour les voitures
                //Mais cette fonctionnalité n'est pas complètement terminée
                foreach (Transform t in roadPoints.transform)
                {
                    roadPath[j][index_roadPath] = t.position + new Vector3(0, 0.55f, 0);
                    index_roadPath++;
                }

                float largeur_route;
                //J'ai enlevé les trottoirs car cela ne rendait pas très bien
                float largeur_trottoir = 1.2f;
                float hauteur_route = 0.08f;
                float hauteur_trottoir = hauteur_route + 0.03f;
                float hauteur_tot = hauteur_route + hauteur_trottoir;

                //Largeur de la route à un mètre par défaut (cela ne concerne que les tous petits chemins)
                if (bigjson["features"][j]["properties"]["largeur"] == 0)
                {
                    largeur_route = 1;
                }
                else
                {
                    largeur_route = bigjson["features"][j]["properties"]["largeur"];
                }

                //Déclaration de toutes les variables nécessaires à la construction des routes
                Mesh mesh = new Mesh();
                Vector3[] vertices = new Vector3[4 * nb_points]; //sans les trottoirs
                //Vector3[] vertices = new Vector3[10 * nb_points]; //averc les trottoirs
                int vertices_index = 0;
                Vector2[] uvs = new Vector2[vertices.Length];
                //Color[] colors = new Color[vertices.Length];
                route.AddComponent<MeshFilter>();
                route.AddComponent<MeshRenderer>();
                route.GetComponent<MeshFilter>().mesh = mesh;

                /** Pour la construction des routes, il faut imaginer que l'on considère la route par portion de
                 * 3 points (old, mid, new). On construira une route en ligne droite entre old et mid,  et une entre 
                 * mid et new.
                 * 
                 * Pour avoir des routes 'jolies', il faut que la jointure entre les deux parcelles de route se fasse
                 * sans interruption. On prendra donc en considération le vecteur somme entre les deux vecteurs directeurs
                 * de nos deux parcelles.
                 * 
                 */

                //Il s'agit des 2 vecteurs directeurs et du vecteur somme (line_average)
                Vector2 line_old_mid;
                Vector2 line_average;
                Vector2 line_mid_new;

                //Perpendiculaires aux 2 vecteurs directeurs et du vecteur somme (perp_average)
                Vector2 perp_old_mid;
                Vector2 perp_average;
                Vector2 perp_mid_new;

                //Similaires aux perpendiculaires, mais orientés dans le bon sens et en 3D
                Vector3 decale_old_mid;
                Vector3 decale_average;
                Vector3 decale_mid_new;

                //Dans le cas où notre route est seulement composé de deux points
                if (nb_points == 2)
                {
                    //On récupère nos deux points
                    old_trans = roadPoints.transform.GetChild(0);
                    mid_trans = roadPoints.transform.GetChild(1);

                    //Calcul des vecteurs directeurs et des perpendiculaires
                    between_old_mid = old_trans.position - mid_trans.position;
                    line_old_mid = new Vector2(old_trans.position.x - mid_trans.position.x, old_trans.position.z - mid_trans.position.z);
                    perp_old_mid = Vector2.Perpendicular(line_old_mid);
                    decale_old_mid = new Vector3(perp_old_mid.x, 0, perp_old_mid.y);
                    decale_old_mid = decale_old_mid.normalized;
                    decale_old_mid = decale_old_mid * largeur_route;

                    //vertices[vertices_index + 0] = old_trans.position + decale_old_mid * largeur_trottoir;
                    //vertices[vertices_index + 1] = old_trans.position + decale_old_mid;
                    //vertices[vertices_index + 2] = old_trans.position - decale_old_mid;
                    //vertices[vertices_index + 3] = old_trans.position - decale_old_mid * largeur_trottoir;
                    //vertices[vertices_index + 4] = old_trans.position + decale_old_mid * largeur_trottoir + Vector3.up * hauteur_tot;
                    //vertices[vertices_index + 5] = old_trans.position + decale_old_mid + Vector3.up * hauteur_tot;
                    //vertices[vertices_index + 6] = old_trans.position + decale_old_mid + Vector3.up * hauteur_route;
                    //vertices[vertices_index + 7] = old_trans.position - decale_old_mid + Vector3.up * hauteur_route;
                    //vertices[vertices_index + 8] = old_trans.position - decale_old_mid + Vector3.up * hauteur_tot;
                    //vertices[vertices_index + 9] = old_trans.position - decale_old_mid * largeur_trottoir + Vector3.up * hauteur_tot;

                    //vertices[vertices_index + 10] = mid_trans.position + decale_old_mid * largeur_trottoir;
                    //vertices[vertices_index + 11] = mid_trans.position + decale_old_mid;
                    //vertices[vertices_index + 12] = mid_trans.position - decale_old_mid;
                    //vertices[vertices_index + 13] = mid_trans.position - decale_old_mid * largeur_trottoir;
                    //vertices[vertices_index + 14] = mid_trans.position + decale_old_mid * largeur_trottoir + Vector3.up * hauteur_tot;
                    //vertices[vertices_index + 15] = mid_trans.position + decale_old_mid + Vector3.up * hauteur_tot;
                    //vertices[vertices_index + 16] = mid_trans.position + decale_old_mid + Vector3.up * hauteur_route;
                    //vertices[vertices_index + 17] = mid_trans.position - decale_old_mid + Vector3.up * hauteur_route;
                    //vertices[vertices_index + 18] = mid_trans.position - decale_old_mid + Vector3.up * hauteur_tot;
                    //vertices[vertices_index + 19] = mid_trans.position - decale_old_mid * largeur_trottoir + Vector3.up * hauteur_tot;

                    //Placement des vertices
                    vertices[vertices_index + 0] = old_trans.position + decale_old_mid;
                    vertices[vertices_index + 1] = old_trans.position - decale_old_mid;
                    vertices[vertices_index + 2] = old_trans.position + decale_old_mid + Vector3.up * hauteur_route;
                    vertices[vertices_index + 3] = old_trans.position - decale_old_mid + Vector3.up * hauteur_route;

                    vertices[vertices_index + 4] = mid_trans.position + decale_old_mid;
                    vertices[vertices_index + 5] = mid_trans.position - decale_old_mid;
                    vertices[vertices_index + 6] = mid_trans.position + decale_old_mid + Vector3.up * hauteur_route;
                    vertices[vertices_index + 7] = mid_trans.position - decale_old_mid + Vector3.up * hauteur_route;

                }

                //Dans le cas où la route est composé de 3 points ou plus
                if (nb_points >= 3)
                {
                    for (int i = 0; i < nb_points - 2; i++)
                    {
                        //On travaille par tranche de 3 points
                        old_trans = roadPoints.transform.GetChild(i);
                        mid_trans = roadPoints.transform.GetChild(i + 1);
                        new_trans = roadPoints.transform.GetChild(i + 2);


                        //Calcul desd vecteurs directeurs et des perpendiculaires
                        between_old_mid = old_trans.position - mid_trans.position;
                        between_mid_new = mid_trans.position - new_trans.position;

                        distance_old_mid = between_old_mid.magnitude;
                        distance_mid_new = between_mid_new.magnitude;

                        //float nb_ray_line = distance * 3;
                        line_old_mid = new Vector2(old_trans.position.x - mid_trans.position.x, old_trans.position.z - mid_trans.position.z);
                        perp_old_mid = Vector2.Perpendicular(line_old_mid);

                        line_mid_new = new Vector2(mid_trans.position.x - new_trans.position.x, mid_trans.position.z - new_trans.position.z);
                        perp_mid_new = Vector2.Perpendicular(line_mid_new);

                        line_average = (line_old_mid + line_mid_new) / 2;
                        perp_average = Vector2.Perpendicular(line_average);

                        decale_old_mid = new Vector3(perp_old_mid.x, 0, perp_old_mid.y);
                        decale_mid_new = new Vector3(perp_mid_new.x, 0, perp_mid_new.y);
                        decale_average = new Vector3(perp_average.x, 0, perp_average.y);

                        decale_old_mid = decale_old_mid.normalized;
                        decale_mid_new = decale_mid_new.normalized;
                        decale_average = decale_average.normalized;

                        decale_old_mid = decale_old_mid * largeur_route;
                        decale_mid_new = decale_mid_new * largeur_route;
                        decale_average = decale_average * largeur_route;

                        //vertices[vertices_index + 0] = old_trans.position + decale_old_mid * largeur_trottoir;
                        //vertices[vertices_index + 1] = old_trans.position + decale_old_mid;
                        //vertices[vertices_index + 2] = old_trans.position - decale_old_mid;
                        //vertices[vertices_index + 3] = old_trans.position - decale_old_mid * largeur_trottoir;
                        //vertices[vertices_index + 4] = old_trans.position + decale_old_mid * largeur_trottoir + Vector3.up * hauteur_tot;
                        //vertices[vertices_index + 5] = old_trans.position + decale_old_mid + Vector3.up * hauteur_tot;
                        //vertices[vertices_index + 6] = old_trans.position + decale_old_mid + Vector3.up * hauteur_route;
                        //vertices[vertices_index + 7] = old_trans.position - decale_old_mid + Vector3.up * hauteur_route;
                        //vertices[vertices_index + 8] = old_trans.position - decale_old_mid + Vector3.up * hauteur_tot;
                        //vertices[vertices_index + 9] = old_trans.position - decale_old_mid * largeur_trottoir + Vector3.up * hauteur_tot;

                        //vertices[vertices_index + 10] = mid_trans.position + decale_average * largeur_trottoir;
                        //vertices[vertices_index + 11] = mid_trans.position + decale_average;
                        //vertices[vertices_index + 12] = mid_trans.position - decale_average;
                        //vertices[vertices_index + 13] = mid_trans.position - decale_average * largeur_trottoir;
                        //vertices[vertices_index + 14] = mid_trans.position + decale_average * largeur_trottoir + Vector3.up * hauteur_tot;
                        //vertices[vertices_index + 15] = mid_trans.position + decale_average + Vector3.up * hauteur_tot;
                        //vertices[vertices_index + 16] = mid_trans.position + decale_average + Vector3.up * hauteur_route;
                        //vertices[vertices_index + 17] = mid_trans.position - decale_average + Vector3.up * hauteur_route;
                        //vertices[vertices_index + 18] = mid_trans.position - decale_average + Vector3.up * hauteur_tot;
                        //vertices[vertices_index + 19] = mid_trans.position - decale_average * largeur_trottoir + Vector3.up * hauteur_tot;

                        //vertices[vertices_index + 20] = new_trans.position + decale_mid_new * largeur_trottoir;
                        //vertices[vertices_index + 21] = new_trans.position + decale_mid_new;
                        //vertices[vertices_index + 22] = new_trans.position - decale_mid_new;
                        //vertices[vertices_index + 23] = new_trans.position - decale_mid_new * largeur_trottoir;
                        //vertices[vertices_index + 24] = new_trans.position + decale_mid_new * largeur_trottoir + Vector3.up * hauteur_tot;
                        //vertices[vertices_index + 25] = new_trans.position + decale_mid_new + Vector3.up * hauteur_tot;
                        //vertices[vertices_index + 26] = new_trans.position + decale_mid_new + Vector3.up * hauteur_route;
                        //vertices[vertices_index + 27] = new_trans.position - decale_mid_new + Vector3.up * hauteur_route;
                        //vertices[vertices_index + 28] = new_trans.position - decale_mid_new + Vector3.up * hauteur_tot;
                        //vertices[vertices_index + 29] = new_trans.position - decale_mid_new * largeur_trottoir + Vector3.up * hauteur_tot;

                        //Placement ds vertices
                        vertices[vertices_index + 0] = old_trans.position + decale_old_mid;
                        vertices[vertices_index + 1] = old_trans.position - decale_old_mid;
                        vertices[vertices_index + 2] = old_trans.position + decale_old_mid + Vector3.up * hauteur_route;
                        vertices[vertices_index + 3] = old_trans.position - decale_old_mid + Vector3.up * hauteur_route;

                        vertices[vertices_index + 4] = mid_trans.position + decale_average;
                        vertices[vertices_index + 5] = mid_trans.position - decale_average;
                        vertices[vertices_index + 6] = mid_trans.position + decale_average + Vector3.up * hauteur_route;
                        vertices[vertices_index + 7] = mid_trans.position - decale_average + Vector3.up * hauteur_route;

                        vertices[vertices_index + 8] = new_trans.position + decale_mid_new;
                        vertices[vertices_index + 9] = new_trans.position - decale_mid_new;
                        vertices[vertices_index + 10] = new_trans.position + decale_mid_new + Vector3.up * hauteur_route;
                        vertices[vertices_index + 11] = new_trans.position - decale_mid_new + Vector3.up * hauteur_route;

                        //vertices_index += 10;
                        vertices_index += 4;
                    }
                }

                /** Maintenant qu'on a placé les vertices, il faut placer les triangles.
                 * Pour ça on référence les (3) indices des vertices pour chaque triangle.
                 * Nécessaire de faire un schéma pour s'y retrouver.
                 */

                //avec les trottoirs
                //int[] triangles = new int[8 * (nb_points - 1) * 3];
                //int[] triangles_trottoirs = new int[24 * (nb_points - 1) * 3];

                int[] triangles = new int[12 * (nb_points - 1) * 3]; //sans les troittoires

                int tri_index = 0;
                //int tri_tro_index = 0;
                int vert_index = 0;

                for (int i = 0; i < nb_points - 1; i++)
                {
                    ////ROUTE
                    ////Dessous
                    //triangles[tri_index + 0] = vert_index + 1;
                    //triangles[tri_index + 1] = vert_index + 11;
                    //triangles[tri_index + 2] = vert_index + 2;

                    //triangles[tri_index + 3] = vert_index + 2;
                    //triangles[tri_index + 4] = vert_index + 11;
                    //triangles[tri_index + 5] = vert_index + 12;

                    ////Dessus
                    //triangles[tri_index + 6] = vert_index + 6;
                    //triangles[tri_index + 7] = vert_index + 7;
                    //triangles[tri_index + 8] = vert_index + 16;

                    //triangles[tri_index + 9] = vert_index + 7;
                    //triangles[tri_index + 10] = vert_index + 17;
                    //triangles[tri_index + 11] = vert_index + 16;

                    ////face avant
                    //triangles[tri_index + 12] = vert_index + 1;
                    //triangles[tri_index + 13] = vert_index + 2;
                    //triangles[tri_index + 14] = vert_index + 6;

                    //triangles[tri_index + 15] = vert_index + 2;
                    //triangles[tri_index + 16] = vert_index + 7;
                    //triangles[tri_index + 17] = vert_index + 6;

                    ////face arrière
                    //triangles[tri_index + 18] = vert_index + 11;
                    //triangles[tri_index + 19] = vert_index + 12;
                    //triangles[tri_index + 20] = vert_index + 16;

                    //triangles[tri_index + 21] = vert_index + 12;
                    //triangles[tri_index + 22] = vert_index + 17;
                    //triangles[tri_index + 23] = vert_index + 16;

                    ////trottoir Gauche

                    ////Dessous
                    //triangles_trottoirs[tri_tro_index + 0] = vert_index + 0;
                    //triangles_trottoirs[tri_tro_index + 1] = vert_index + 1;
                    //triangles_trottoirs[tri_tro_index + 2] = vert_index + 10;

                    //triangles_trottoirs[tri_tro_index + 3] = vert_index + 1;
                    //triangles_trottoirs[tri_tro_index + 4] = vert_index + 11;
                    //triangles_trottoirs[tri_tro_index + 5] = vert_index + 10;

                    ////Dessus
                    //triangles_trottoirs[tri_tro_index + 6] = vert_index + 4;
                    //triangles_trottoirs[tri_tro_index + 7] = vert_index + 5;
                    //triangles_trottoirs[tri_tro_index + 8] = vert_index + 14;

                    //triangles_trottoirs[tri_tro_index + 9] = vert_index + 5;
                    //triangles_trottoirs[tri_tro_index + 10] = vert_index + 15;
                    //triangles_trottoirs[tri_tro_index + 11] = vert_index + 14;

                    ////face avant 
                    //triangles_trottoirs[tri_tro_index + 12] = vert_index + 0;
                    //triangles_trottoirs[tri_tro_index + 13] = vert_index + 1;
                    //triangles_trottoirs[tri_tro_index + 14] = vert_index + 4;

                    //triangles_trottoirs[tri_tro_index + 15] = vert_index + 1;
                    //triangles_trottoirs[tri_tro_index + 16] = vert_index + 5;
                    //triangles_trottoirs[tri_tro_index + 17] = vert_index + 4;

                    ////face arrière
                    //triangles_trottoirs[tri_tro_index + 18] = vert_index + 10;
                    //triangles_trottoirs[tri_tro_index + 19] = vert_index + 15;
                    //triangles_trottoirs[tri_tro_index + 20] = vert_index + 11;

                    //triangles_trottoirs[tri_tro_index + 21] = vert_index + 10;
                    //triangles_trottoirs[tri_tro_index + 22] = vert_index + 14;
                    //triangles_trottoirs[tri_tro_index + 23] = vert_index + 15;

                    ////Côté interne
                    //triangles_trottoirs[tri_tro_index + 24] = vert_index + 6;
                    //triangles_trottoirs[tri_tro_index + 25] = vert_index + 16;
                    //triangles_trottoirs[tri_tro_index + 26] = vert_index + 5;

                    //triangles_trottoirs[tri_tro_index + 27] = vert_index + 16;
                    //triangles_trottoirs[tri_tro_index + 28] = vert_index + 15;
                    //triangles_trottoirs[tri_tro_index + 29] = vert_index + 5;

                    ////Côté externe
                    //triangles_trottoirs[tri_tro_index + 30] = vert_index + 0;
                    //triangles_trottoirs[tri_tro_index + 31] = vert_index + 14;
                    //triangles_trottoirs[tri_tro_index + 32] = vert_index + 10;

                    //triangles_trottoirs[tri_tro_index + 33] = vert_index + 0;
                    //triangles_trottoirs[tri_tro_index + 34] = vert_index + 4;
                    //triangles_trottoirs[tri_tro_index + 35] = vert_index + 14;


                    ////trottoir DROITE
                    ////Dessous
                    //triangles_trottoirs[tri_tro_index + 36] = vert_index + 2;
                    //triangles_trottoirs[tri_tro_index + 37] = vert_index + 3;
                    //triangles_trottoirs[tri_tro_index + 38] = vert_index + 12;

                    //triangles_trottoirs[tri_tro_index + 39] = vert_index + 3;
                    //triangles_trottoirs[tri_tro_index + 40] = vert_index + 13;
                    //triangles_trottoirs[tri_tro_index + 41] = vert_index + 12;

                    ////Dessus
                    //triangles_trottoirs[tri_tro_index + 42] = vert_index + 8;
                    //triangles_trottoirs[tri_tro_index + 43] = vert_index + 9;
                    //triangles_trottoirs[tri_tro_index + 44] = vert_index + 18;

                    //triangles_trottoirs[tri_tro_index + 45] = vert_index + 9;
                    //triangles_trottoirs[tri_tro_index + 46] = vert_index + 19;
                    //triangles_trottoirs[tri_tro_index + 47] = vert_index + 18;

                    ////face avant 
                    //triangles_trottoirs[tri_tro_index + 48] = vert_index + 2;
                    //triangles_trottoirs[tri_tro_index + 49] = vert_index + 3;
                    //triangles_trottoirs[tri_tro_index + 50] = vert_index + 8;

                    //triangles_trottoirs[tri_tro_index + 51] = vert_index + 3;
                    //triangles_trottoirs[tri_tro_index + 52] = vert_index + 9;
                    //triangles_trottoirs[tri_tro_index + 53] = vert_index + 8;

                    ////face arrière
                    //triangles_trottoirs[tri_tro_index + 54] = vert_index + 13;
                    //triangles_trottoirs[tri_tro_index + 55] = vert_index + 12;
                    //triangles_trottoirs[tri_tro_index + 56] = vert_index + 19;

                    //triangles_trottoirs[tri_tro_index + 57] = vert_index + 12;
                    //triangles_trottoirs[tri_tro_index + 58] = vert_index + 18;
                    //triangles_trottoirs[tri_tro_index + 59] = vert_index + 19;

                    ////côté interne 
                    //triangles_trottoirs[tri_tro_index + 60] = vert_index + 8;
                    //triangles_trottoirs[tri_tro_index + 61] = vert_index + 17;
                    //triangles_trottoirs[tri_tro_index + 62] = vert_index + 7;

                    //triangles_trottoirs[tri_tro_index + 63] = vert_index + 8;
                    //triangles_trottoirs[tri_tro_index + 64] = vert_index + 18;
                    //triangles_trottoirs[tri_tro_index + 65] = vert_index + 17;

                    ////côté externe
                    //triangles_trottoirs[tri_tro_index + 66] = vert_index + 3;
                    //triangles_trottoirs[tri_tro_index + 67] = vert_index + 13;
                    //triangles_trottoirs[tri_tro_index + 68] = vert_index + 19;

                    //triangles_trottoirs[tri_tro_index + 69] = vert_index + 3;
                    //triangles_trottoirs[tri_tro_index + 70] = vert_index + 19;
                    //triangles_trottoirs[tri_tro_index + 71] = vert_index + 9;

                    //vert_index += 10;
                    //tri_index += 24;
                    //tri_tro_index += 72;

                    //ROUTE
                    //Dessous
                    triangles[tri_index + 0] = vert_index + 0;
                    triangles[tri_index + 1] = vert_index + 5;
                    triangles[tri_index + 2] = vert_index + 1;

                    triangles[tri_index + 3] = vert_index + 0;
                    triangles[tri_index + 4] = vert_index + 4;
                    triangles[tri_index + 5] = vert_index + 5;

                    //Dessus
                    triangles[tri_index + 6] = vert_index + 2;
                    triangles[tri_index + 7] = vert_index + 3;
                    triangles[tri_index + 8] = vert_index + 7;

                    triangles[tri_index + 9] = vert_index + 2;
                    triangles[tri_index + 10] = vert_index + 7;
                    triangles[tri_index + 11] = vert_index + 6;

                    //face avant
                    triangles[tri_index + 12] = vert_index + 0;
                    triangles[tri_index + 13] = vert_index + 1;
                    triangles[tri_index + 14] = vert_index + 3;

                    triangles[tri_index + 15] = vert_index + 0;
                    triangles[tri_index + 16] = vert_index + 3;
                    triangles[tri_index + 17] = vert_index + 2;

                    //face arrière
                    triangles[tri_index + 18] = vert_index + 5;
                    triangles[tri_index + 19] = vert_index + 4;
                    triangles[tri_index + 20] = vert_index + 7;

                    triangles[tri_index + 21] = vert_index + 4;
                    triangles[tri_index + 22] = vert_index + 6;
                    triangles[tri_index + 23] = vert_index + 7;

                    //Côté droit
                    triangles[tri_index + 24] = vert_index + 1;
                    triangles[tri_index + 25] = vert_index + 5;
                    triangles[tri_index + 26] = vert_index + 3;

                    triangles[tri_index + 27] = vert_index + 3;
                    triangles[tri_index + 28] = vert_index + 5;
                    triangles[tri_index + 29] = vert_index + 7;

                    //Côté gauche
                    triangles[tri_index + 30] = vert_index + 4;
                    triangles[tri_index + 31] = vert_index + 0;
                    triangles[tri_index + 32] = vert_index + 2;

                    triangles[tri_index + 33] = vert_index + 4;
                    triangles[tri_index + 34] = vert_index + 2;
                    triangles[tri_index + 35] = vert_index + 6;


                    vert_index += 4;
                    tri_index += 36;
                }

                //La route étant plate, on n'a pas besoin de se casser plus la tête pour les uvs
                for (int i = 0; i < uvs.Length; i++)
                {
                    uvs[i+0] = new Vector2(vertices[i].x, vertices[i].z);
                }

                //On applique le material que l'on a choisi dans l'editeur
                route.GetComponent<MeshRenderer>().material = roadMat[0];

                //On applique le mesh créer à notre route
                mesh.Clear();
                //mesh.subMeshCount = 2;
                //mesh.subMeshCount = 1;
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                //mesh.SetTriangles(triangles_trottoirs, 1);
                mesh.uv = uvs;
                mesh.RecalculateNormals();

                //On supprime les points de la route, ces derniers étant désormais inutiles
                Destroy(roadPoints);
                route.transform.parent = All_roads.transform;
                route.AddComponent<MeshCollider>();
                //route.AddComponent<RendererTimer>();
            }
        }
    }

    /// <summary>
    /// Génère les rails.
    /// Est très similaire à GetsPointsRoads()
    /// </summary>
    /// <returns></returns>
    public IEnumerator GetPointsTracks()
    {
        GameObject All_tracks;
        if (GameObject.Find("All_tracks") == null)
        {
            All_tracks = new GameObject();
            All_tracks.name = "All_tracks";
        }
        else
        {
            All_tracks = GameObject.Find("All_trakcs");
        }

        string myjson;
        string path = "Assets/Data/Roads/" + "track_" + typename_tracks + "_xbot_" + this.GetComponent<Tile>().left_down_x + "_ybot_" + this.GetComponent<Tile>().left_down_y + "_xtop_" + this.GetComponent<Tile>().right_up_x + "_ytop_" + this.GetComponent<Tile>().right_up_y + ".json";
        if (!File.Exists(path))
        {
            string url = DataController.GetWfsRequest(typename_tracks, format_tracks, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2);
            StartCoroutine(DataController.WriteDataFile(url, path));
            Debug.Log(url);
            yield return null;
        }
        StreamReader reader = new StreamReader(path);
        myjson = reader.ReadToEnd();
        var bigjson = JSON.Parse(myjson);
        reader.Close();

        int layermask = 1 << 9;
        RaycastHit hit;

        trackPath = new Vector3[bigjson["features"].Count][];

        for (int j = 0; j < bigjson["features"].Count; j++)
        {

            if (GameObject.Find(bigjson["features"][j]["id"]) == null && bigjson["features"][j]["properties"]["franchisst"] != "Tunnel")
            {
                Debug.Log(DataController.GetWfsRequest(typename_tracks, format_tracks, left_down.Item1, left_down.Item2, right_up.Item1, right_up.Item2));
                GameObject RAIL = new GameObject();
                string nature;
                RAIL.name = bigjson["features"][j]["id"];
                GameObject trackPoints = new GameObject();
                trackPoints.name = "trackPoints" + GameObject.Find(bigjson["features"][j]["id"]);
                JSONArray items = (JSONArray)bigjson["features"][j]["geometry"]["coordinates"][0]; //il faut rester en JSONArray sinon il y a un problème pour lire les valeurs

                for (int i = 0; i < items.Count; i++)
                {
                        
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        float x = items[i][0] - left_down.Item1;
                    //float y = items[i][2];
                        float y = 0;
                        float z = items[i][1] - left_down.Item2;

                        if (y < 4000)
                        {
                            sphere.transform.position = new Vector3(x, y, z);
                            sphere.transform.parent = trackPoints.transform;
                            sphere.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                            sphere.GetComponent<Renderer>().material.color = Color.red;
                        }
                        Destroy(sphere);
                }

                trackPoints.transform.Rotate(0, -90, 0);
                trackPoints.transform.position = transform.position;

                if (this.GetComponent<GenerateMesh>() != null)
                {
                    trackPoints.transform.Translate(0, 0, -this.GetComponent<GenerateMesh>().xSize * this.GetComponent<GenerateMesh>().rapport_réduction);
                }
                else if (GetComponent<TerrainGenerator>() != null)
                {
                    trackPoints.transform.Translate(0, 0, -this.GetComponent<TerrainGenerator>().depth);
                }

                int nb_points = items.Count;

                Transform old_trans;
                Transform mid_trans;
                Transform new_trans;

                float distance_old_mid;
                float distance_mid_new;

                Vector3 between_old_mid;
                Vector3 between_mid_new;

                for (int i = 0; i < nb_points - 1; i++)
                {
                    old_trans = trackPoints.transform.GetChild(i);
                    mid_trans = trackPoints.transform.GetChild(i + 1);
                    between_old_mid = old_trans.position - mid_trans.position;
                    distance_old_mid = between_old_mid.magnitude;

                    if (distance_old_mid >= 3)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        sphere.transform.position = (old_trans.position + mid_trans.position) / 2;
                        sphere.transform.parent = trackPoints.transform;
                        sphere.transform.SetSiblingIndex(i + 1);
                        nb_points += 1;
                        i--;
                        Destroy(sphere);
                    }

                }

                trackPath[j] = new Vector3[nb_points];
                int index_trackPath = 0;

                //Plaquage du rail sur le MNT
                foreach (Transform t in trackPoints.transform)
                {
                    if (Physics.Raycast(t.position + new Vector3(0, 500, 0), Vector3.down, out hit, 10000, layermask))
                    {
                        if (hit.transform.gameObject.GetComponent<Tile>() != null)
                        {
                            t.position = hit.point;
                        }
                        //else
                        //{
                        //    //t.Translate()
                        //}

                    }
                }


                foreach (Transform t in trackPoints.transform)
                {
                    trackPath[j][index_trackPath] = t.position + new Vector3(0, 0.55f, 0);
                    index_trackPath++;
                }

                float largeur_route_bas = 1.3f;
                float largeur_route_haut = 0.9f;
                float hauteur_route = 0.16f;

                float largeur_rail_a = 0.17f * 0.35f;
                float largeur_rail_b = 0.11f * 0.35f;
                float largeur_rail_c = 0.02f * 0.35f;
                float largeur_rail_d = 0.06f * 0.35f;
                float largeur_rail_e = 0.12f * 0.35f;
                float largeur_rail_f = 0.09f * 0.35f;

                float hauteur_rail_a = 0.05f * 0.35f;
                float hauteur_rail_b = 0.2f * 0.35f;
                float hauteur_rail_c = 0.4f * 0.35f;
                float hauteur_rail_d = 0.57f * 0.35f;
                float hauteur_rail_e = 0.65f * 0.35f;

                float espace_between_roads = 1f;
                float espace_between_tracks = 0.305f;

                //if (bigjson["features"][j]["properties"]["largeur"] == 0)
                //{
                //    largeur_route_bas = 1;
                //    largeur_route_haut = 0.80f;

                //}
                //else
                //{
                //    largeur_route_bas = bigjson["features"][j]["properties"]["largeur"];
                //    largeur_route_haut = bigjson["features"][j]["properties"]["largeur"]*(4/5);
                //}

                Mesh mesh_route_gauche = new Mesh();
                Mesh mesh_route_droite = new Mesh();
                Mesh mesh_rail_gauche_gauche = new Mesh();
                Mesh mesh_rail_gauche_droite = new Mesh();
                Mesh mesh_rail_droite_gauche = new Mesh();
                Mesh mesh_rail_droite_droite = new Mesh();

                Vector3[] vertices_route_gauche = new Vector3[4 * nb_points];
                Vector3[] vertices_route_droite = new Vector3[4 * nb_points];
                Vector3[] vertices_rail_droite_gauche = new Vector3[12 * nb_points];
                Vector3[] vertices_rail_droite_droite = new Vector3[12 * nb_points];
                Vector3[] vertices_rail_gauche_gauche = new Vector3[12 * nb_points];
                Vector3[] vertices_rail_gauche_droite = new Vector3[12 * nb_points];

                int vertices_index_route = 0;
                int vertices_index_rail = 0;
                Vector2[] uvs_route = new Vector2[vertices_route_gauche.Length];
                Color[] colors_route = new Color[vertices_route_gauche.Length];

                Vector2[] uvs_rail = new Vector2[vertices_rail_gauche_gauche.Length];
                Color[] colors_rail = new Color[vertices_rail_gauche_gauche.Length];

                GameObject route_gauche = new GameObject();
                route_gauche.transform.parent = RAIL.transform;

                GameObject route_droite = new GameObject();
                route_droite.transform.parent = RAIL.transform;

                GameObject rail_gauche_gauche = new GameObject();
                rail_gauche_gauche.transform.parent = RAIL.transform;

                GameObject rail_gauche_droite = new GameObject();
                rail_gauche_droite.transform.parent = RAIL.transform;

                GameObject rail_droite_gauche = new GameObject();
                rail_droite_gauche.transform.parent = RAIL.transform;

                GameObject rail_droite_droite = new GameObject();
                rail_droite_droite.transform.parent = RAIL.transform;

                rail_gauche_gauche.AddComponent<MeshFilter>();
                rail_gauche_gauche.AddComponent<MeshRenderer>();
                rail_gauche_gauche.GetComponent<MeshFilter>().mesh = mesh_rail_gauche_gauche;

                rail_gauche_droite.AddComponent<MeshFilter>();
                rail_gauche_droite.AddComponent<MeshRenderer>();
                rail_gauche_droite.GetComponent<MeshFilter>().mesh = mesh_rail_gauche_droite;

                rail_droite_gauche.AddComponent<MeshFilter>();
                rail_droite_gauche.AddComponent<MeshRenderer>();
                rail_droite_gauche.GetComponent<MeshFilter>().mesh = mesh_rail_droite_gauche;

                rail_droite_droite.AddComponent<MeshFilter>();
                rail_droite_droite.AddComponent<MeshRenderer>();
                rail_droite_droite.GetComponent<MeshFilter>().mesh = mesh_rail_droite_droite;

                route_gauche.AddComponent<MeshFilter>();
                route_gauche.AddComponent<MeshRenderer>();
                route_gauche.GetComponent<MeshFilter>().mesh = mesh_route_gauche;

                route_droite.AddComponent<MeshFilter>();
                route_droite.AddComponent<MeshRenderer>();
                route_droite.GetComponent<MeshFilter>().mesh = mesh_route_droite;

                Vector2 line_old_mid;
                Vector2 line_average;
                Vector2 line_mid_new;

                Vector2 perp_old_mid;
                Vector2 perp_average;
                Vector2 perp_mid_new;

                Vector3 decale_old_mid_bas;
                Vector3 decale_old_mid_haut;
                Vector3 decale_average_bas;
                Vector3 decale_average_haut;
                Vector3 decale_mid_new_bas;
                Vector3 decale_mid_new_haut;

                Vector3 perp_old_mid_neutral;
                Vector3 perp_average_neutral;
                Vector3 perp_mid_new_neutral;

                Vector3 decale_double_route_old_mid;
                Vector3 decale_double_route_average;
                Vector3 decale_double_route_mid_new;

                Vector3 decale_double_rail_old_mid;
                Vector3 decale_double_rail_average;
                Vector3 decale_double_rail_mid_new;

                if (nb_points == 2)
                {
                    old_trans = trackPoints.transform.GetChild(0);
                    mid_trans = trackPoints.transform.GetChild(1);
                    between_old_mid = old_trans.position - mid_trans.position;
                    line_old_mid = new Vector2(old_trans.position.x - mid_trans.position.x, old_trans.position.z - mid_trans.position.z);
                    perp_old_mid = Vector2.Perpendicular(line_old_mid);
                    decale_old_mid_bas = new Vector3(perp_old_mid.x, 0, perp_old_mid.y);
                    decale_old_mid_bas = decale_old_mid_bas.normalized;
                    decale_old_mid_haut = decale_old_mid_bas;
                    decale_old_mid_bas = decale_old_mid_bas * largeur_route_bas;
                    decale_old_mid_haut = decale_old_mid_haut * largeur_route_haut;

                    perp_old_mid_neutral = new Vector3(perp_old_mid.x, 0, perp_old_mid.y);
                    decale_double_route_old_mid = perp_old_mid_neutral * espace_between_roads;
                    decale_double_rail_old_mid = perp_old_mid_neutral * espace_between_roads;

                    vertices_route_gauche[vertices_index_route + 0] = old_trans.position + decale_double_route_old_mid + decale_old_mid_bas;
                    vertices_route_gauche[vertices_index_route + 1] = old_trans.position + decale_double_route_old_mid - decale_old_mid_bas;
                    vertices_route_gauche[vertices_index_route + 2] = old_trans.position + decale_double_route_old_mid + decale_old_mid_haut + Vector3.up * hauteur_route;
                    vertices_route_gauche[vertices_index_route + 3] = old_trans.position + decale_double_route_old_mid - decale_old_mid_haut + Vector3.up * hauteur_route;

                    vertices_route_gauche[vertices_index_route + 4] = mid_trans.position + decale_double_route_old_mid + decale_old_mid_bas;
                    vertices_route_gauche[vertices_index_route + 5] = mid_trans.position + decale_double_route_old_mid - decale_old_mid_bas;
                    vertices_route_gauche[vertices_index_route + 6] = mid_trans.position + decale_double_route_old_mid + decale_old_mid_haut + Vector3.up * hauteur_route;
                    vertices_route_gauche[vertices_index_route + 7] = mid_trans.position + decale_double_route_old_mid - decale_old_mid_haut + Vector3.up * hauteur_route;

                    vertices_rail_gauche_gauche[vertices_index_rail + 0] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_gauche_gauche[vertices_index_rail + 1] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_gauche_gauche[vertices_index_rail + 2] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_gauche_gauche[vertices_index_rail + 3] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_gauche_gauche[vertices_index_rail + 4] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_gauche_gauche[vertices_index_rail + 5] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_gauche_gauche[vertices_index_rail + 6] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_gauche_gauche[vertices_index_rail + 7] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_gauche_gauche[vertices_index_rail + 8] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_gauche_gauche[vertices_index_rail + 9] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_gauche_gauche[vertices_index_rail + 10] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                    vertices_rail_gauche_gauche[vertices_index_rail + 11] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                    vertices_rail_gauche_gauche[vertices_index_rail + 12] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_gauche_gauche[vertices_index_rail + 13] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_gauche_gauche[vertices_index_rail + 14] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_gauche_gauche[vertices_index_rail + 15] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_gauche_gauche[vertices_index_rail + 16] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_gauche_gauche[vertices_index_rail + 17] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_gauche_gauche[vertices_index_rail + 18] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_gauche_gauche[vertices_index_rail + 19] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_gauche_gauche[vertices_index_rail + 20] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_gauche_gauche[vertices_index_rail + 21] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_gauche_gauche[vertices_index_rail + 22] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                    vertices_rail_gauche_gauche[vertices_index_rail + 23] = mid_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                    vertices_rail_gauche_droite[vertices_index_rail + 0] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_gauche_droite[vertices_index_rail + 1] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_gauche_droite[vertices_index_rail + 2] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_gauche_droite[vertices_index_rail + 3] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_gauche_droite[vertices_index_rail + 4] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_gauche_droite[vertices_index_rail + 5] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_gauche_droite[vertices_index_rail + 6] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_gauche_droite[vertices_index_rail + 7] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_gauche_droite[vertices_index_rail + 8] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_gauche_droite[vertices_index_rail + 9] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_gauche_droite[vertices_index_rail + 10] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                    vertices_rail_gauche_droite[vertices_index_rail + 11] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                    vertices_rail_gauche_droite[vertices_index_rail + 12] = mid_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_gauche_droite[vertices_index_rail + 13] = mid_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_gauche_droite[vertices_index_rail + 14] = mid_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_gauche_droite[vertices_index_rail + 15] = mid_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_gauche_droite[vertices_index_rail + 16] = mid_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_gauche_droite[vertices_index_rail + 17] = mid_trans.position - decale_double_rail_old_mid - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_gauche_droite[vertices_index_rail + 18] = mid_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_gauche_droite[vertices_index_rail + 19] = mid_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_gauche_droite[vertices_index_rail + 20] = mid_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_gauche_droite[vertices_index_rail + 21] = mid_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_gauche_droite[vertices_index_rail + 22] = mid_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                    vertices_rail_gauche_droite[vertices_index_rail + 23] = mid_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                    vertices_route_droite[vertices_index_route + 0] = old_trans.position - decale_double_route_old_mid + decale_old_mid_bas;
                    vertices_route_droite[vertices_index_route + 1] = old_trans.position - decale_double_route_old_mid - decale_old_mid_bas;
                    vertices_route_droite[vertices_index_route + 2] = old_trans.position - decale_double_route_old_mid + decale_old_mid_haut + Vector3.up * hauteur_route;
                    vertices_route_droite[vertices_index_route + 3] = old_trans.position - decale_double_route_old_mid - decale_old_mid_haut + Vector3.up * hauteur_route;

                    vertices_route_droite[vertices_index_route + 4] = mid_trans.position - decale_double_route_old_mid + decale_old_mid_bas;
                    vertices_route_droite[vertices_index_route + 5] = mid_trans.position - decale_double_route_old_mid - decale_old_mid_bas;
                    vertices_route_droite[vertices_index_route + 6] = mid_trans.position - decale_double_route_old_mid + decale_old_mid_haut + Vector3.up * hauteur_route;
                    vertices_route_droite[vertices_index_route + 7] = mid_trans.position - decale_double_route_old_mid - decale_old_mid_haut + Vector3.up * hauteur_route;

                    vertices_rail_droite_gauche[vertices_index_rail + 0] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_droite_gauche[vertices_index_rail + 1] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_droite_gauche[vertices_index_rail + 2] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_droite_gauche[vertices_index_rail + 3] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_droite_gauche[vertices_index_rail + 4] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_droite_gauche[vertices_index_rail + 5] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_droite_gauche[vertices_index_rail + 6] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_droite_gauche[vertices_index_rail + 7] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_droite_gauche[vertices_index_rail + 8] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_droite_gauche[vertices_index_rail + 9] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_droite_gauche[vertices_index_rail + 10] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                    vertices_rail_droite_gauche[vertices_index_rail + 11] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                    vertices_rail_droite_gauche[vertices_index_rail + 12] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_droite_gauche[vertices_index_rail + 13] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_droite_gauche[vertices_index_rail + 14] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_droite_gauche[vertices_index_rail + 15] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_droite_gauche[vertices_index_rail + 16] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_droite_gauche[vertices_index_rail + 17] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_droite_gauche[vertices_index_rail + 18] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_droite_gauche[vertices_index_rail + 19] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_droite_gauche[vertices_index_rail + 20] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_droite_gauche[vertices_index_rail + 21] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_droite_gauche[vertices_index_rail + 22] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                    vertices_rail_droite_gauche[vertices_index_rail + 23] = mid_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                    vertices_rail_droite_droite[vertices_index_rail + 0] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_droite_droite[vertices_index_rail + 1] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_droite_droite[vertices_index_rail + 2] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_droite_droite[vertices_index_rail + 3] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_droite_droite[vertices_index_rail + 4] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_droite_droite[vertices_index_rail + 5] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_droite_droite[vertices_index_rail + 6] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_droite_droite[vertices_index_rail + 7] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_droite_droite[vertices_index_rail + 8] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_droite_droite[vertices_index_rail + 9] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_droite_droite[vertices_index_rail + 10] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                    vertices_rail_droite_droite[vertices_index_rail + 11] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                    vertices_rail_droite_droite[vertices_index_rail + 12] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_droite_droite[vertices_index_rail + 13] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                    vertices_rail_droite_droite[vertices_index_rail + 14] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_droite_droite[vertices_index_rail + 15] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                    vertices_rail_droite_droite[vertices_index_rail + 16] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_droite_droite[vertices_index_rail + 17] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                    vertices_rail_droite_droite[vertices_index_rail + 18] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_droite_droite[vertices_index_rail + 19] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                    vertices_rail_droite_droite[vertices_index_rail + 20] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_droite_droite[vertices_index_rail + 21] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                    vertices_rail_droite_droite[vertices_index_rail + 22] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                    vertices_rail_droite_droite[vertices_index_rail + 23] = mid_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                }


                if (nb_points >= 3)
                {
                    for (int i = 0; i < nb_points - 2; i++)
                    {
                        old_trans = trackPoints.transform.GetChild(i);
                        mid_trans = trackPoints.transform.GetChild(i + 1);
                        new_trans = trackPoints.transform.GetChild(i + 2);

                        between_old_mid = old_trans.position - mid_trans.position;
                        between_mid_new = mid_trans.position - new_trans.position;

                        distance_old_mid = between_old_mid.magnitude;
                        distance_mid_new = between_mid_new.magnitude;

                        line_old_mid = new Vector2(old_trans.position.x - mid_trans.position.x, old_trans.position.z - mid_trans.position.z);
                        perp_old_mid = Vector2.Perpendicular(line_old_mid);

                        line_mid_new = new Vector2(mid_trans.position.x - new_trans.position.x, mid_trans.position.z - new_trans.position.z);
                        perp_mid_new = Vector2.Perpendicular(line_mid_new);

                        line_average = (line_old_mid + line_mid_new) / 2;
                        perp_average = Vector2.Perpendicular(line_average);

                        decale_old_mid_bas = new Vector3(perp_old_mid.x, 0, perp_old_mid.y);
                        decale_mid_new_bas = new Vector3(perp_mid_new.x, 0, perp_mid_new.y);
                        decale_average_bas = new Vector3(perp_average.x, 0, perp_average.y);

                        decale_old_mid_bas = decale_old_mid_bas.normalized;
                        decale_mid_new_bas = decale_mid_new_bas.normalized;
                        decale_average_bas = decale_average_bas.normalized;

                        decale_old_mid_haut = decale_old_mid_bas;
                        decale_mid_new_haut = decale_mid_new_bas;
                        decale_average_haut = decale_average_bas;

                        decale_old_mid_bas = decale_old_mid_bas * largeur_route_bas;
                        decale_mid_new_bas = decale_mid_new_bas * largeur_route_bas;
                        decale_average_bas = decale_average_bas * largeur_route_bas;

                        decale_old_mid_haut = decale_old_mid_haut * largeur_route_haut;
                        decale_mid_new_haut = decale_mid_new_haut * largeur_route_haut;
                        decale_average_haut = decale_average_haut * largeur_route_haut;

                        perp_old_mid_neutral = new Vector3(perp_old_mid.x, 0, perp_old_mid.y);
                        perp_average_neutral = new Vector3(perp_average.x, 0, perp_average.y);
                        perp_mid_new_neutral = new Vector3(perp_old_mid.x, 0, perp_mid_new.y);

                        decale_double_route_old_mid = perp_old_mid_neutral * espace_between_roads;
                        decale_double_route_average = perp_average_neutral * espace_between_roads;
                        decale_double_route_mid_new = perp_mid_new_neutral * espace_between_roads;

                        decale_double_rail_old_mid = perp_old_mid_neutral * espace_between_tracks;
                        decale_double_rail_average = perp_average_neutral * espace_between_tracks;
                        decale_double_rail_mid_new = perp_mid_new_neutral * espace_between_tracks;

                        vertices_route_gauche[vertices_index_route + 0] = old_trans.position + decale_double_route_old_mid + decale_old_mid_bas;
                        vertices_route_gauche[vertices_index_route + 1] = old_trans.position + decale_double_route_old_mid - decale_old_mid_bas;
                        vertices_route_gauche[vertices_index_route + 2] = old_trans.position + decale_double_route_old_mid + decale_old_mid_haut + Vector3.up * hauteur_route;
                        vertices_route_gauche[vertices_index_route + 3] = old_trans.position + decale_double_route_old_mid - decale_old_mid_haut + Vector3.up * hauteur_route;

                        vertices_route_gauche[vertices_index_route + 4] = mid_trans.position + decale_double_route_average + decale_average_bas;
                        vertices_route_gauche[vertices_index_route + 5] = mid_trans.position + decale_double_route_average - decale_average_bas;
                        vertices_route_gauche[vertices_index_route + 6] = mid_trans.position + decale_double_route_average + decale_average_haut + Vector3.up * hauteur_route;
                        vertices_route_gauche[vertices_index_route + 7] = mid_trans.position + decale_double_route_average - decale_average_haut + Vector3.up * hauteur_route;

                        vertices_route_gauche[vertices_index_route + 8] = new_trans.position + decale_double_route_mid_new + decale_mid_new_bas;
                        vertices_route_gauche[vertices_index_route + 9] = new_trans.position + decale_double_route_mid_new - decale_mid_new_bas;
                        vertices_route_gauche[vertices_index_route + 10] = new_trans.position + decale_double_route_mid_new + decale_mid_new_haut + Vector3.up * hauteur_route;
                        vertices_route_gauche[vertices_index_route + 11] = new_trans.position + decale_double_route_mid_new - decale_mid_new_haut + Vector3.up * hauteur_route;

                        vertices_route_droite[vertices_index_route + 0] = old_trans.position - decale_double_route_old_mid + decale_old_mid_bas;
                        vertices_route_droite[vertices_index_route + 1] = old_trans.position - decale_double_route_old_mid - decale_old_mid_bas;
                        vertices_route_droite[vertices_index_route + 2] = old_trans.position - decale_double_route_old_mid + decale_old_mid_haut + Vector3.up * hauteur_route;
                        vertices_route_droite[vertices_index_route + 3] = old_trans.position - decale_double_route_old_mid - decale_old_mid_haut + Vector3.up * hauteur_route;

                        vertices_route_droite[vertices_index_route + 4] = mid_trans.position - decale_double_route_average + decale_average_bas;
                        vertices_route_droite[vertices_index_route + 5] = mid_trans.position - decale_double_route_average - decale_average_bas;
                        vertices_route_droite[vertices_index_route + 6] = mid_trans.position - decale_double_route_average + decale_average_haut + Vector3.up * hauteur_route;
                        vertices_route_droite[vertices_index_route + 7] = mid_trans.position - decale_double_route_average - decale_average_haut + Vector3.up * hauteur_route;

                        vertices_route_droite[vertices_index_route + 8] = new_trans.position - decale_double_route_mid_new + decale_mid_new_bas;
                        vertices_route_droite[vertices_index_route + 9] = new_trans.position - decale_double_route_mid_new - decale_mid_new_bas;
                        vertices_route_droite[vertices_index_route + 10] = new_trans.position - decale_double_route_mid_new + decale_mid_new_haut + Vector3.up * hauteur_route;
                        vertices_route_droite[vertices_index_route + 11] = new_trans.position - decale_double_route_mid_new - decale_mid_new_haut + Vector3.up * hauteur_route;

                        vertices_index_route += 4;
                        GameObject poutre_gauche = Instantiate(poutre, trackPoints.transform.GetChild(i).position + new Vector3(0, 0.14f, 0) + decale_double_route_old_mid, Quaternion.LookRotation(new Vector3(perp_old_mid.x, 0, perp_old_mid.y), Vector3.up));
                        poutre_gauche.transform.Rotate(new Vector3(0, 90, 0));
                        poutre_gauche.transform.parent = route_gauche.transform;

                        GameObject poutre_droite = Instantiate(poutre, trackPoints.transform.GetChild(i).position + new Vector3(0, 0.14f, 0) - decale_double_route_old_mid, Quaternion.LookRotation(new Vector3(perp_old_mid.x, 0, perp_old_mid.y), Vector3.up));
                        poutre_droite.transform.Rotate(new Vector3(0, 90, 0));
                        poutre_droite.transform.parent = route_droite.transform;

                        vertices_rail_gauche_gauche[vertices_index_rail + 0] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 1] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 2] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 3] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 4] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_gauche[vertices_index_rail + 5] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_gauche[vertices_index_rail + 6] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_gauche[vertices_index_rail + 7] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_gauche[vertices_index_rail + 8] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_gauche[vertices_index_rail + 9] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_gauche[vertices_index_rail + 10] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_gauche_gauche[vertices_index_rail + 11] = old_trans.position + decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_rail_gauche_gauche[vertices_index_rail + 12] = mid_trans.position + decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 13] = mid_trans.position + decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 14] = mid_trans.position + decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 15] = mid_trans.position + decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 16] = mid_trans.position + decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_gauche[vertices_index_rail + 17] = mid_trans.position + decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_gauche[vertices_index_rail + 18] = mid_trans.position + decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_gauche[vertices_index_rail + 19] = mid_trans.position + decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_gauche[vertices_index_rail + 20] = mid_trans.position + decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_gauche[vertices_index_rail + 21] = mid_trans.position + decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_gauche[vertices_index_rail + 22] = mid_trans.position + decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_gauche_gauche[vertices_index_rail + 23] = mid_trans.position + decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_rail_gauche_gauche[vertices_index_rail + 24] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 25] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 26] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 27] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_gauche[vertices_index_rail + 28] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_gauche[vertices_index_rail + 29] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_gauche[vertices_index_rail + 30] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_gauche[vertices_index_rail + 31] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_gauche[vertices_index_rail + 32] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_gauche[vertices_index_rail + 33] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_gauche[vertices_index_rail + 34] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_gauche_gauche[vertices_index_rail + 35] = new_trans.position + decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_rail_gauche_droite[vertices_index_rail + 0] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 1] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 2] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 3] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 4] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_droite[vertices_index_rail + 5] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_droite[vertices_index_rail + 6] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_droite[vertices_index_rail + 7] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_droite[vertices_index_rail + 8] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_droite[vertices_index_rail + 9] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_droite[vertices_index_rail + 10] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_gauche_droite[vertices_index_rail + 11] = old_trans.position - decale_double_rail_old_mid + decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_rail_gauche_droite[vertices_index_rail + 12] = mid_trans.position - decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 13] = mid_trans.position - decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 14] = mid_trans.position - decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 15] = mid_trans.position - decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 16] = mid_trans.position - decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_droite[vertices_index_rail + 17] = mid_trans.position - decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_droite[vertices_index_rail + 18] = mid_trans.position - decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_droite[vertices_index_rail + 19] = mid_trans.position - decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_droite[vertices_index_rail + 20] = mid_trans.position - decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_droite[vertices_index_rail + 21] = mid_trans.position - decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_droite[vertices_index_rail + 22] = mid_trans.position - decale_double_rail_average + decale_double_route_average + perp_average_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_gauche_droite[vertices_index_rail + 23] = mid_trans.position - decale_double_rail_average + decale_double_route_average - perp_average_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_rail_gauche_droite[vertices_index_rail + 24] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 25] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 26] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 27] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_gauche_droite[vertices_index_rail + 28] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_droite[vertices_index_rail + 29] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_gauche_droite[vertices_index_rail + 30] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_droite[vertices_index_rail + 31] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_gauche_droite[vertices_index_rail + 32] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_droite[vertices_index_rail + 33] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_gauche_droite[vertices_index_rail + 34] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_gauche_droite[vertices_index_rail + 35] = new_trans.position - decale_double_rail_mid_new + decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_rail_droite_gauche[vertices_index_rail + 0] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 1] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 2] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 3] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 4] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_gauche[vertices_index_rail + 5] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_gauche[vertices_index_rail + 6] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_gauche[vertices_index_rail + 7] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_gauche[vertices_index_rail + 8] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_gauche[vertices_index_rail + 9] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_gauche[vertices_index_rail + 10] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_droite_gauche[vertices_index_rail + 11] = old_trans.position + decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_rail_droite_gauche[vertices_index_rail + 12] = mid_trans.position + decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 13] = mid_trans.position + decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 14] = mid_trans.position + decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 15] = mid_trans.position + decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 16] = mid_trans.position + decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_gauche[vertices_index_rail + 17] = mid_trans.position + decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_gauche[vertices_index_rail + 18] = mid_trans.position + decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_gauche[vertices_index_rail + 19] = mid_trans.position + decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_gauche[vertices_index_rail + 20] = mid_trans.position + decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_gauche[vertices_index_rail + 21] = mid_trans.position + decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_gauche[vertices_index_rail + 22] = mid_trans.position + decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_droite_gauche[vertices_index_rail + 23] = mid_trans.position + decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_rail_droite_gauche[vertices_index_rail + 24] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 25] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 26] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 27] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_gauche[vertices_index_rail + 28] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_gauche[vertices_index_rail + 29] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_gauche[vertices_index_rail + 30] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_gauche[vertices_index_rail + 31] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_gauche[vertices_index_rail + 32] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_gauche[vertices_index_rail + 33] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_gauche[vertices_index_rail + 34] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_droite_gauche[vertices_index_rail + 35] = new_trans.position + decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_rail_droite_droite[vertices_index_rail + 0] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 1] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 2] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 3] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 4] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_droite[vertices_index_rail + 5] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_droite[vertices_index_rail + 6] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_droite[vertices_index_rail + 7] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_droite[vertices_index_rail + 8] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_droite[vertices_index_rail + 9] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_droite[vertices_index_rail + 10] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid + perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_droite_droite[vertices_index_rail + 11] = old_trans.position - decale_double_rail_old_mid - decale_double_route_old_mid - perp_old_mid_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_rail_droite_droite[vertices_index_rail + 12] = mid_trans.position - decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 13] = mid_trans.position - decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 14] = mid_trans.position - decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 15] = mid_trans.position - decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 16] = mid_trans.position - decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_droite[vertices_index_rail + 17] = mid_trans.position - decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_droite[vertices_index_rail + 18] = mid_trans.position - decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_droite[vertices_index_rail + 19] = mid_trans.position - decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_droite[vertices_index_rail + 20] = mid_trans.position - decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_droite[vertices_index_rail + 21] = mid_trans.position - decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_droite[vertices_index_rail + 22] = mid_trans.position - decale_double_rail_average - decale_double_route_average + perp_average_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_droite_droite[vertices_index_rail + 23] = mid_trans.position - decale_double_rail_average - decale_double_route_average - perp_average_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_rail_droite_droite[vertices_index_rail + 24] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 25] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 26] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 27] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_b + Vector3.up * hauteur_rail_a;
                        vertices_rail_droite_droite[vertices_index_rail + 28] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_droite[vertices_index_rail + 29] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_c + Vector3.up * hauteur_rail_b;
                        vertices_rail_droite_droite[vertices_index_rail + 30] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_droite[vertices_index_rail + 31] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_d + Vector3.up * hauteur_rail_c;
                        vertices_rail_droite_droite[vertices_index_rail + 32] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_droite[vertices_index_rail + 33] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_e + Vector3.up * hauteur_rail_d;
                        vertices_rail_droite_droite[vertices_index_rail + 34] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new + perp_mid_new_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;
                        vertices_rail_droite_droite[vertices_index_rail + 35] = new_trans.position - decale_double_rail_mid_new - decale_double_route_mid_new - perp_mid_new_neutral * largeur_rail_f + Vector3.up * hauteur_rail_e;

                        vertices_index_rail += 12;
                    }
                }
                int[] triangles_route = new int[12 * (nb_points - 1) * 3];
                int[] triangles_rail = new int[24 * (nb_points - 1) * 3]; //Attention, il n'y a pas les trianles sur les faces avant et arrière des rails 

                int tri_index_route = 0;
                int vert_index_route = 0;

                int tri_index_rail = 0;
                int vert_index_rail = 0;

                for (int i = 0; i < nb_points - 1; i++)
                {
                    //route
                    //TODO : PENSER A ENLEVER LA FACE AVANT ET LA FACE ARRIERE POUR GAGNER EN TRIANGLES
                    //Dessous
                    triangles_route[tri_index_route + 0] = vert_index_route + 0;
                    triangles_route[tri_index_route + 1] = vert_index_route + 5;
                    triangles_route[tri_index_route + 2] = vert_index_route + 1;

                    triangles_route[tri_index_route + 3] = vert_index_route + 0;
                    triangles_route[tri_index_route + 4] = vert_index_route + 4;
                    triangles_route[tri_index_route + 5] = vert_index_route + 5;

                    //Dessus
                    triangles_route[tri_index_route + 6] = vert_index_route + 2;
                    triangles_route[tri_index_route + 7] = vert_index_route + 3;
                    triangles_route[tri_index_route + 8] = vert_index_route + 7;

                    triangles_route[tri_index_route + 9] = vert_index_route + 2;
                    triangles_route[tri_index_route + 10] = vert_index_route + 7;
                    triangles_route[tri_index_route + 11] = vert_index_route + 6;

                    //face avant
                    triangles_route[tri_index_route + 12] = vert_index_route + 0;
                    triangles_route[tri_index_route + 13] = vert_index_route + 1;
                    triangles_route[tri_index_route + 14] = vert_index_route + 3;

                    triangles_route[tri_index_route + 15] = vert_index_route + 0;
                    triangles_route[tri_index_route + 16] = vert_index_route + 3;
                    triangles_route[tri_index_route + 17] = vert_index_route + 2;

                    //face arrière
                    triangles_route[tri_index_route + 18] = vert_index_route + 5;
                    triangles_route[tri_index_route + 19] = vert_index_route + 4;
                    triangles_route[tri_index_route + 20] = vert_index_route + 7;

                    triangles_route[tri_index_route + 21] = vert_index_route + 4;
                    triangles_route[tri_index_route + 22] = vert_index_route + 6;
                    triangles_route[tri_index_route + 23] = vert_index_route + 7;

                    //Côté droit
                    triangles_route[tri_index_route + 24] = vert_index_route + 1;
                    triangles_route[tri_index_route + 25] = vert_index_route + 5;
                    triangles_route[tri_index_route + 26] = vert_index_route + 3;

                    triangles_route[tri_index_route + 27] = vert_index_route + 3;
                    triangles_route[tri_index_route + 28] = vert_index_route + 5;
                    triangles_route[tri_index_route + 29] = vert_index_route + 7;

                    //Côté gauche
                    triangles_route[tri_index_route + 30] = vert_index_route + 4;
                    triangles_route[tri_index_route + 31] = vert_index_route + 0;
                    triangles_route[tri_index_route + 32] = vert_index_route + 2;

                    triangles_route[tri_index_route + 33] = vert_index_route + 4;
                    triangles_route[tri_index_route + 34] = vert_index_route + 2;
                    triangles_route[tri_index_route + 35] = vert_index_route + 6;

                    vert_index_route += 4;
                    tri_index_route += 36;

                    //rail
                    //Dessous
                    triangles_rail[tri_index_rail + 0] = vert_index_rail + 0;
                    triangles_rail[tri_index_rail + 1] = vert_index_rail + 12;
                    triangles_rail[tri_index_rail + 2] = vert_index_rail + 1;

                    triangles_rail[tri_index_rail + 3] = vert_index_rail + 1;
                    triangles_rail[tri_index_rail + 4] = vert_index_rail + 12;
                    triangles_rail[tri_index_rail + 5] = vert_index_rail + 13;

                    //Dessus
                    triangles_rail[tri_index_rail + 6] = vert_index_rail + 10;
                    triangles_rail[tri_index_rail + 7] = vert_index_rail + 23;
                    triangles_rail[tri_index_rail + 8] = vert_index_rail + 22;

                    triangles_rail[tri_index_rail + 9] = vert_index_rail + 10;
                    triangles_rail[tri_index_rail + 10] = vert_index_rail + 11;
                    triangles_rail[tri_index_rail + 11] = vert_index_rail + 23;

                    //Côté droit
                    triangles_rail[tri_index_rail + 12] = vert_index_rail + 1;
                    triangles_rail[tri_index_rail + 13] = vert_index_rail + 15;
                    triangles_rail[tri_index_rail + 14] = vert_index_rail + 3;

                    triangles_rail[tri_index_rail + 15] = vert_index_rail + 1;
                    triangles_rail[tri_index_rail + 16] = vert_index_rail + 13;
                    triangles_rail[tri_index_rail + 17] = vert_index_rail + 15;

                    triangles_rail[tri_index_rail + 18] = vert_index_rail + 3;
                    triangles_rail[tri_index_rail + 19] = vert_index_rail + 17;
                    triangles_rail[tri_index_rail + 20] = vert_index_rail + 5;

                    triangles_rail[tri_index_rail + 21] = vert_index_rail + 3;
                    triangles_rail[tri_index_rail + 22] = vert_index_rail + 15;
                    triangles_rail[tri_index_rail + 23] = vert_index_rail + 17;

                    triangles_rail[tri_index_rail + 24] = vert_index_rail + 5;
                    triangles_rail[tri_index_rail + 25] = vert_index_rail + 19;
                    triangles_rail[tri_index_rail + 26] = vert_index_rail + 7;

                    triangles_rail[tri_index_rail + 27] = vert_index_rail + 5;
                    triangles_rail[tri_index_rail + 28] = vert_index_rail + 17;
                    triangles_rail[tri_index_rail + 29] = vert_index_rail + 19;

                    triangles_rail[tri_index_rail + 30] = vert_index_rail + 7;
                    triangles_rail[tri_index_rail + 31] = vert_index_rail + 21;
                    triangles_rail[tri_index_rail + 32] = vert_index_rail + 9;

                    triangles_rail[tri_index_rail + 33] = vert_index_rail + 7;
                    triangles_rail[tri_index_rail + 34] = vert_index_rail + 19;
                    triangles_rail[tri_index_rail + 35] = vert_index_rail + 21;

                    triangles_rail[tri_index_rail + 36] = vert_index_rail + 9;
                    triangles_rail[tri_index_rail + 37] = vert_index_rail + 23;
                    triangles_rail[tri_index_rail + 38] = vert_index_rail + 11;

                    triangles_rail[tri_index_rail + 39] = vert_index_rail + 9;
                    triangles_rail[tri_index_rail + 40] = vert_index_rail + 21;
                    triangles_rail[tri_index_rail + 41] = vert_index_rail + 23;

                    //Côté gauche
                    triangles_rail[tri_index_rail + 42] = vert_index_rail + 12;
                    triangles_rail[tri_index_rail + 43] = vert_index_rail + 2;
                    triangles_rail[tri_index_rail + 44] = vert_index_rail + 14;

                    triangles_rail[tri_index_rail + 45] = vert_index_rail + 12;
                    triangles_rail[tri_index_rail + 46] = vert_index_rail + 0;
                    triangles_rail[tri_index_rail + 47] = vert_index_rail + 2;

                    triangles_rail[tri_index_rail + 48] = vert_index_rail + 14;
                    triangles_rail[tri_index_rail + 49] = vert_index_rail + 4;
                    triangles_rail[tri_index_rail + 50] = vert_index_rail + 16;

                    triangles_rail[tri_index_rail + 51] = vert_index_rail + 14;
                    triangles_rail[tri_index_rail + 52] = vert_index_rail + 2;
                    triangles_rail[tri_index_rail + 53] = vert_index_rail + 4;

                    triangles_rail[tri_index_rail + 54] = vert_index_rail + 16;
                    triangles_rail[tri_index_rail + 55] = vert_index_rail + 6;
                    triangles_rail[tri_index_rail + 56] = vert_index_rail + 18;

                    triangles_rail[tri_index_rail + 57] = vert_index_rail + 16;
                    triangles_rail[tri_index_rail + 58] = vert_index_rail + 4;
                    triangles_rail[tri_index_rail + 59] = vert_index_rail + 6;

                    triangles_rail[tri_index_rail + 60] = vert_index_rail + 18;
                    triangles_rail[tri_index_rail + 61] = vert_index_rail + 8;
                    triangles_rail[tri_index_rail + 62] = vert_index_rail + 20;

                    triangles_rail[tri_index_rail + 63] = vert_index_rail + 18;
                    triangles_rail[tri_index_rail + 64] = vert_index_rail + 6;
                    triangles_rail[tri_index_rail + 65] = vert_index_rail + 8;

                    triangles_rail[tri_index_rail + 66] = vert_index_rail + 20;
                    triangles_rail[tri_index_rail + 67] = vert_index_rail + 10;
                    triangles_rail[tri_index_rail + 68] = vert_index_rail + 22;

                    triangles_rail[tri_index_rail + 69] = vert_index_rail + 20;
                    triangles_rail[tri_index_rail + 70] = vert_index_rail + 8;
                    triangles_rail[tri_index_rail + 71] = vert_index_rail + 10;

                    tri_index_rail += 72;
                    vert_index_rail += 12;

                }

                //UV mapping
                //route
                for (int i = 0; i < uvs_route.Length; i++)
                {
                    uvs_route[i] = new Vector2(vertices_route_gauche[i].x, vertices_route_gauche[i].z);
                }

                //rail
                for (int i = 0; i < uvs_rail.Length; i++)
                {
                    uvs_rail[i] = new Vector2(vertices_rail_gauche_gauche[i].x, vertices_rail_gauche_gauche[i].z);
                }

                route_gauche.GetComponent<MeshRenderer>().material = trackMat[0];
                route_gauche.AddComponent<MeshCollider>();

                route_droite.GetComponent<MeshRenderer>().material = trackMat[0];
                route_droite.AddComponent<MeshCollider>();

                rail_gauche_gauche.GetComponent<MeshRenderer>().material = trackMat[1];
                rail_gauche_gauche.AddComponent<MeshCollider>();
                rail_gauche_gauche.transform.Translate(new Vector3(0, 0.22f, 0));

                rail_gauche_droite.GetComponent<MeshRenderer>().material = trackMat[1];
                rail_gauche_droite.AddComponent<MeshCollider>();
                rail_gauche_droite.transform.Translate(new Vector3(0, 0.22f, 0));

                rail_droite_gauche.GetComponent<MeshRenderer>().material = trackMat[1];
                rail_droite_gauche.AddComponent<MeshCollider>();
                rail_droite_gauche.transform.Translate(new Vector3(0, 0.22f, 0));

                rail_droite_droite.GetComponent<MeshRenderer>().material = trackMat[1];
                rail_droite_droite.AddComponent<MeshCollider>();
                rail_droite_droite.transform.Translate(new Vector3(0, 0.22f, 0));

                //route_gauche.AddComponent<RendererTimer>();
                //route_droite.AddComponent<RendererTimer>();
                //rail_gauche_gauche.AddComponent<RendererTimer>();
                //rail_gauche_droite.AddComponent<RendererTimer>();
                //rail_droite_gauche.AddComponent<RendererTimer>();
                //rail_droite_droite.AddComponent<RendererTimer>();

                mesh_route_gauche.Clear();
                mesh_route_gauche.name = "routeMesh";
                mesh_route_gauche.vertices = vertices_route_gauche;
                mesh_route_gauche.triangles = triangles_route;
                mesh_route_gauche.uv = uvs_route;
                mesh_route_gauche.RecalculateNormals();

                mesh_rail_gauche_gauche.Clear();
                mesh_rail_gauche_gauche.name = "railMesh";
                mesh_rail_gauche_gauche.vertices = vertices_rail_gauche_gauche;
                mesh_rail_gauche_gauche.triangles = triangles_rail;
                mesh_rail_gauche_gauche.uv = uvs_rail;
                mesh_rail_gauche_gauche.RecalculateNormals();

                mesh_rail_gauche_droite.Clear();
                mesh_rail_gauche_droite.name = "railMesh";
                mesh_rail_gauche_droite.vertices = vertices_rail_gauche_droite;
                mesh_rail_gauche_droite.triangles = triangles_rail;
                mesh_rail_gauche_droite.uv = uvs_rail;
                mesh_rail_gauche_droite.RecalculateNormals();

                mesh_route_droite.Clear();
                mesh_route_droite.name = "routeMesh";
                mesh_route_droite.vertices = vertices_route_droite;
                mesh_route_droite.triangles = triangles_route;
                mesh_route_droite.uv = uvs_route;
                mesh_route_droite.RecalculateNormals();

                mesh_rail_droite_gauche.Clear();
                mesh_rail_droite_gauche.name = "railMesh";
                mesh_rail_droite_gauche.vertices = vertices_rail_droite_gauche;
                mesh_rail_droite_gauche.triangles = triangles_rail;
                mesh_rail_droite_gauche.uv = uvs_rail;
                mesh_rail_droite_gauche.RecalculateNormals();

                mesh_rail_droite_droite.Clear();
                mesh_rail_droite_droite.name = "railMesh";
                mesh_rail_droite_droite.vertices = vertices_rail_droite_droite;
                mesh_rail_droite_droite.triangles = triangles_rail;
                mesh_rail_droite_droite.uv = uvs_rail;
                mesh_rail_droite_droite.RecalculateNormals();

                Destroy(trackPoints);
                //RAIL.transform.parent = All_tracks.transform;
            }
        }
    }


}

