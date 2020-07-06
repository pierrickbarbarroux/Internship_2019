using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Cette classe permet de gérer les interactions avec le canvas/UI du menu principal.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Tooltip("Input field easting")]
    public TMP_InputField inputfield_long;
    [Tooltip("Input field northing")]
    public TMP_InputField inputfield_lat;

    [HideInInspector]
    public TMP_InputField inputfield_size;
    [Tooltip("Input field du numéro de la borne")]
    public TMP_InputField inputfield_borne;
    [Tooltip("Input field de la question concernant la borne")]
    public TMP_InputField inputfield_question;

    [Tooltip("Ecran de transition de la scène")]
    public GameObject transitionScreen;

    [Tooltip("Menu déroulant des villes")]
    public TMP_Dropdown myDropdown;

    [Tooltip("Menu déroulant de la taille de la carte (256*256)")]
    public TMP_Dropdown myDropdownSize;


    // Start is called before the first frame update
    void Start()
    {
        //Par défaut, on se place au niveau de Saint-Mandé
        //La borne par défaut est la borne choisie par les dames de la communication
        inputfield_long.text = "657995";
        inputfield_lat.text = "6860374";
        inputfield_borne.text = "7505601";

        //inputfield_size.text = "3";
        ParameterManager.Size = 1;

        //Code gérant en partie le système des menus déroulants
        myDropdown.onValueChanged.AddListener(delegate
        {
            myDropdownValueChangedHandler(myDropdown);
        });
        myDropdownSize.onValueChanged.AddListener(delegate
        {
            myDropdownValueChangedHandler(myDropdownSize);
        });
    }


    /// <summary>
    /// Charge la carte et donc la scène suivante. cette fonction est appelée dans l'event systeme du
    /// bouton 'Valider'.
    /// </summary>
    public void LoadMap()
    {
        StartCoroutine(LoadMapCoroutine());
    }

    /// <summary>
    /// Charge la carte. Version Coroutine.
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadMapCoroutine()
    {
        //On vérifie que les input fields ne soient pas vides
        if (inputfield_long.text != "" && inputfield_lat.text != "")// && inputfield_size.text != "")
        {
            //Le ParameterManager gère les variables static (globales) du projet (ou du moins les plus importantes)
            //C'est de cette manière qu'on fait le lien entre les inputs et les valeurs des paramètres
            //choisis pour les tuiles.
            ParameterManager.Long = float.Parse(inputfield_long.text);
            ParameterManager.Lat = float.Parse(inputfield_lat.text);
            //ParameterManager.Size = int.Parse(inputfield_size.text);
            ParameterManager.BorneNumber = int.Parse(inputfield_borne.text);
            ParameterManager.Question = inputfield_question.text;

            //On active l'écran de transition et on lance l'animation
            transitionScreen.SetActive(true);
            transitionScreen.GetComponent<Animator>().Play("Transition_Screen_Menu");
            yield return new WaitForSeconds(1.4f);
            SceneManager.LoadScene("Stage_IGN", LoadSceneMode.Single);
        }
    }

    /// <summary>
    ///  Cette fonction est appelé chaque fois que le dropdown change de valeur (voir les event sur
    ///  l'objet dropdown de la scène). Elle permet de modifier les inputs du menu principal.
    /// </summary>
    public void ChangeTown()
    {
        string longit = "0";
        string lat = "0";
        //string size = "3";
        switch (myDropdown.value)
        {
            case 0:
                longit = "657995";
                lat = "6860374";
                break;
            case 1:
                longit = "930587.3";
                lat = "6258474";
                break;
            case 2:
                longit = "651363";
                lat = "6861531";
                break;
        }

        //On modifie les valeurs affichées des input fields
        inputfield_long.text = longit;
        inputfield_lat.text = lat;
        ParameterManager.Long = int.Parse(longit);
        ParameterManager.Lat = int.Parse(lat);
        //inputfield_size.text = size;
    }

    /// <summary>
    /// Cette méthode permet de modifier les valeurs de la variable gloabale Size qui caractérise la taille 
    /// de la carte en foncion du nombre de 'couche' de tuiles. Pour plus de clarté, j'ai mis un dropdown 
    /// avec la taille réelle (mètres * mètres) de la carte dans le canvas.
    /// </summary>
    public void ChangeSize()
    {
        int size = 1;
        switch (myDropdownSize.value)
        {
            case 0:
                size = 1;
                break;
            case 1:
                size = 2;
                break;
            case 2:
                size = 3;
                break;
            case 3:
                size = 4;
                break;
            case 4:
                size = 5;
                break;
            case 5:
                size = 6;
                break;
            case 6:
                size = 7;
                break;
        }

        ParameterManager.Size = size;
        Downloader.Size = size;
        Downloader.xSize = 2 * size - 1;
    }

    /// <summary>
    /// Gère les dropdown (en partie)
    /// </summary>
    void Destroy()
    {
        myDropdown.onValueChanged.RemoveAllListeners();
    }

    /// <summary>
    /// Gère les dropdown (en partie)
    /// </summary>
    private void myDropdownValueChangedHandler(TMP_Dropdown target)
    {
        Debug.Log("selected: " + target.value);
    }

    /// <summary>
    /// Gère les dropdown (en partie)
    /// </summary>
    public void SetDropdownIndex(int index)
    {
        myDropdown.value = index;
    }
}
