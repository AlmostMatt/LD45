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

    // Name
    //https://ancestorville.com/blogs/articles/victorian-female-nicknames
    Alice, // Anna, Amanda, Alex
    Brianna, // Beth, Belle, 
    Catherine, // Carol,
    // Identity
    Exwife,
    Daughter,
    Bastard,
    // HairColor,
    Blonde,
    Brown,
    Red,
    // Role (special)
    Victim,
    Killer,
    // Motive
    Debt,
    Inheritance,
    Grudge,
    // Backstory
    Philanthropist,
    Writer,
    Scientist,
    Artist


    // When adding new values to this enum, also update the switch statement below.
}

// Extenstion methods can be called with attr.method(). For example, Noun.Blonde.Type()
public static class NounExtensions
{
    public static NounType Type(this Noun noun)
    {
        switch (noun)
        {
            case Noun.Blonde:
            case Noun.Brown:
            case Noun.Red:
                return NounType.HairColor;
            case Noun.Exwife:
            case Noun.Daughter:
            case Noun.Bastard:
                return NounType.Identity;
            case Noun.Killer:
            case Noun.Victim:
                return NounType.Role;
            case Noun.Debt:
            case Noun.Inheritance:
            case Noun.Grudge:
                return NounType.Motive;
            case Noun.Philanthropist:
            case Noun.Writer:
            case Noun.Scientist:
            case Noun.Artist:
                return NounType.Backstory;
            case Noun.Alice:
            case Noun.Brianna:
            case Noun.Catherine:
            default:
                return NounType.Name;
        }
    }
}