using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryGenerator
{
    static Noun[] appearances = { Noun.Blonde, Noun.Brown, Noun.Red };
    static Noun[] identities = { Noun.Exwife, Noun.Daughter, Noun.Bastard };
    static Noun[] names = { Noun.Alice, Noun.Brianna, Noun.Catherine };
    
    

    public static void Generate(out PersonState[] people, out ClueInfo startingClue, out List<ClueItem> cluesToScatter)
    {
        cluesToScatter = new List<ClueItem>();

        // Randomly determine the world

        // The player is going to have some natural questions, and we should
        // create clues that answer them, otherwise they're going to be unsatisfied.

        // questions:
        // who's the killer?
        // why don't we remember anything?
        // where are we?
        // who is each person?
        // why are we here?
        // what's the story? what events led to where we are now?

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
        // Each player knows the existence of the hair-colors of the other players
        for (int i = 0; i < names.Length; i++)
        {
            for (int j = 0; j < names.Length; j++)
            {
                if (i != j)
                {
                    people[i].knowledge.KnownWords.Add(people[j].AttributeMap[NounType.HairColor]);
                }
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
            Noun hair = people[i].AttributeMap[NounType.HairColor];
            Noun identity = people[i].AttributeMap[NounType.Identity];
            Noun name = people[i].AttributeMap[NounType.Name];
            ClueItem appearanceToIdentity = ClueManifest.GetClue(hair, identity);
            if(appearanceToIdentity == null)
            {
                Debug.Log("No clue for " + hair + " <-> " + identity + "!");
            }
            else
            {
                cluesToScatter.Add(appearanceToIdentity);
            }

            ClueItem identityToName = ClueManifest.GetClue(identity, name);
            if(identityToName == null)
            {
                Debug.Log("No clue for " + hair + " <-> " + identity + "!");
            }
            else
            {
                cluesToScatter.Add(identityToName);
            }
        }
    }
}
