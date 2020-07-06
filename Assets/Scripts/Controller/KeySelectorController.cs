using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe gérant l'outil permettant de sélectionner les différentes clés
/// </summary>
public class KeySelectorController : MonoBehaviour
{
    [Tooltip("Le cylindre que l'on doit tourner pour choisir une clé")]
    public GameObject rouleau;

    [Tooltip("Le panneau de texte qui affiche la ville correspondante à la clé")]
    public GameObject text;

    [Tooltip("La liste des villes")]
    public string[] liste_villes;

    [Tooltip("Intervalle en degré entre chaque sélection de clé via le rouleau")]
    public int intervalle;

    [Tooltip("Le modèle de la clé")]
    public GameObject key;

    [Tooltip("Là où attéri la clé lorsqu'elle apparait")]
    public Transform key_parent;

    [Tooltip("Material (numéro 1) d'une clé ")]
    public Material keymat1;

    [Tooltip("Material (numéro 2) d'une clé")]
    public Material keymat2;

    //Clé actuel, celle qui est matérialisée dans le jeu à cet instant
    GameObject actual_key;

    //Ancien angle du cylindre. Nécessaire pour le calcul de l'intervalle
    float old_ang;

    int index;

    // Start is called before the first frame update
    void Start()
    {
        //On initialise nos variables et on trie les villes par ordre alphabétique
        Array.Sort(liste_villes, (x, y) => String.Compare(x, y));
        old_ang = rouleau.transform.localEulerAngles.y;
        index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //Si on a assez tourné le cylindre ...
        if (Mathf.Abs(rouleau.transform.localEulerAngles.y - old_ang)>=intervalle)
        {
            //Dans le sens alphabétique....
            if (rouleau.transform.localEulerAngles.y>old_ang)
            {
                //On détruit la clé à chaque fois qu'o fait apparaitre une nouvelle clé
                Destroy(actual_key);

                if (index==liste_villes.Length-1)
                {
                    index = 0;
                }
                else
                {
                    index += 1;
                }
                //On fait apparaitre le bon nom de ville et la clé qui va avec
                text.GetComponent<TextMesh>().text = liste_villes[index];
                old_ang = rouleau.transform.localEulerAngles.y;
                actual_key = Instantiate(key, key_parent);

                GetComponent<AudioSource>().Play();

                //La clé de Saint-Mandé est la bonne clé
                if (liste_villes[index]=="Saint-Mandé")
                {
                    actual_key.GetComponent<KeyController2>().is_good_key = true;
                }
                else
                {
                    actual_key.GetComponent<KeyController2>().is_good_key = false;
                }
            }

            //Dans l'autre sens...
            if (rouleau.transform.localEulerAngles.y < old_ang)
            {
                Destroy(actual_key);
                if (index ==0)
                {
                    index = liste_villes.Length - 1;
                }
                else
                {
                    index -= 1;
                }
                text.GetComponent<TextMesh>().text = liste_villes[index];
                old_ang = rouleau.transform.localEulerAngles.y;
                actual_key = Instantiate(key, key_parent);
                GetComponent<AudioSource>().Play();
                if (liste_villes[index] == "Saint-Mandé")
                {
                    actual_key.GetComponent<KeyController2>().is_good_key = true;
                }
                else
                {
                    actual_key.GetComponent<KeyController2>().is_good_key = false;
                }
            }
            //Le material change à chaque clé pour que le joueur est bien l'mpression qu'il s'agisse
            //d'une nouvelle clé
            if (index % 2 == 0)
            {
                actual_key.transform.GetChild(0).GetComponent<Renderer>().material = keymat1;
            }
            else
            {
                actual_key.transform.GetChild(0).GetComponent<Renderer>().material = keymat2;
            }
        }
    }
}
