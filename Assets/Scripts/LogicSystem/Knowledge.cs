using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Knowledge
{
    private struct SentenceBelief
    {
        public Sentence mSentence;
        public float mConfidence; // maybe we always calculate this on the fly?

        bool mDeduced;
        Sentence mSourceSentence1;
        Sentence mSourceSentence2;

        int mSourceId; // what person did this come from (if any)?        

        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }

            SentenceBelief other = (SentenceBelief)obj;
            return (
                    mSentence.Equals(other.mSentence)
                // &&  mDeduced == other.mDeduced
//                && (
//                        (mDeduced && mSourceSentence1 == other.mSourceSentence1 && mSourceSentence2 == other.mSourceSentence2)
//                    ||  (!mDeduced && mSourceId == other.mSourceId)
//                )
            );
        }

        public SentenceBelief(Sentence sentence, int sourceId, float confidence)
        {
            mDeduced = false;
            mSentence = sentence;
            mSourceId = sourceId;
            mConfidence = confidence;

            mSourceSentence1 = null;
            mSourceSentence2 = null;
        }

        public SentenceBelief(Sentence sentence, Sentence source1, Sentence source2, float confidence)
        {
            mDeduced = true;
            mSentence = sentence;
            mSourceSentence1 = source1;
            mSourceSentence2 = source2;
            mConfidence = confidence;

            mSourceId = -1;
        }
    };

    // TODO: if we want them to lie, we probably want each person to not only have their own knowledge base,
    // but also be simulating everyone else's knowledge base.
    // Then before they reveal information, they simulate what everyone else would deduce, and ask them to 
    // verify the sentence "I am the killer". If they verify it, then we choose to withhold the information.
    private float[] mPersonConfidence = { 0.5f, 0.5f, 0.5f };
    private int mPersonId;

    // DEPRECATED
    private List<Sentence> knownSentences = new List<Sentence>();
    // DEPRECATED
    private Dictionary<Noun, List<Sentence>> sentencesBySpeaker = new Dictionary<Noun, List<Sentence>>();

    private List<SentenceBelief> mBeliefs = new List<SentenceBelief>();

    private List<Sentence> deductions = new List<Sentence>(); // TODO: we might want to track which sentences contributed to a given deduction?

    public Knowledge(int personId)
    {
        mPersonId = personId;
        mPersonConfidence[mPersonId] = 1f;
    }

    public Sentence Speak()
    {
        if (knownSentences.Count > 0)
        {
            return knownSentences[knownSentences.Count - 1];
        } else
        {
            return null;
        }
    }

    // DEPRECATED
    public void Listen(int personId, Noun speaker, Sentence sentence)
    {
        if (!sentencesBySpeaker.ContainsKey(speaker))
        {
            sentencesBySpeaker[speaker] = new List<Sentence>();
        }
        sentencesBySpeaker[speaker].Add(sentence);
    }

    public void Listen(PersonState person, Sentence sentence)
    {
        Debug.Log(mPersonId + " hears " + person.PersonId + " say " + sentence);
        SentenceBelief belief = new SentenceBelief(sentence, person.PersonId, mPersonConfidence[person.PersonId]);
        List<SentenceBelief> beliefs = new List<SentenceBelief>();
        beliefs.Add(belief);
        AddBeliefs(beliefs);
    }

    public float VerifySentence(Sentence sentence)
    {
        // find all beliefs matching this sentence,
        // return confidence?

        if (knownSentences.Contains(sentence)) return 1f;
        return 0f;
    }

    public float VerifyBelief(Sentence sentence)
    {
        float maxConfidence = 0f;
        foreach(SentenceBelief b in mBeliefs)
        {
            if(b.mSentence.Equals(sentence))
            {
                maxConfidence = Mathf.Max(maxConfidence, b.mConfidence);
            }
        }
        return maxConfidence;
    }

    private bool AddKnowledgeUnique(Sentence sentence)
    {
        if(knownSentences.Contains(sentence)) { return false; }

        knownSentences.Add(sentence);
        Debug.Log(mPersonId + " learned " + sentence);
        return true;
    }

    public void AddKnowledge(Sentence sentence) // implied source is yourself
    {
        // is it ok to have multiple beliefs about the same sentence? maybe it gets resolved later
        SentenceBelief belief = new SentenceBelief(sentence, mPersonId, mPersonConfidence[mPersonId]);
        if(!mBeliefs.Contains(belief))
        {
            Debug.Log(mPersonId + " believes " + sentence);
            mBeliefs.Add(belief);

            List<SentenceBelief> newBeliefs = new List<SentenceBelief>();
            newBeliefs.Add(belief);
            UpdateBeliefs(newBeliefs);
        }
        
        //if (AddKnowledgeUnique(sentence))
            //UpdateBeliefs();
    }

    private void AddBeliefs(List<SentenceBelief> beliefs)
    {
        List<SentenceBelief> newBeliefs = new List<SentenceBelief>();
        foreach (SentenceBelief b in beliefs)
        {
            if (!mBeliefs.Contains(b))
            {
                mBeliefs.Add(b);
                newBeliefs.Add(b);

                Debug.Log(mPersonId + " now believes " + b.mSentence + " (confidence: " + b.mConfidence + ")");
            }
        }

        if(newBeliefs.Count > 0)
        {
            UpdateBeliefs(newBeliefs);
        }
    }

    public void AddKnowledge(List<Sentence> sentences)
    {
        bool somethingNew = false;
        foreach(Sentence s in sentences)
        {
            somethingNew = AddKnowledgeUnique(s) || somethingNew; // order matters! don't short-circuit
        }

        if (somethingNew) { }
            // UpdateBeliefs();
    }

    public List<Sentence> GetKnown() { return knownSentences; }

    private void UpdateBeliefs(List<SentenceBelief> newBeliefs)
    {
        // go through all knowledge, and see if we can synthesize anything new from the new beliefs
        Debug.Log(mPersonId + " updating beliefs ");

        List<SentenceBelief> beliefDeductions = new List<SentenceBelief>(); // defer adding new knowledge until the end, so we aren't modifying knowledge while we read it

        // rule 1: transitivity
        // [A is B] and [B is C] => [A is C] (including valid permutations)
        foreach (SentenceBelief b1 in newBeliefs)
        {
            Sentence s1 = b1.mSentence;
            if (!(s1.Verb == Verb.Is && s1.Adverb == Adverb.True)) return;

            foreach (SentenceBelief b2 in mBeliefs)
            {
                if (b1.Equals(b2)) continue;
                
                Sentence s2 = b2.mSentence;                
                if (!(s2.Verb == Verb.Is && s2.Adverb == Adverb.True)) continue;

                float confidence = b1.mConfidence * b2.mConfidence;
                Sentence newSentence = null;
                if (s1.DirectObject == s2.Subject)
                {
                    newSentence = new Sentence(s1.Subject, Verb.Is, s2.DirectObject, Adverb.True);
                }
                else if (s1.Subject == s2.Subject)
                {
                    newSentence = new Sentence(s1.DirectObject, Verb.Is, s2.DirectObject, Adverb.True);
                }
                else if (s1.DirectObject == s2.DirectObject)
                {
                    newSentence = new Sentence(s1.Subject, Verb.Is, s2.Subject, Adverb.True);
                }
                else if (s1.Subject == s2.DirectObject)
                {
                    newSentence = new Sentence(s1.DirectObject, Verb.Is, s2.Subject, Adverb.True);
                }

                if (newSentence != null)
                {
                    SentenceBelief newBelief = new SentenceBelief(newSentence, s1, s2, confidence);
                    beliefDeductions.Add(newBelief);
                    Debug.Log("New belief: " + newBelief.mSentence + " (confidence: " + confidence + ")");
                    Debug.Log("Since " + s1 + " (confidence: " + b1.mConfidence + ") and " + s2 + " (confidence: " + b2.mConfidence + ")");
                }
            }
        }

        // rule 2: mutual exclusion
        // if [A is X] and [X, Y] are mutually exclusive, then [A is not Y]
        foreach (SentenceBelief b1 in newBeliefs)
        {
            Sentence s1 = b1.mSentence;
            if (!(s1.Verb == Verb.Is && s1.Adverb == Adverb.True)) return;

            NounType t = s1.DirectObject.Type();
            Noun[] nouns = t.GetNouns();
            if(nouns != null)
            {
                foreach(Noun n in nouns)
                {
                    if (s1.DirectObject != n)
                    {
                        Sentence newSentence = new Sentence(s1.Subject, Verb.Is, n, Adverb.False);
                        SentenceBelief belief = new SentenceBelief(newSentence, b1.mSentence, null, b1.mConfidence);
                        beliefDeductions.Add(belief);

                        Debug.Log("Testing belief: " + belief.mSentence + " (confidence: " + b1.mConfidence + ")");
                        Debug.Log("Since " + s1 + " (confidence: " + b1.mConfidence + ")");
                    }
                }                
            }

            t = s1.Subject.Type();
            nouns = t.GetNouns();
            if (nouns != null)
            {
                foreach (Noun n in nouns)
                {
                    if (s1.Subject != n)
                    {
                        Sentence newSentence = new Sentence(s1.DirectObject, Verb.Is, n, Adverb.False);
                        SentenceBelief belief = new SentenceBelief(newSentence, b1.mSentence, null, b1.mConfidence);
                        beliefDeductions.Add(belief);

                        Debug.Log("Testing belief: " + belief.mSentence + " (confidence: " + b1.mConfidence + ")");
                        Debug.Log("Since " + s1 + " (confidence: " + b1.mConfidence + ")");
                    }
                }
            }
        }


        AddBeliefs(beliefDeductions);
    }
}
