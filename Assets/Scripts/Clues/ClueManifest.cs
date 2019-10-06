using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueManifest : MonoBehaviour
{
    public List<GameObject> clues;

    private static ClueManifest sClueManifestSingleton;
    
    public static ClueItem GetClue(Noun n1, Noun n2)
    {
        if (sClueManifestSingleton == null)
        {
            sClueManifestSingleton = GameObject.FindWithTag("GameRules").GetComponent<ClueManifest>();
        }
        return sClueManifestSingleton.GetClueInternal(n1, n2);
    }

    private ClueItem GetClueInternal(Noun n1, Noun n2)
    {
        NounType t1 = n1.Type();
        NounType t2 = n2.Type();
        foreach(GameObject g in clues)
        {
            ClueGenerator generator = g.GetComponent<ClueGenerator>();
            if(generator.MatchTypes(t1, t2))
            {
                ClueItem item = generator.GetItem(n1, n2);
                return item;
            }
        }
        return null;
    }
}
