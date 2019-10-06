public enum NounType
{
    Identity,
    HairColor,
    Name,    
    Role,
    Backstory,
    Motive
}

public static class NounTypeExtensions
{
    public static Noun[] GetMutuallyExclusiveNouns(this NounType nounType) // this is for the AI to make deductions based on mutual exclusion
    {
        switch (nounType)
        {
            case NounType.HairColor:
                return new Noun[] { Noun.Blonde, Noun.Brown, Noun.Red };
            case NounType.Identity:
                return new Noun[] { Noun.Exwife, Noun.Daughter, Noun.Bastard };
            case NounType.Role:
                return new Noun[] { Noun.Killer };
            case NounType.Name:
                return new Noun[] { Noun.Alice, Noun.Brianna, Noun.Catherine };
            default:
                return null;
        }
    }
}
