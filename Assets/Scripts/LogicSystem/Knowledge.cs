using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Knowledge
{
    private class SentenceBelief
    {
        public Sentence mSentence;
        public float mConfidence; // maybe we always calculate this on the fly?

        public int mSourceId; // what person did this come from (if any)?

        public bool mDeduced;
        public SentenceBelief mSourceBelief1;
        public SentenceBelief mSourceBelief2;

        public List<SentenceBelief> mDeductions = new List<SentenceBelief>();

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

            mSourceBelief1 = null;
            mSourceBelief2 = null;
        }

        public SentenceBelief(Sentence sentence, SentenceBelief source1, SentenceBelief source2, float confidence)
        {
            mDeduced = true;
            mSentence = sentence;
            mSourceBelief1 = source1;
            mSourceBelief2 = source2;
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
    private PersonState mPerson;

    public HashSet<Noun> KnownWords = new HashSet<Noun>();

    private List<SentenceBelief> mBeliefs = new List<SentenceBelief>();

    private List<List<SentenceBelief>> mSuspiciousBeliefs = new List<List<SentenceBelief>>();

    public Knowledge(PersonState person)
    {
        mPersonId = person.PersonId;

        if(mPersonId < 3)
        {
            mPersonConfidence[mPersonId] = 1f;
        }
        
        mPerson = person;
    }
    
    private void RecursiveUpdateConfidence(SentenceBelief b, float amt)
    {        
        b.mConfidence *= amt;
        Debug.Log(mPersonId + " now only believes " + b.mSentence + " " + b.mConfidence);
        foreach (SentenceBelief deduction in b.mDeductions)
        {
            RecursiveUpdateConfidence(deduction, amt);
        }
    }

    private void ConfidenceLost(int personId)
    {
        float decay = 0.8f;
        mPersonConfidence[personId] *= decay;
        float newConfidence = mPersonConfidence[personId];
        Debug.Log(mPersonId + " now only trusts " + personId + " this much: " + newConfidence);
        // update all things heard from this person
        foreach(SentenceBelief b in mBeliefs)
        {
            if(b.mSourceId == personId)
            {
                RecursiveUpdateConfidence(b, decay);
            }
        }
    }

    public void Listen(PersonState person, Sentence sentence, out string[] spokenResponse, out AudioClipIndex[] audioResponse)
    {
        // Allow the player to use these words for sentences later
        // unique words are not worth talking about, and AI should also not store any beliefs.
        if (sentence.Subject.Type() == NounType.Unique || sentence.DirectObject.Type() == NounType.Unique)
        {
            int randomReaction2 = Random.Range(0, 3);
            string[] possibleSpokenReact2 = new string[] { "Interesting...", "Hmm...", "Oh." };
            AudioClipIndex[] possibleAudio2 = new AudioClipIndex[] { AudioClipIndex.HMM, AudioClipIndex.HMM, AudioClipIndex.OH };
            spokenResponse = new string[] { possibleSpokenReact2[randomReaction2] };
            audioResponse = new AudioClipIndex[] { possibleAudio2[randomReaction2] };
            return;
        }
        // special case hax: SuspectedName is not worth talking about
        if (sentence.Subject != Noun.SuspectedName)
        {
            KnownWords.Add(sentence.Subject);
        }

        if (sentence.DirectObject != Noun.SuspectedName)
        {
            KnownWords.Add(sentence.DirectObject);
        }

        // preemptively reject sentences that contradict something we are sure of
        Sentence opposite = new Sentence(sentence.Subject, sentence.Verb, sentence.DirectObject, sentence.Adverb == Adverb.True ? Adverb.False : Adverb.True);
        float confidenceInOpposite = VerifyBelief(opposite);
        if(confidenceInOpposite >= 1f)
        {
            Debug.Log(person.PersonId + " told a lie: " + sentence);
            ConfidenceLost(person.PersonId);
            spokenResponse = new string[] { "What? I know that's not true." };
            audioResponse = new AudioClipIndex[] { AudioClipIndex.SURPRISE_EH };
            return;
        }

        float confidence = VerifyBelief(sentence);
        if(confidence > 0)
        {
            if (confidence >= 1)
            {
                spokenResponse = new string[] { "Sure, I already knew that." };
                audioResponse = new AudioClipIndex[] { AudioClipIndex.AGREE };
                return;
            }

            if(confidence >= 0.5)
            {
                spokenResponse = new string[] { "I suspected as much." }; // should this actually early return?
                audioResponse = new AudioClipIndex[] { AudioClipIndex.AGREE };
                return;
            }

            // increase confidence? maybe this is a way to "hack the system" to gain AI trust: tell them things they already believe
        }

        // Add this to beliefs with some confidence number
        Debug.Log(mPersonId + " hears " + person.PersonId + " say " + sentence);
        SentenceBelief belief = new SentenceBelief(sentence, person.PersonId, mPersonConfidence[person.PersonId]);
        List<SentenceBelief> beliefs = new List<SentenceBelief>();
        beliefs.Add(belief);
        AddBeliefs(beliefs);

        Noun myHairColor = mPerson.AttributeMap[NounType.HairColor];
        if(sentence.Subject == myHairColor)
        {

            spokenResponse = new string[] {
                "So " + sentence.DirectObject.AsPersonal() + "?",
                sentence.DirectObject.PersonalReaction()
            };
            audioResponse = new AudioClipIndex[] { AudioClipIndex.SURPRISE_EH, AudioClipIndex.NONE };
            return;
        }
        else if(sentence.DirectObject == myHairColor)
        {
            spokenResponse = new string[] {
                "So I'm " + sentence.Subject.AsSubject() + "...?",
                sentence.Subject.PersonalReaction()
            };
            audioResponse = new AudioClipIndex[] { AudioClipIndex.SURPRISE_EH, AudioClipIndex.NONE };
            return;
        }

        int randomReaction = Random.Range(0, 3);
        string[] possibleSpokenReact = new string[] { "Interesting...", "Hmm...", "Oh." };
        AudioClipIndex[] possibleAudio = new AudioClipIndex[] { AudioClipIndex.HMM, AudioClipIndex.HMM, AudioClipIndex.OH};
        spokenResponse = new string[] { possibleSpokenReact[randomReaction] };
        audioResponse = new AudioClipIndex[] { possibleAudio[randomReaction] };
        return;
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

    public List<string> ExplainBelief(Sentence sentence)
    {
        List<string> explanation = new List<string>();

        float maxConfidence = 0f;
        SentenceBelief bestBelief = null;
        foreach (SentenceBelief b in mBeliefs)
        {
            if (b.mSentence.Equals(sentence))
            {
                maxConfidence = Mathf.Max(maxConfidence, b.mConfidence);
                bestBelief = b;
            }
        }

        if(bestBelief == null)
        {
            return explanation;
        }

        ExplainBeliefRecursive(bestBelief, explanation);
        return explanation;
    }

    private void ExplainBeliefRecursive(SentenceBelief b, List<string> explanation)
    {
        if (!b.mDeduced)
        {
            if (b.mSourceId == mPersonId)
            {
                explanation.Add((explanation.Count > 0 ? "And " : "") + b.mSentence);
            }
            else
            {
                explanation.Add((explanation.Count > 0 ? "And " : "") + GameState.Get().mPeople[b.mSourceId].AttributeMap[NounType.HairColor] + " said that " + b.mSentence);
            }

            return;
        }

        ExplainBeliefRecursive(b.mSourceBelief1, explanation);
        if (b.mSourceBelief2 != null)
            ExplainBeliefRecursive(b.mSourceBelief2, explanation);
    }

    public void AddKnowledge(Sentence sentence) // implied source is yourself, 100% confidence
    {
        if (sentence.Subject.Type() == NounType.Unique || sentence.DirectObject.Type() == NounType.Unique)
        {
            return; // dont store any AI beliefs about things like the potion. Dont allow sentences about it.
        }
        // Allow the player to use these words for sentences later
        if (sentence.Subject != Noun.SuspectedName)
            KnownWords.Add(sentence.Subject);

        if(sentence.DirectObject != Noun.SuspectedName)
            KnownWords.Add(sentence.DirectObject);

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

                // update links for deduction graph
                if(b.mSourceBelief1 != null) { b.mSourceBelief1.mDeductions.Add(b); }
                if(b.mSourceBelief2 != null) { b.mSourceBelief2.mDeductions.Add(b); }

                Debug.Log(mPersonId + " now believes " + b.mSentence + " (confidence: " + b.mConfidence + ")");
            }
        }

        if(newBeliefs.Count > 0)
        {
            UpdateBeliefs(newBeliefs);
        }
    }

    private void UpdateBeliefs(List<SentenceBelief> newBeliefs)
    {
        // go through all knowledge, and see if we can synthesize anything new from the new beliefs
        Debug.Log(mPersonId + " updating beliefs ");

        // defer adding/removing knowledge until the end, so we aren't modifying knowledge while we read it
        List<SentenceBelief> beliefDeductions = new List<SentenceBelief>();
        List<SentenceBelief> refutedBeliefs = new List<SentenceBelief>();

        foreach (SentenceBelief b1 in newBeliefs)
        {
            Sentence s1 = b1.mSentence;

            // before making any inferences, check for contradictions
            bool contradictory = false;
            foreach (SentenceBelief b2 in mBeliefs)
            {
                Sentence s2 = b2.mSentence;
                if (s1.SameIdea(s2) && s1.Adverb != s2.Adverb)
                {
                    contradictory = true;
                    if (b2.mConfidence >= 1)
                    {
                        Debug.Log(mPersonId + " accepted info that contradicts something known: " + s1 + " vs. " + s2);
                        refutedBeliefs.Add(b1);
                        continue;
                    }

                    if (b1.mConfidence >= 1)
                    {
                        Debug.Log(mPersonId + " found information contradicting previous beliefs: " + s1 + " vs. " + s2);
                        refutedBeliefs.Add(b2);
                        beliefDeductions.Add(b1); // queue up this belief for re-thinking, after removing the belief it contradicts
                    }
                    else
                    {
                        // conflicting information, but not sure about which is true... oh well
                        // TODO: what to do here? set up some kind of contradictory info set that the AI active seeks to resolve?
                    }
                }
            }

            if(contradictory)
            {
                Debug.Log(mPersonId + " thinks " + b1.mSentence + " is contradictory, and will not make inferences with it");
                continue;
            }

            // rule 1: transitivity
            // [A is B] and [B is C] => [A is C] (including valid permutations)
            // also, the negative version (rule 1.5): [A is B] and [B is not C] => [A is not C]
            if (s1.Verb == Verb.Is)
            {
                if (s1.Adverb == Adverb.True)
                {
                    foreach (SentenceBelief b2 in mBeliefs)
                    {
                        if (b1.Equals(b2)) continue;

                        Sentence s2 = b2.mSentence;
                        if (s2.Verb != Verb.Is) continue;

                        Sentence newSentence = null;
                        float confidence = b1.mConfidence * b2.mConfidence;
                        if (confidence <= 0.2) { continue; }

                        if (s2.Adverb == Adverb.True || s2.Adverb == Adverb.False)
                        {
                            if (s1.DirectObject == s2.Subject)
                            {
                                newSentence = new Sentence(s1.Subject, Verb.Is, s2.DirectObject, s2.Adverb);
                            }
                            else if (s1.Subject == s2.Subject)
                            {
                                newSentence = new Sentence(s1.DirectObject, Verb.Is, s2.DirectObject, s2.Adverb);
                            }
                            else if (s1.DirectObject == s2.DirectObject)
                            {
                                newSentence = new Sentence(s1.Subject, Verb.Is, s2.Subject, s2.Adverb);
                            }
                            else if (s1.Subject == s2.DirectObject)
                            {
                                newSentence = new Sentence(s1.DirectObject, Verb.Is, s2.Subject, s2.Adverb);
                            }
                        }

                        if (newSentence != null)
                        {
                            SentenceBelief newBelief = new SentenceBelief(newSentence, b1, b2, confidence);
                            beliefDeductions.Add(newBelief);
                            Debug.Log("New belief: " + newBelief.mSentence + " (confidence: " + confidence + ") | Since " + s1 + " (confidence: " + b1.mConfidence + ") and " + s2 + " (confidence: " + b2.mConfidence + ")");
                        }
                    }
                }
                else if (s1.Adverb == Adverb.False)
                {
                    foreach (SentenceBelief b2 in mBeliefs)
                    {
                        if (b1.Equals(b2)) continue;

                        Sentence s2 = b2.mSentence;
                        if (!(s2.Verb == Verb.Is && s2.Adverb == Adverb.True)) continue;

                        Sentence newSentence = null;
                        float confidence = b1.mConfidence * b2.mConfidence;
                        if (confidence <= 0.2) { continue; }

                        if (s1.DirectObject == s2.Subject)
                        {
                            newSentence = new Sentence(s1.Subject, Verb.Is, s2.DirectObject, Adverb.False);
                        }
                        else if (s1.Subject == s2.Subject)
                        {
                            newSentence = new Sentence(s1.DirectObject, Verb.Is, s2.DirectObject, Adverb.False);
                        }
                        else if (s1.DirectObject == s2.DirectObject)
                        {
                            newSentence = new Sentence(s1.Subject, Verb.Is, s2.Subject, Adverb.False);
                        }
                        else if (s1.Subject == s2.DirectObject)
                        {
                            newSentence = new Sentence(s1.DirectObject, Verb.Is, s2.Subject, Adverb.False);
                        }

                        if (newSentence != null)
                        {
                            SentenceBelief newBelief = new SentenceBelief(newSentence, b1, b2, confidence);
                            beliefDeductions.Add(newBelief);
                            Debug.Log("New belief: " + newBelief.mSentence + " (confidence: " + confidence + ") | Since " + s1 + " (confidence: " + b1.mConfidence + ") and " + s2 + " (confidence: " + b2.mConfidence + ")");
                        }
                    }
                }
            } // END TRANSITIVITY


            // rule 2: mutual exclusion
            // if [A is X] and [X, Y] are mutually exclusive, then [A is not Y]
            // but this only applies if A <-> X is an IS relationship (i.e. nothing else can be X)
            if ((s1.Verb == Verb.Is && s1.Adverb == Adverb.True))
            {
                NounType t = s1.DirectObject.Type();
                Noun[] nouns = t.GetMutuallyExclusiveNouns();
                if (nouns != null)
                {
                    foreach (Noun n in nouns)
                    {
                        if (s1.DirectObject != n)
                        {
                            Sentence newSentence = new Sentence(s1.Subject, Verb.Is, n, Adverb.False);
                            SentenceBelief belief = new SentenceBelief(newSentence, b1, null, b1.mConfidence);
                            beliefDeductions.Add(belief);

                            Debug.Log("New belief: " + belief.mSentence + " (confidence: " + b1.mConfidence + ") | Since " + s1 + " (confidence: " + b1.mConfidence + ")");
                        }
                    }
                }

                t = s1.Subject.Type();
                nouns = t.GetMutuallyExclusiveNouns();
                if (nouns != null)
                {
                    foreach (Noun n in nouns)
                    {
                        if (s1.Subject != n)
                        {
                            Sentence newSentence = new Sentence(s1.DirectObject, Verb.Is, n, Adverb.False);
                            SentenceBelief belief = new SentenceBelief(newSentence, b1, null, b1.mConfidence);
                            beliefDeductions.Add(belief);

                            Debug.Log("New belief: " + belief.mSentence + " (confidence: " + b1.mConfidence + ") | Since " + s1 + " (confidence: " + b1.mConfidence + ")");
                        }
                    }
                }
            } // END MUTUAL EXCLUSIVITY


            // special rule: determining motive
            NounType subjectType = s1.Subject.Type();
            NounType objectType = s1.DirectObject.Type();
            if (subjectType == NounType.Motive)
            {
                Sentence newSentence = new Sentence(s1.DirectObject, Verb.Has, Noun.Motive, Adverb.True);
                SentenceBelief belief = new SentenceBelief(newSentence, b1, null, b1.mConfidence);
                beliefDeductions.Add(belief);
            }
            else if (objectType == NounType.Motive)
            {
                Sentence newSentence = new Sentence(s1.Subject, Verb.Has, Noun.Motive, Adverb.True);
                SentenceBelief belief = new SentenceBelief(newSentence, b1, null, b1.mConfidence);
                beliefDeductions.Add(belief);
            }
        }

        RemoveBeliefs(refutedBeliefs);
        AddBeliefs(beliefDeductions);
    }
    
    private void RemoveBeliefs(List<SentenceBelief> refutedBeliefs)
    {
        foreach(SentenceBelief b in refutedBeliefs)
        {
            RemoveBelief(b);
        }
    }

    private void GetRootBeliefs(SentenceBelief b, List<SentenceBelief> rootBeliefs, bool filterOutTrue)
    {
        if(!b.mDeduced)
        {
            if(!filterOutTrue || b.mConfidence < 1) rootBeliefs.Add(b);
            return;
        }

        GetRootBeliefs(b.mSourceBelief1, rootBeliefs, filterOutTrue);
        if (b.mSourceBelief2 != null) GetRootBeliefs(b.mSourceBelief2, rootBeliefs, filterOutTrue);
    }

    private void RemoveBelief(SentenceBelief b)
    {
        // if this belief is false, then at least 1 belief we used to arrive at it must be false
        List<SentenceBelief> rootBeliefs = new List<SentenceBelief>();
        GetRootBeliefs(b, rootBeliefs, true);

        mSuspiciousBeliefs.Add(rootBeliefs);

        Debug.Log(mPersonId + " no longer believes " + b.mSentence);
        mBeliefs.Remove(b); // TODO: this is probably too naive
    }
}
