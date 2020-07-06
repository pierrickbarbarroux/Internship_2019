using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ce script était surtout utilisé au début du stage, lorsque je devait mouvoir la caméra avec les flèches directionnelles
/// pour observer le terrain généré. Il n'est plus utile depuis qu'on est passé en VR.
/// </summary>
public class CameraController : MonoBehaviour
{
    private Vector3 lastMouse = new Vector3(255, 255, 255);

    public int position_x;
    public int position_z;
    public int old_position_x;
    public int old_position_z;
    public float old_position_y;
    public int tileMatrix;
    public int old_tileMatrix;


    // Start is called before the first frame update
    void Start()
    {
        tileMatrix = int.Parse(GameObject.Find("MeshMNT").GetComponent<Tile>().tileMatrix);
        old_position_y = GetComponent<Transform>().position.y;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        UpdatePosition();
    }

    /// <summary>
    /// A mettre dans l'update, permet de mouvoir la caméra
    /// flèches directionnelles ou ZQSD pour se déplacer
    /// Space bar pour s'élever
    /// Ctrl pour descendre
    /// </summary>
    void MoveCamera()
    {
        if (Input.GetKey("space"))
        {
            transform.position += Vector3.up * 400 * Time.deltaTime;
        }

        if (Input.GetKey("left ctrl"))
        {
            transform.position += Vector3.down * 400 * Time.deltaTime;
        }

        Vector3 moveDir = Vector3.zero;
        moveDir.x = Input.GetAxis("Horizontal");
        moveDir.z = Input.GetAxis("Vertical");
        transform.Translate(moveDir * 600 * Time.deltaTime, Space.Self);

        lastMouse = Input.mousePosition - lastMouse;
        lastMouse = new Vector3(-lastMouse.y * 0.25f, lastMouse.x * 0.25f, 0);
        lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
        transform.eulerAngles = lastMouse;
        lastMouse = Input.mousePosition;
    }

    /// <summary>
    /// Change la position de la caméra pour qu'elle corresponde aux coorodnnées des tuiles
    /// </summary>
    void UpdatePosition()
    {
        if (position_x != (int)(GetComponent<Transform>().position.x) / 64)
        {
            old_position_x = position_x;
            position_x = (int)(GetComponent<Transform>().position.x) / 64;
        }
        if (position_z != (int)(GetComponent<Transform>().position.z) / 64)
        {
            old_position_z = position_z;
            position_z = (int)(GetComponent<Transform>().position.z) / 64;
        }
        if ((GetComponent<Transform>().position.y)/100 >= tileMatrix)
        {
            old_position_y = GetComponent<Transform>().position.y;
            tileMatrix += 1;
        }
        if ((GetComponent<Transform>().position.y)/100 <= tileMatrix)
        {
            old_position_y = GetComponent<Transform>().position.y;
            tileMatrix += -1;
        }
    }

    

}
