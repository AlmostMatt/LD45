using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryGenerator
{
    static Noun[] appearances = { Noun.Blonde, Noun.Brown, Noun.Red };
    static Noun[] identities = { Noun.Exwife, Noun.Daughter, Noun.Bastard };
    static Noun[] names = { Noun.Alice, Noun.Beth, Noun.Carol };

    public static void Generate(out PersonState[] people, out ClueInfo startingClue, out List<ClueInfo> cluesToScatter)
    {
        cluesToScatter = new List<ClueInfo>();

        // Randomly determine the world

        // Generate people by shuffling a list of indexes for each attribute type.
        Noun[][] attributeLists = { appearances, identities, names };
        int[][] shuffledLists = {
            Utilities.RandomList(3, 3),
            Utilities.RandomList(3, 3),
            Utilities.RandomList(3, 3),
        };
        people = new PersonState[3];
        for (int i = 0; i < names.Length; ++i)
        {
            people[i] = new PersonState(i);
            Dictionary<NounType, Noun> attributes = people[i].AttributeMap;
            for (int attrIndex = 0; attrIndex < attributeLists.Length; attrIndex++)
            {
                Noun playerAttr = attributeLists[attrIndex][shuffledLists[attrIndex][i]];
                attributes.Add(playerAttr.Type(), playerAttr);
            }
        }
        // Pick a killer
        int killer = Random.Range(0,3);
        people[killer].IsKiller = true;

        // Generate an additional clue for the killer
        Noun killerName = people[killer].AttributeMap[NounType.Name];
        ClueInfo deathClue = new ClueInfo(Noun.Killer, killerName);
        startingClue = deathClue;

        // Generate clues for everything else
        // this is probably too many, and we want to strategically
        // omit certain clues (so that we don't have an immediate person -> name clue)
        for (int i = 0; i < names.Length; ++i)
        {
            ClueInfo appearanceToIdentity = new ClueInfo(
                people[i].AttributeMap[NounType.HairColor],
                people[i].AttributeMap[NounType.Identity]
            );
            cluesToScatter.Add(appearanceToIdentity);

            ClueInfo identityToName = new ClueInfo(
                people[i].AttributeMap[NounType.Identity],
                people[i].AttributeMap[NounType.Name]
            );
            cluesToScatter.Add(identityToName);
        }
    }
}
