using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


/// <summary>
/// Classe de test. Teste les différents mode de vibration des controlleurs.
/// Ne sert à rien dans le projet final
/// </summary>
public class HapticsController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SteamVR_Actions._default.Haptic.Execute(10f, 1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);
        SteamVR_Actions._default.Haptic.Execute(13f, 0.1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);
        SteamVR_Actions._default.Haptic.Execute(13.5f, 0.1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);
        SteamVR_Actions._default.Haptic.Execute(14f, 0.1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);
        SteamVR_Actions._default.Haptic.Execute(14.5f, 0.1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);
        SteamVR_Actions._default.Haptic.Execute(15f, 0.1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);

    }

    // Update is called once per frame
    void Update()
    {
        if (SteamVR_Actions._default.GrabPinch.GetState(SteamVR_Input_Sources.LeftHand))
        {
            SteamVR_Actions._default.Haptic.Execute(0f, 1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);
            SteamVR_Actions._default.Haptic.Execute(13f, 0.1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);
            SteamVR_Actions._default.Haptic.Execute(13.5f, 0.1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);
            SteamVR_Actions._default.Haptic.Execute(14f, 0.1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);
            SteamVR_Actions._default.Haptic.Execute(14.5f, 0.1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);
            SteamVR_Actions._default.Haptic.Execute(15f, 0.1f, 20f, 20f, SteamVR_Input_Sources.LeftHand);
        }

    }
}
