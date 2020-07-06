using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe implantant l'occlusion culling pour des niveaux procéduraux. 
/// Classe fonctionnelle mais non utilisait dnas le projet finale;
/// Habituellement, unity permet de gérer ça très bien avec un système de bake.
/// Néanmoins, la map est générée au lancement de l'application. J'ai voulu implémanter une méthode, mais celle-ci
/// manque d'efficacité : le gain que l'on gagne en performance côté rendering est perdu à cause des raycast et 
/// de la physique (il faut un nombre important de rayon pour éviter le clipping)
/// </summary>
public class CameraRayCast : MonoBehaviour
{

    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        StartCoroutine(SendRaycast());
    }
    /// <summary>
    /// La méthode consiste à envoyer un grand nombre de rayon toutes les 0.1 secondes à travers tout 
    /// l'écran et au-delà (pour éviter le clipping). Chaque objet possèe un timer qui lui est propre
    /// (voir script RendererTimer). Si le timer tombe à 0, on disable le renderer de l'objet. Si l'objet 
    /// est touché par un rayon on reset son timer. Cela demande beaucoup de calcul en terme de physique,
    /// surtout si l'on veut avoir un résultat correcte (sans clipping, etc).
    /// </summary>
    /// <returns></returns>
    IEnumerator SendRaycast()
    {
        Ray ray = new Ray();
        RaycastHit hit;
        while (true)
        {
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    float pixeli = (float)i / 45;
                    float pixelj = (float)j / 45;
                    ray = cam.ViewportPointToRay(new Vector3(pixeli, pixelj, 0));
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.transform.gameObject.GetComponent<RendererTimer>() != null)
                        {
                            hit.transform.gameObject.GetComponent<RendererTimer>().timer = 5f;
                        }
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

    }
}
