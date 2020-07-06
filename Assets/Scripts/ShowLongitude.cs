using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Classe gérant l'affichage des easting une fois passé en mode drone
/// </summary>
public class ShowLongitude : MonoBehaviour
{

    public Transform player;
    float left_bot_z;
    public GameObject mnt;

    float longitude;

    // Start is called before the first frame update
    void Start()
    {
        left_bot_z = GameObject.Find("Terrain_X:0_Z:0").GetComponent<Tile>().left_down_x;
    }

    // Update is called once per frame
    void Update()
    {
        longitude = left_bot_z + player.position.z;
        this.GetComponent<TextMeshProUGUI>().text = "E (m) : " +longitude.ToString("F2");
    }
}
