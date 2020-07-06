using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// Classe gérant l'affichage de l'altitude une fois passé en mode drone
/// </summary>
public class ShowAltitude : MonoBehaviour
{
    public Transform player;

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<TextMeshProUGUI>().text = "Altitude : " + player.position.y.ToString("F2");
    }
}
