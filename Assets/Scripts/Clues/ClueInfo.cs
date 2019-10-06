
public class ClueInfo
{
    public Noun nounA;
    public Noun nounB;

    public ClueInfo(Noun a, Noun b)
    {
        nounA = a;
        nounB = b;
    }

    public Sentence GetSentence()
    {
        return new Sentence(nounA, Verb.Is, nounB, Adverb.True);
    }
}
