using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;


/// <summary>
/// Classe gérant l'animation des clés une fois rentrée dans la boite contenant l'ordinateur.
/// J'ai remplacé ce scripte par une animation au niveau de la clé. Donc ce script est obsolète. 
/// </summary>
public class KeyAnimationController : MonoBehaviour
{
    Transform start;
    public GameObject stop;
    Vector3 direction;
    Vector3 rotation;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        start = GameObject.Find("key_position_when_activated").transform;

        Destroy(GetComponent<Throwable>());
        Destroy(GetComponent<VelocityEstimator>());
        Destroy(GetComponent<Interactable>());
        Destroy(GetComponent<Rigidbody>());
        transform.parent = start;
        stop = new GameObject();
        direction = new Vector3(-1f,-0.35f,0);
        rotation = new Vector3(5000f, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localPosition.y >= -0.45f)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }
}
