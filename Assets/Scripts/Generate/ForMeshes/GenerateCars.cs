using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe n'ets pas utilisée dans le projet finale
/// Classe non terminée. Devait générer des voitures sur les routes pour plus de réalisme.
/// </summary>
public class GenerateCars : MonoBehaviour
{

    public GameObject carModel;
    GameObject car;
    Vector3 direction;
    public float speed;
    int id_car;

    // Start is called before the first frame update
    void Start()
    {
        id_car = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            car = Instantiate(carModel, GenerateRoad.roadPath[0][0], Quaternion.identity);
            car.name = "car" + id_car;
            id_car++;
            car.transform.LookAt(GenerateRoad.roadPath[0][1]);
            car.AddComponent<CarController>();
            car.GetComponent<CarController>().speed = speed;
            car.layer = 13;
            
        }
    }
}
