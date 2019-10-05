using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Knowledge
{
    private List<Sentence> knownSentences = new List<Sentence>();
    private Dictionary<Noun, List<Sentence>> sentencesBySpeaker = new Dictionary<Noun, List<Sentence>>();

    private List<Sentence> deductions = new List<Sentence>(); // TODO: we might want to track which sentences contributed to a given deduction?

    public Knowledge()
    {
    }

    public Sentence Speak()
    {
        return knownSentences[0];
    }

    public void Listen(Noun speaker, Sentence sentence)
    {
        if (!sentencesBySpeaker.ContainsKey(speaker))
        {
            sentencesBySpeaker[speaker] = new List<Sentence>();
        }
        sentencesBySpeaker[speaker].Add(sentence);
    }

    public float VerifySentence(Sentence sentence)
    {
        if (knownSentences.Contains(sentence)) return 1f;
        return 0f;
    }

    private bool AddKnowledgeUnique(Sentence sentence)
    {
        if(knownSentences.Contains(sentence)) { return false; }

        knownSentences.Add(sentence);
        Debug.Log("Learned " + sentence);
        return true;
    }

    public void AddKnowledge(Sentence sentence) // implied source is yourself
    {
        if (AddKnowledgeUnique(sentence))
            UpdateBeliefs();
    }

    public void AddKnowledge(List<Sentence> sentences)
    {
        bool somethingNew = false;
        foreach(Sentence s in sentences)
        {
            somethingNew = AddKnowledgeUnique(s) || somethingNew; // order matters! don't short-circuit
        }

        if(somethingNew)
            UpdateBeliefs();
    }

    public List<Sentence> GetKnown() { return knownSentences; }

    private void UpdateBeliefs()
    {
        List<Sentence> deductions = new List<Sentence>(); // defer adding new knowledge until the end, so we aren't modifying knowledge while we read it

        // rule 1: transitivity
        // [A is B] and [B is C] => [A is C] (including valid permutations)
        // go through all knowledge, and see if we can synthesize anything new
        foreach(Sentence s1 in knownSentences)
        {
            if (!(s1.Verb == Verb.Is && s1.Adverb == Adverb.True)) continue;
            
            foreach(Sentence s2 in knownSentences)
            {
                if (s1 == s2) continue;
                if (!(s2.Verb == Verb.Is && s2.Adverb == Adverb.True)) continue;
                
                if(s1.DirectObject == s2.Subject)
                {
                    Debug.Log("Deduced sentence! ");
                    Debug.Log("Given " + s1);
                    Debug.Log("And " + s2);
                    Sentence newSentence = new Sentence(s1.Subject, Verb.Is, s2.DirectObject, Adverb.True);
                    Debug.Log("Implies " + newSentence);
                    deductions.Add(newSentence);
                }
                else if(s1.Subject == s2.Subject)
                {
                    Debug.Log("Deduced sentence! ");
                    Debug.Log("Given " + s1);
                    Debug.Log("And " + s2);
                    Sentence newSentence = new Sentence(s1.DirectObject, Verb.Is, s2.DirectObject, Adverb.True);
                    Debug.Log("Implies " + newSentence);
                    deductions.Add(newSentence);
                }
                else if(s1.DirectObject == s2.DirectObject)
                {
                    Debug.Log("Deduced sentence! ");
                    Debug.Log("Given " + s1);
                    Debug.Log("And " + s2);
                    Sentence newSentence = new Sentence(s1.Subject, Verb.Is, s2.Subject, Adverb.True);
                    Debug.Log("Implies " + newSentence);
                    deductions.Add(newSentence);
                }
            }
        }

        AddKnowledge(deductions);
    }
}
