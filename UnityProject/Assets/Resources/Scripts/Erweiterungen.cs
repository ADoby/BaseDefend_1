using System.Collections.Generic;
using UnityEngine;

public static class Erweiterungen
{
    public static T RandomEntry<T>(this IEnumerable<T> enumerable, System.Func<T, float> weightFunc)
    {
        float totalWeight = 0; // this stores sum of weights of all elements before current
        T selected = default(T); // currently selected element
        foreach (var data in enumerable)
        {
            float weight = weightFunc(data); // weight of current element
            float r = Random.Range(0f, totalWeight + weight); // random value
            if (r >= totalWeight) // probability of this is weight/(totalWeight+weight)
                selected = data; // it is the probability of discarding last selected element and selecting current one instead
            totalWeight += weight; // increase weight sum
        }

        return selected; // when iterations end, selected is some element of sequence. 
    }
}

