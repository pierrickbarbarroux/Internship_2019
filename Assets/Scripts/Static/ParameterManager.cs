using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe gère les variables globales du projet et plus exactement les paramètres que l'on passe en entrée dans
/// le menu principal. Ces paramètres sont :
/// -l'easting et le northing de la tuile centrale
/// -la taille de la carte (sous forme de couche de tuiles)
/// -le numéro de la borne que l'on cherche
/// -la question à laquelle l'équipe doit répondre à la fin du jeu pour pouvoir avancer dans l'escape game
/// </summary>
public static class ParameterManager
{
    private static float longitude, latitude;
    private static int size;
    private static int borne_number;
    private static string question;

    public static float Long
    {
        get
        {
            return longitude;
        }
        set
        {
            longitude = value;
        }
    }

    public static float Lat
    {
        get
        {
            return latitude;
        }
        set
        {
            latitude = value;
        }
    }

    public static int Size
    {
        get
        {
            return size;
        }
        set
        {
            size = value;
        }
    }

    public static int BorneNumber
    {
        get
        {
            return borne_number;
        }
        set
        {
            borne_number = value;
        }
    }

    public static string Question
    {
        get
        {
            return question;
        }
        set
        {
            question = value;
        }
    }
}
