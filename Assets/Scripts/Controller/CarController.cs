using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère le déplacement des voitures sur la route. Ce script n'est pas utiliser dans la version final du projet
/// </summary>
public class CarController : MonoBehaviour
{
    [Tooltip("Vitesse des voitures")]
    public float speed;
    int i;
    int j;
    LayerMask myLayer;

    // Start is called before the first frame update
    void Start()
    {
        i = 0;
        j = 0;
        myLayer = 1 << 13;
    }

    /// <summary>
    /// Les voitures suivent un ensemble de points (transform) qui parcourt les routes.
    /// </summary>
    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);

        if (Physics.OverlapSphere(GenerateRoad.roadPath[i][j + 1], 0.8f, myLayer).Length!=0)
        {
            if (Physics.OverlapSphere(GenerateRoad.roadPath[i][j + 1], 0.8f, myLayer)[0].transform.gameObject == gameObject)
            {
                //si on arrive au bout de la route
                if (j+1 == GenerateRoad.roadPath[i].Length - 1)
                {
                    //si il n'y a plus de route
                    if (i == GenerateRoad.roadPath.Length - 1)
                    {
                        i = 0;
                        j = 0;
                        transform.position = GenerateRoad.roadPath[i][j];
                        transform.LookAt(GenerateRoad.roadPath[i][j + 1] + new Vector3(0, 0.35f, 0));
                    }
                    else
                    {
                        j = 0;
                        i++;
                        transform.position = GenerateRoad.roadPath[i][j];
                        transform.LookAt(GenerateRoad.roadPath[i][j + 1] + new Vector3(0, 0.35f, 0));
                    }
                    
                }
                //si on reste sur la même route
                else
                {
                    j++;
                    transform.LookAt(GenerateRoad.roadPath[i][j + 1] + new Vector3(0, 0.35f, 0));
                }

            }
        }
        
    }

}
