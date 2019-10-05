public enum Noun
{
    // One of the following should be present.
    // Person (Identified by hair color)
    // Name
    // Place
    // Object
    // Motive?
    // Profession?
    // Backstory?

    // HairColor,
    Blonde,
    Black,
    Red,
    // Name
    Alice,
    Beth,
    Carol,
}

// Extenstion methods can be called with attr.method(). For example, Noun.Blonde.Type()
public static class NounExtensions
{
    public static NounType Type(this Noun noun)
    {
        switch (noun)
        {
            case Noun.Blonde:
            case Noun.Black:
            case Noun.Red:
                return NounType.HairColor;
            default:
                return NounType.Name;
        }
    }
}