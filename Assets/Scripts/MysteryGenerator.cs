using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryGenerator
{
    static Noun[] appearances = { Noun.Blonde, Noun.Black, Noun.Red };
    static Noun[] identities = { Noun.Exwife, Noun.Daughter, Noun.Bastard };
    static Noun[] names = { Noun.Alice, Noun.Beth, Noun.Carol };
    
    public static void Generate(out ClueInfo startingClue, out List<ClueInfo> cluesToScatter)
    {
        cluesToScatter = new List<ClueInfo>();

        // randomly determine the world
        int[] appearanceShuffle = Utilities.RandomList(3, 3);
        int[] identityShuffle = Utilities.RandomList(3, 3);
        int[] nameShuffle = Utilities.RandomList(3, 3);
        int killer = (int)Random.Range(0,3);
        
        // generate clue for the killer
        Noun killerName = names[nameShuffle[killer]];
        ClueInfo deathClue = new ClueInfo(Noun.Victim, killerName);
        startingClue = deathClue;
        
        // generate clues for everything else
        // this is probably too many, and we want to strategically
        // omit certain clues (so that we don't have an immediate person -> name clue)
        for(int i = 0; i < names.Length; ++i)
        {
            ClueInfo appearanceToIdentity = new ClueInfo(appearances[appearanceShuffle[i]], identities[identityShuffle[i]]);
            cluesToScatter.Add(appearanceToIdentity);

            ClueInfo identityToName = new ClueInfo(identities[identityShuffle[i]], names[nameShuffle[i]]);
            cluesToScatter.Add(identityToName);
        }
    }
}
