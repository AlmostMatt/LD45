
public class ClueInfo
{
    public Noun nounA;
    public Noun nounB;
    private Verb verb;

    public ClueInfo(Noun a, Noun b, Verb v = Verb.Is)
    {
        nounA = a;
        nounB = b;
        verb = v;
    }

    public Sentence GetSentence()
    {
        return new Sentence(nounA, verb, nounB, Adverb.True);
    }
}
