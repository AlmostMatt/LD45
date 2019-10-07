using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Represents a logical sentence.
 * 
 * Examples:
 * Sentence s = new Sentence(Noun.Alice,Verb.Is,Noun.Blonde,Adverb.False);
 * Alice IS Blonde TRUE
 * Alice HAS Knife TRUE
 * Blonde HAS Knife FALSE
 * 
 * Ideally a group of sentences can either imply guilt or suspicion, or eliminate possibilities.
 * 
 */

public class Sentence
{
    public Noun Subject;
    public Verb Verb; // Maybe all verbs are functionally identical
    public Noun DirectObject;
    public Adverb Adverb;

    public Sentence(Noun subj, Verb verb, Noun obj, Adverb adv)
    {
        if(subj.Type() > obj.Type())
        {
            Noun tmp = subj;
            subj = obj;
            obj = tmp;
        }

        this.Subject = subj;
        this.Verb = verb;
        this.DirectObject = obj;
        this.Adverb = adv;
    }

    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }

        Sentence other = (Sentence)obj;
        return(
               (Verb == other.Verb)
            && (Adverb == other.Adverb)
            && (
                    (Subject == other.Subject && DirectObject == other.DirectObject)
                || (Subject == other.DirectObject && DirectObject == other.Subject)
            )
        );
    }
    
    // there's a warning about not overriding GetHashCode?

    public bool SameIdea(Sentence other)
    {
        return (
               (Verb == other.Verb)
            && (
                    (Subject == other.Subject && DirectObject == other.DirectObject)
                 || (Subject == other.DirectObject && DirectObject == other.Subject)
               )
        );
    }

    public override string ToString()
    {
        // To be human-readable, Name should come before identity before property.
        // Some words should be preceeded by "a" or "the"
        // For properties, it is often preceeded by "Has" not "Is"
        // When linking property to property it should be something like "The red-haired person has X"

        List<string> words = new List<string>();
        
        words.Add(Subject.AsSubject());
        words.Add(DirectObject.AsObject(Adverb == Adverb.True));
        
        return string.Join(" ", words) + ".";
    }
}
