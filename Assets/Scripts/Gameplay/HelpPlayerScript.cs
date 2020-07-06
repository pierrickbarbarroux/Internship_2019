using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe sert à aider le joueur à l'intérieur du georoom en rajoutant des flèche bleues au dessus des cubes 
/// sur lesquels est marqué le code qui permet d'ouvrir le coffre sous l'escalier.
/// </summary>
public class HelpPlayerScript : MonoBehaviour
{
    [Tooltip("Ensemble des cubes possédant le code du coffre")]
    public GameObject[] cubes_with_codes;

    [Tooltip("Modèle de la flèche qui apparaitra au dessus de chaque cube codé")]
    public GameObject arrow;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(HelpPlayerWithArrows());
    }

    /// <summary>
    /// Au bout de 120 secondes (après que la fonction aie été appelée), des objets 'arrow' apparaissent au dessus 
    /// de tous les objets référencés dans 'cubes_with_codes'
    /// </summary>
    /// <returns></returns>
    IEnumerator HelpPlayerWithArrows()
    {
        yield return new WaitForSecondsRealtime(120f);
        foreach (GameObject cube in cubes_with_codes)
        {
            GameObject arrow_clone = Instantiate(arrow, cube.GetComponent<Renderer>().bounds.center + new Vector3(0, 1.5f, 0), Quaternion.identity);            
        }
    }
}
