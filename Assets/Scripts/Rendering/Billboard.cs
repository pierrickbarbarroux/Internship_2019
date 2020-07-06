using UnityEngine;
using System.Collections;

/// <summary>
/// Cette classe ne semble pas être utile. Dans le doute, il ne vaut mieux pas la supprimer
/// </summary>
public class CameraFacingBillboard : MonoBehaviour
{
    public Camera m_Camera;

    //Orient the camera after all movement is completed this frame to avoid jittering
    void LateUpdate()
    {
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
            m_Camera.transform.rotation * Vector3.up);
    }
}