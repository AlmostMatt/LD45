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
    Victor, // The victim!
    // Identity
    ExWife,
    Daughter,
    Mistress,
    // HairColor,
    Blonde,
    Brunette,
    Redhead,
    // Role (special)
    Victim,
    Killer,
    // Name in blood
    SuspectedName,
    // Motive
    OwesDebt,
    Inheritance,
    HasGrudge,
    Abused, // Currently unused, effectively the same as grudge
    // Backstory
    Philanthropist,
    Writer,
    Scientist,
    Artist,
    // Special flag for generalizing motive
    Motive,
    // Special nouns for unique objects related to the lore
    Potion,
    MemoryLoss,

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
            case Noun.Brunette:
            case Noun.Redhead:
                return NounType.HairColor;
            case Noun.ExWife:
            case Noun.Daughter:
            case Noun.Mistress:
                return NounType.Identity;
            case Noun.Killer:
            case Noun.Victim:
                return NounType.Role;
            case Noun.OwesDebt:
            case Noun.Inheritance:
            case Noun.HasGrudge:
            case Noun.Abused:
                return NounType.Motive;
            case Noun.Philanthropist:
            case Noun.Writer:
            case Noun.Scientist:
            case Noun.Artist:
                return NounType.Backstory;
            case Noun.Motive:
                return NounType.HasMotive;
            case Noun.SuspectedName:
                return NounType.SuspectedName;
            case Noun.Potion:
            case Noun.MemoryLoss:
                return NounType.Unique;
            case Noun.Alice:
            case Noun.Brianna:
            case Noun.Catherine:
            case Noun.Victor:
            default:
                return NounType.Name;

        }
    }

    public static string AsPersonal(this Noun noun)
    {
        switch(noun)
        {
            case Noun.Inheritance:
                return "I'm getting an inheritance from the victim";
            case Noun.HasGrudge:
                return "I held a grudge against the victim";
            case Noun.OwesDebt:
                return "I owed the vitcim money";
            default:
                return "I'm " + noun.AsSubject();
        }
    }

    public static string AsSubject(this Noun noun)
    {
        switch(noun)
        {
            case Noun.Potion:
            case Noun.MemoryLoss:
                return "the potion that";
            case Noun.ExWife:
                return "the victim's " + bold("ex-wife");
            case Noun.Daughter:
                return "the victim's " + bold("daughter");
            case Noun.Mistress:
                return "the victim's " + bold("mistress");
            case Noun.Artist:
                return "an " + bold(noun.ToString().ToLower());
            case Noun.Scientist:
            case Noun.Writer:
            case Noun.Philanthropist:
                return "a " + bold(noun.ToString().ToLower());
            case Noun.OwesDebt:
                return "the debt";
            default:
                // This will happen for names
                return bold(noun.ToString());
        }
    }

    public static string AsObject(this Noun noun, bool positive = true)
    {
        string isString = (positive ? "is " : "is not ");
        string hasString = (positive ? "has " : "does not have ");
        switch(noun)
        {
            case Noun.Potion:
            case Noun.MemoryLoss:
                return "caused our memory loss";
            case Noun.ExWife:
                return isString + "the victim's " + bold("ex-wife");
            case Noun.Daughter:
                return isString + "the victim's " + bold("daughter");
            case Noun.Mistress:
                return isString + "the victim's " + bold("mistress");
            case Noun.OwesDebt:
                return (positive ? bold("owes") : "does not " + bold("owe")) + " the victim money";
            case Noun.Inheritance:
                return (positive ? "will " : "will not ") + "receive an " + bold("inheritance") + " from the victim";
            case Noun.HasGrudge:
                return (positive ? "held " : "did not hold ") + "a " + bold("grudge") + " against the victim";
            case Noun.Abused:
                return (positive ? "was  " : "was not ") + "" + bold("abused") + " by the victim";
            case Noun.Redhead:
            case Noun.Brunette:
            case Noun.Blonde:
                return isString + "a " + bold(noun.ToString().ToLower());
            case Noun.Alice:
            case Noun.Brianna:
            case Noun.Catherine:
                return isString + "named " + bold(noun.ToString());
            case Noun.Killer:
                return isString + "the " + bold("murderer");
            case Noun.Artist:
                return isString + "an " + bold(noun.ToString().ToLower());
            case Noun.Scientist:
            case Noun.Writer:
            case Noun.Philanthropist:
                return isString + "a " + bold(noun.ToString().ToLower());
            case Noun.SuspectedName:
                return "was written in blood next to the victim's body";
            default:
                // I dont think this is necessary
                return isString + bold(noun.ToString());
        }
    }

    public static string WithVictim(this Noun noun)
    {
        switch(noun)
        {
            case Noun.ExWife:
                return "his " + bold("ex-wife");
            case Noun.Daughter:
                return "his " + bold("daughter");
            case Noun.Mistress:
                return "his " + bold("mistress");
            default:
                return noun.ToString();
        }
    }

    public static string PersonalReaction(this Noun noun)
    {
        switch(noun)
        {
            case Noun.ExWife:
                return "...is that supposed to make me feel better about all this?";
            case Noun.Daughter:
                return "Dad...";
            case Noun.Mistress:
                return "Oh...";
            default:
                return "...";
        }
    }

    public static bool UseHas(this Noun noun)
    {
        switch(noun)
        {
            case Noun.HasGrudge:
            case Noun.OwesDebt:
            case Noun.Motive:
            case Noun.Inheritance:
            case Noun.Abused:
                return true;
            default:
                return false;
        }
    }

    private static string bold(string str)
    {
        return Utilities.bold(str);
    }
}