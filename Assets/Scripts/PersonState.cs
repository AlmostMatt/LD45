using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Contains the information associated with a person.
 */
public class PersonState
{
    public int PersonId;
    public bool IsKiller;
    public Dictionary<NounType, Noun> AttributeMap = new Dictionary<NounType, Noun>();
    public Knowledge knowledge = new Knowledge();

    public bool IsPlayer
    {
        get { return PersonId == GameState.Get().PlayerId; }
    }

    public PersonState(int personId)
    {
        PersonId = personId;
    }
}
