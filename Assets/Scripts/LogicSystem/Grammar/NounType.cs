public enum NounType
{
    HairColor,
    Name,
    Identity,
    Role
}

public static class NounTypeExtensions
{
    public static Noun[] GetNouns(this NounType nounType)
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
