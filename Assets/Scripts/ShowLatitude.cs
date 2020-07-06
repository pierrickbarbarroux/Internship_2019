using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Classe gérant l'affichage des northing une fois passé en mode drone
/// </summary>
public class ShowLatitude : MonoBehaviour
{
    public Transform player;
    public GameObject mnt;

    float left_bot_x;
    float latitude;
    // Start is called before the first frame update
    void Start()
    {
        left_bot_x = GameObject.Find("Terrain_X:0_Z:0").GetComponent<Tile>().left_down_y;
    }

    // Update is called once per frame
    void Update()
    {
        latitude = left_bot_x - player.position.x + 256;
        this.GetComponent<TextMeshProUGUI>().text = "N (m) : " + latitude.ToString("F2");
    }
}
