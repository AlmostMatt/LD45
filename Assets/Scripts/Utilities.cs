using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities
{
    public static int[] RandomList(int totalPossibilities, int numChoices)
    {
        numChoices = Mathf.Min(totalPossibilities, numChoices);
        int[] availableChoices = new int[totalPossibilities];
        for (int i = 0; i < totalPossibilities; ++i)
        {
            availableChoices[i] = i;
        }

        int[] chosenIndices = new int[numChoices];
        for (int i = 0; i < numChoices; ++i)
        {
            int pick = Random.Range(0, totalPossibilities);
            chosenIndices[i] = availableChoices[pick];
            availableChoices[pick] = availableChoices[--totalPossibilities];
        }
        return (chosenIndices);
    }

    public static string bold(string str)
    {
        // This could also use other tags - color, italics, size
        return "<b>" + str + "</b>";
    }
}
