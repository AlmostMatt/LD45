using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryGenerator
{
    static Noun[] appearances = { Noun.Blonde, Noun.Brunette, Noun.Redhead };
    static Noun[] identities = { Noun.ExWife, Noun.Daughter, Noun.Mistress };
    static Noun[] names = { Noun.Alice, Noun.Brianna, Noun.Catherine };
    static Noun[] backstories = { Noun.Philanthropist, Noun.Writer, Noun.Scientist, Noun.Artist };
    static Noun[] motives = { Noun.OwesDebt, Noun.Inheritance, Noun.HasGrudge };

    // These attributes should be dependent, and not random
    private static readonly Dictionary<Noun, Noun[]> mDependentNouns = new Dictionary<Noun, Noun[]>
    {
        { Noun.ExWife, new Noun[] {Noun.Scientist, Noun.Inheritance } },
        { Noun.Daughter, new Noun[] {Noun.Writer, Noun.OwesDebt } },
        { Noun.Mistress, new Noun[] {Noun.Artist, Noun.HasGrudge } }
    };

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

        // TODO - have predermined pairings between backstory / motive / identity

        // Generate people by shuffling a list of indexes for each attribute type.
        Noun[][] attributeLists = { appearances, identities, names};
        int[][] shuffledLists = {
            Utilities.RandomList(3, 3),
            Utilities.RandomList(3, 3),
            Utilities.RandomList(3, 3),
            // Don't randomize backstories and motives
            // Utilities.RandomList(backstories.Length, 3),
            // Utilities.RandomList(3, 3),
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
            Noun[] depNouns = mDependentNouns[attributes[NounType.Identity]];
            for (int nounI = 0; nounI < depNouns.Length; nounI++ )
            {
                attributes.Add(depNouns[nounI].Type(), depNouns[nounI]);
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
        GameState.Get().KillerId = Random.Range(0,3);
        people[GameState.Get().KillerId].IsKiller = true;
        // Pick a person to have no motive
        int innocentId = Random.Range(0, 2); // two possible values
        if (innocentId >= GameState.Get().KillerId)
        {
            innocentId += 1;
        }

        // Generate an additional clue for the killer
        // starting: victim wrote a name in blood
        Noun killerName = people[GameState.Get().KillerId].AttributeMap[NounType.Name];
        ClueInfo deathClue = new ClueInfo(Noun.SuspectedName, killerName);
        startingClue = deathClue;

        // Generate clues for everything else
        // this is probably too many, and we want to strategically
        // omit certain clues (so that we don't have an immediate person -> name clue)
        for (int i = 0; i < names.Length; ++i)
        {
            Noun hair = people[i].AttributeMap[NounType.HairColor];
            Noun identity = people[i].AttributeMap[NounType.Identity];
            Noun name = people[i].AttributeMap[NounType.Name];
            Noun motive = people[i].AttributeMap[NounType.Motive];
            Noun backstory = people[i].AttributeMap[NounType.Backstory];

            ClueItem appearanceToIdentity = ClueManifest.GetClue(hair, identity);
            if(appearanceToIdentity != null)
            {
                cluesToScatter.Add(appearanceToIdentity);                
            }
            else
            {
                Debug.Log("No clue for " + hair + " <-> " + identity + "!");
            }

            ClueItem identityToName = ClueManifest.GetClue(identity, name);
            if(identityToName != null)
            {
                cluesToScatter.Add(identityToName);
            }
            else
            {
                Debug.Log("No clue for " + hair + " <-> " + identity + "!");
            }

            if (i != innocentId)
            {
                // Connect identity to motive (for thematic and mechanical reasons)
                ClueItem motiveClue = ClueManifest.GetClue(identity, motive);
                if (motiveClue != null)
                {
                    cluesToScatter.Add(motiveClue);
                }
                else
                {
                    Debug.Log("No clue for " + identity + " <-> " + motive + "!");
                }
            }
            // Generate a clue connecting something to backstory
            Noun identityOrName = Random.Range(0, 2) == 0 ? identity : name;
            ClueItem backstoryClue = ClueManifest.GetClue(identityOrName, backstory);
            if (backstoryClue != null)
            {
                cluesToScatter.Add(backstoryClue);
            }
            else
            {
                Debug.Log("No clue for " + identityOrName + " <-> " + backstory + "!");
            }
        }
        // Unique clues
        ClueItem potion = ClueManifest.GetClue(Noun.Potion, Noun.MemoryLoss);
        if (potion != null)
        {
            cluesToScatter.Add(potion);
        }
        else
        {
            Debug.Log("No clue for " + Noun.Potion + " <-> " + Noun.MemoryLoss + "!");
        }
    }
}
