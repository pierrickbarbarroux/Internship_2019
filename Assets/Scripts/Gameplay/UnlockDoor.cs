using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// Cette classe n'est pas utilisée dans le projet finale
/// Classe gérant l'ouverture et la fermeture des portes. 
/// </summary>
public class UnlockDoor : MonoBehaviour
{

    public GameObject door;

    // Update is called once per frame
    void Update()
    {
        //if (door.GetComponent<CircularDrive>()!=null)
        //{
        //    if (door.GetComponent<CircularDrive>().enabled == false)
        //    {
        //        Debug.Log(this.transform.eulerAngles.z - 360);
        //        Debug.Log(this.transform.eulerAngles.z);
        //        if (this.transform.eulerAngles.z-360 < -40)
        //        {
        //            Debug.Log("fodhlfjdslkfjl");
        //            door.GetComponent<Interactable>().enabled = true;
        //            door.GetComponent<CircularDrive>().enabled = true;
        //        }
        //    }

        //    if (door.GetComponent<CircularDrive>().enabled == true)
        //    {
        //        Debug.Log(this.transform.eulerAngles.z - 360);
        //        Debug.Log(this.transform.eulerAngles.z);

        //        if (this.transform.eulerAngles.z-360 > -40)
        //        {
        //            door.GetComponent<CircularDrive>().enabled = false;
        //            door.GetComponent<Interactable>().enabled = false;

        //        }
        //    }
        //} 
    }

    /// <summary>
    /// Quand cette fonction est appelée, les portes peuvent être ouvertes
    /// </summary>
    public void ActivateCircularDrive()
    {
        door.GetComponent<Interactable>().enabled = true;
        door.GetComponent<CircularDrive>().enabled = true;
    }
}
