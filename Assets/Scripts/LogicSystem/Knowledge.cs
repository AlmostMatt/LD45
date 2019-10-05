using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Knowledge
{
    private List<Sentence> knownSentences = new List<Sentence>();
    private Dictionary<Noun, List<Sentence>> sentencesBySpeaker = new Dictionary<Noun, List<Sentence>>();

    public Knowledge()
    {
        knownSentences.Add(new Sentence(Noun.Alice, Verb.Is, Noun.Blonde, Adverb.False));
        knownSentences.Add(new Sentence(Noun.Alice, Verb.Is, Noun.Red, Adverb.True));
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
}
