using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Update : Cette classe n'est plus utilisée
/// Classe gérant l'orientation d'une boussole vers différents objectifs.
/// On peut rentrer les différents objectifs via 'points'
/// L'objet auquel on attachera ce script s'orientera vers ces objectifs (les uns après les autres)
/// </summary>
public class Boussole : MonoBehaviour
{
    [Tooltip("Ensemble des objets que la boussole suivra")]
    public GameObject[] points;
    [Tooltip("Le joueur VR (l'objet dans la scène s'appelle VR)")]
    public GameObject player;
    [Tooltip("Canvas/texte où l'on affiche la distance nous séparant du prochain objectif")]
    public Text display;
    

    int index;
    float distance;
    GameObject next_point;

    // Start is called before the first frame update
    void Start()
    {
        //On initialise au premier objectif
        index = 0;
        next_point = points[0];
    }

    // Update is called once per frame
    void Update()
    {
        //On oriente la boussole vers le prochain point 
        transform.LookAt(next_point.transform);
        //On affiche la distance entre le joueur et l'objectif
        display.text = "Distance : " + ((int)distance).ToString();
        UpdateNextPoint();
    }

    /// <summary>
    /// Met à jour le prochain point si la distance entre le joueur et l'objectif est suffisamment faible
    /// </summary>
    void UpdateNextPoint()
    {
        distance  = Mathf.Abs((player.transform.position - next_point.transform.position).magnitude);
        if(distance<=2 && index!=points.Length-1){
            index++;
            next_point = points[index];
        }
    }
}
