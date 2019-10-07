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
    public bool KnowsOwnFace = false;
    public Dictionary<NounType, Noun> AttributeMap = new Dictionary<NounType, Noun>();
    public Knowledge knowledge;

    public bool IsPlayer
    {
        get { return PersonId == GameState.Get().PlayerId; }
    }
    
    public Sprite HeadSprite
    {
        // Expects a sprite to exist with a name like HeadRed
        get {
            return (IsPlayer && !KnowsOwnFace) ?
                SpriteManager.GetSprite("Faceless" + AttributeMap[NounType.HairColor].ToString()) :
                SpriteManager.GetSprite("Head" + AttributeMap[NounType.HairColor].ToString());
        }
    }
    public Sprite PersonSprite
    {
        // Expects a sprite to exist with a name like PersonRed
        get { return SpriteManager.GetSprite("Person" + AttributeMap[NounType.HairColor].ToString()); }
    }
    public string PublicName
    {
        get {
            if (IsPlayer)
                return "Me";
            if (PersonId == 4)
                return "Police Officer";

            return AttributeMap[NounType.HairColor].ToString();
        }
    }

    public PersonState(int personId)
    {
        PersonId = personId;
        // Dont give the victim any knowledge
        if (personId != 3)
        {
            knowledge = new Knowledge(this);
        }
    }
    
    public string Speak(Sentence s)
    {
        Noun myHair = AttributeMap[NounType.HairColor];
        if(s.Subject == myHair)        
            return "I'm " + s.DirectObject.AsSubject() + ".";

        if (s.DirectObject == myHair)
            return "I'm " + s.Subject.AsSubject() + ".";

        return s.ToString();
    }
}
