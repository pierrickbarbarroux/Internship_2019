using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// Class gérant le contôle des mains/controlleurs VR.
/// </summary>
public class HandController : MonoBehaviour
{
    [Tooltip("Le modèle de la boussole")]
    public GameObject compass;

    [Tooltip("Le modèle de la fiche géodésique")]
    public GameObject map;

    [Tooltip("La main gauche")]
    public GameObject leftHand;

    [Tooltip("La main droite")]
    public GameObject rightHand;

    [Tooltip("La vitesse lorsqu'on commande le drone")]
    public float speed;

    [Tooltip("Le collider de la tête du joueur")]
    public Collider headCollider;

    //Le joueur
    GameObject player;
    //La caméra VR
    GameObject cam;

    //La fiche géodésique
    GameObject myMap;

    //True si la boussole est vue, false sinon
    bool compass_is_seen;
    //True si la fiche géodésique est vue, false sinon
    bool map_is_seen;

    //La boussole
    GameObject myCompass;

    Vector3 direction;

    //Utilisé pour se téléporter au niveau du sol
    float chargement;


    // Start is called before the first frame update
    void Start()
    {
        //On initialise toutes nos variables
        chargement = 0;
        compass_is_seen = false;
        map_is_seen = false;
        direction = new Vector3();
        cam = transform.parent.GetChild(3).gameObject;
        player = GameObject.Find("Player");
    }

    /// <summary>
    /// On retrouve dans l'update tous les contrôles personnalisés des controlleurs VR
    /// </summary>
    // Update is called once per frame
    void Update()
    {
        if (player.GetComponent<PlayerController>().in_georoom == false)
        {
            //Téléportation au niveau du sol
            if (SteamVR_Actions._default.GrabPinch.GetState(SteamVR_Input_Sources.LeftHand) && SteamVR_Actions._default.GrabPinch.GetState(SteamVR_Input_Sources.RightHand))
            {
                if (chargement >= 100)
                {
                    chargement = 0;
                    RaycastHit hit;
                    if (Physics.Raycast(player.transform.position, Vector3.down, out hit))
                    {
                        player.transform.position = hit.point;
                    }
                }

                chargement += 0.6f;
                Debug.Log(chargement);
            }

            //Déplacement horizontal à l'exterieur du georoom
            if (SteamVR_Actions._default.GrabGrip.GetState(SteamVR_Input_Sources.LeftHand))
            {

                direction.x = Mathf.Sin(Mathf.Deg2Rad * leftHand.transform.eulerAngles.y);
                direction.y = 0;
                direction.z = Mathf.Cos(Mathf.Deg2Rad * leftHand.transform.eulerAngles.y);

                transform.parent.parent.Translate(direction * speed * Time.deltaTime);
            }

            //Déplacement vertical à l'exterieur du georoom
            if (SteamVR_Actions._default.GrabGrip.GetState(SteamVR_Input_Sources.RightHand))
            {
                //if (TilesGenerator.alt_max!=-10 && TilesGenerator.alt_min!=10000)
                //{
                //    if (player.transform.position.y<TilesGenerator.alt_max + 200 && player.transform.position.y>TilesGenerator.alt_min+2)
                //    {
                //        direction.x = 0;
                //        direction.y = -Mathf.Sin(Mathf.Deg2Rad * rightHand.transform.eulerAngles.x);
                //        direction.z = 0;

                //        transform.parent.parent.Translate(direction * speed * Time.deltaTime);
                //    }
                //}
                //else
                //{

                direction.x = 0;
                direction.y = -Mathf.Sin(Mathf.Deg2Rad * rightHand.transform.eulerAngles.x);
                direction.z = 0;

                // LE ZMIN et le ZMAX pour la visu drone est gérée à cet endroit là
                if (TilesGenerator.alt_max != -10 && TilesGenerator.alt_min != 10000)
                {
                    if (direction.y > 0 && player.transform.position.y >= TilesGenerator.alt_max + 200)
                    {
                        direction.y = 0;
                    }
                    else if (direction.y<0 && player.transform.position.y <= TilesGenerator.alt_min)
                    {
                        direction.y = 0;
                    }
                }

                transform.parent.parent.Translate(direction * speed * Time.deltaTime);
                //}
            }
        }

        //Ici, on peut faire apparaitre ou disparaitre la borne géodésique
        //Mais les dames de la communication voulait que la fiche reste, donc j'ai mis le tout en commentaire
        //Tout n'est pas utile cependant. Voir la partie sur la boussole (compass).
        if (SteamVR_Actions._default.Inventory.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            //if (!map_is_seen && transform.parent.transform.parent.gameObject.GetComponent<PlayerController>().hasMap)
            //{
            //    myMap = Instantiate(map, leftHand.transform);
            //    myMap.name = "map";
            //    map_is_seen = true;
            //}
            //if ((map_is_seen && !compass_is_seen) || (!transform.parent.transform.parent.gameObject.GetComponent<PlayerController>().hasMap && !compass_is_seen)) // montrer la boussole
            //{
            //    //On détruit la map
            //    if (map_is_seen)
            //    {
            //        Destroy(myMap);
            //        map_is_seen = false;
            //    }

            //    //On affiche la boussole
            //    myCompass = Instantiate(compass, leftHand.transform);
            //    myCompass.name = "compass";
            //    compass_is_seen = true;
            //}
            //if (compass_is_seen)
            //{
            //    Destroy(myCompass);
            //    compass_is_seen = false;
            //}
        }

        //On active ou on désactive la boussole
        if (SteamVR_Actions._default.Inventory.GetStateDown(SteamVR_Input_Sources.RightHand))
        {

            if (!compass_is_seen)
            {
                myCompass = Instantiate(compass, rightHand.transform);
                myCompass.name = "compass";
                compass_is_seen = true;
            }
            else
            {
                Destroy(myCompass);
                compass_is_seen = false;
            }
            //if (!map_is_seen && transform.parent.transform.parent.gameObject.GetComponent<PlayerController>().hasMap)
            //{
            //    myMap = Instantiate(map, rightHand.transform);
            //    myMap.name = "map";
            //    map_is_seen = true;
            //}

            //else if ((map_is_seen && !compass_is_seen) || (!transform.parent.transform.parent.gameObject.GetComponent<PlayerController>().hasMap && !compass_is_seen)) // montrer la boussole
            //{
            //    //On détruit la map
            //    if (map_is_seen)
            //    {
            //        Destroy(myMap);
            //        map_is_seen = false;
            //    }

            //    //On affiche la boussole
            //    myCompass = Instantiate(compass, rightHand.transform);
            //    myCompass.name = "compass";
            //    compass_is_seen = true;
            //}

            //else if (!map_is_seen && transform.parent.transform.parent.gameObject.GetComponent<PlayerController>().hasMap)
            //{
            //    myMap = Instantiate(map, rightHand.transform);
            //    myMap.name = "map";
            //    map_is_seen = true;
            //}
            //if (compass_is_seen)
            //{
            //    Destroy(myCompass);
            //    compass_is_seen = false;
            //}
        }

    }

    /// <summary>
    /// Fait appraitre la fiche géodésique
    /// </summary>
    public void ShowMap()
    {
        if (!map_is_seen && transform.parent.transform.parent.gameObject.GetComponent<PlayerController>().hasMap)
        {
            myMap = Instantiate(map, leftHand.transform);
            myMap.name = "map";
            GameObject.Find("CatchMe").gameObject.SetActive(false);
            map_is_seen = true;
        }
    }

    /// <summary>
    /// Fait disparaitre la fiche géodésique
    /// </summary>
    public void HideMap()
    {
        if (map_is_seen)
        {
            Destroy(myMap);
            map_is_seen = false;
        }
    }
}
