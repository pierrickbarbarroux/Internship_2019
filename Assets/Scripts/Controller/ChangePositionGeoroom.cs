using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe gère la téléportation du géoroom au bon emplacement
/// </summary>
public class ChangePositionGeoroom : MonoBehaviour
{
    [Tooltip("Coordonnées du géoroom")]
    public Vector3 position_real_world;

    [Tooltip("Objet 'Player' de la scène")]
    public GameObject player;

    [Tooltip("Position où le jeu commence dans le géoroom")]
    public Transform start_point;

    [Tooltip("Dû à un bug avec le digicode, celui-ci doit être enlevé puis rajouté")]
    public GameObject code_panel;

    Vector3 position_in_scene;

    /// <summary>
    /// Change la position du georoom s'il peut être placé sur la bonne tuile
    /// </summary>
    public void ChangePositionGeo()
    {
        position_in_scene = new Vector3();
        //GameObject[] liste_MNT = GameObject.FindGameObjectsWithTag("Tile_tag");
        GameObject[] liste_MNT = GameObject.FindGameObjectsWithTag("Terrain_tag");

        //On parcourt tout les terrains pour voir si on trouve la tuile sur laquelle doit être placée le géoroom
        foreach (GameObject mnt in liste_MNT)
        {
            if (position_real_world.x >= mnt.GetComponent<Tile>().left_down_x && position_real_world.x <= mnt.GetComponent<Tile>().right_up_x && position_real_world.z >= mnt.GetComponent<Tile>().left_down_y && position_real_world.z <= mnt.GetComponent<Tile>().right_up_y)
            {
                Debug.Log(mnt.GetComponent<Tile>().position_x);
                Debug.Log(mnt.GetComponent<Tile>().position_z);
                position_in_scene.x += mnt.transform.position.x;
                position_in_scene.z += mnt.transform.position.z;
                position_in_scene.x -= position_real_world.z - mnt.GetComponent<Tile>().right_up_y;
                position_in_scene.z += position_real_world.x - mnt.GetComponent<Tile>().left_down_x;
                position_in_scene.y = position_real_world.y;


                //Ajustements pour coller à l'ortho
                position_in_scene += new Vector3(-1.26f, 2.42f, -11.287f);
                transform.position = position_in_scene;
                transform.localEulerAngles = new Vector3(0.077f, 23.424f, 1.889f);
            }
        }
        code_panel.SetActive(true);
    }

    /// <summary>
    /// Change la position du joueur pour qu'il soit bien placé au niveau du géoroom
    /// </summary>
    public void ChangePositionPlayer()
    {
        player.transform.position = start_point.position;
        player.GetComponent<PlayerController>().in_georoom = true;
    }

    /// <summary>
    /// Désactive le renderer des objets situés trop loin du géoroom. Cela permet de gagner des fps
    /// lorsque le joueur se trouve dans le géoroom.
    /// </summary>
    /// <param name="center"></param>
    public static void DisableFarRenderer(Transform center)
    {

        GameObject[] all_go = FindObjectsOfType<GameObject>();
        foreach (GameObject item in all_go)
        {
            if (item.GetComponent<Renderer>() != null)
            {
                Renderer renderer = item.GetComponent<Renderer>();
                Vector3 other = renderer.bounds.center;
                float distance = Vector3.Distance(center.position, other);
                if (distance >= 100)
                {
                    renderer.enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// Réactive le renderer des objets situés autour du géoroom
    /// </summary>
    /// <param name="center"></param>
    public static void EnableFarRenderer(Transform center)
    {
        GameObject[] all_go = FindObjectsOfType<GameObject>();
        foreach (GameObject item in all_go)
        {
            if (item.GetComponent<Renderer>() != null)
            {
                Renderer renderer = item.GetComponent<Renderer>();
                Vector3 other = renderer.bounds.center;
                float distance = Vector3.Distance(center.position, other);
                if (distance >= 100)
                {
                    renderer.enabled = true;
                }
            }
        }
    }
}
