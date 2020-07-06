using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe permettant de récupérer efficacement le résultat d'une coroutine. Ce code n'est pas de moi : 
/// https://answers.unity.com/questions/24640/how-do-i-return-a-value-from-a-coroutine.html
/// </summary>
public class CoroutineResult 
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public CoroutineResult(MonoBehaviour owner, IEnumerator target)
    {
        target = target;
        coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}
