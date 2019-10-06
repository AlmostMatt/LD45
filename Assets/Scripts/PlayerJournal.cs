using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerJournal
{
    public struct SentenceHistory
    {
        Sentence mSentence;
        int mSource;

        public SentenceHistory(Sentence s, int source)
        {
            mSentence = s;
            mSource = source;
        }

        public override string ToString()
        {
            if(mSource == -1)
            {
                return "You discovered that " + mSentence;
            }
            else
            {
                Noun hairColor = GameState.Get().mPeople[mSource].AttributeMap[NounType.HairColor];
                return hairColor + " said that " + mSentence;
            }
        }
    }

    static List<SentenceHistory> sSentences = new List<SentenceHistory>();

    public static void AddClue(ClueInfo c)
    {
        SentenceHistory h = new SentenceHistory(c.GetSentence(), -1);
        sSentences.Add(h);
    }

    public static void AddListen(int source, Sentence s)
    {
        SentenceHistory h = new SentenceHistory(s, source);
        sSentences.Add(h);
    }

    public static List<SentenceHistory> GetJournal()
    {
        return (sSentences);
    }
}
