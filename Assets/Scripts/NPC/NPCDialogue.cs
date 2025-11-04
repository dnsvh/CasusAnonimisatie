using UnityEngine;
public class NPCDialogue : MonoBehaviour
{
    [TextArea] public string introDutch = "Hoi! Ik heb iets gezien.";
    public ClueType clueType = ClueType.AgeExact;

    // Generate the line shown to the player.
    // Round A => precise; Round B => generalized.
    public string GetClueLine(Suspect target, bool anonymized)
    {
        int age = target.age;
        string gender = target.gender;
        string district = target.district;
        string postcode = target.postcode;
        string occupation = target.occupation;

        switch (clueType)
        {
            case ClueType.AgeExact:
                return anonymized
                    ? $"Ze leken {Anonymizer.AgeBin(age)}."
                    : $"Ze leken ongeveer {age} jaar.";

            case ClueType.AgeRange:
                // FORCE precise in Round A, generalized in Round B
                return anonymized
                    ? $"Ze leken {Anonymizer.AgeBin(age)}."
                    : $"Ze leken ongeveer {age} jaar.";

            case ClueType.Gender:
                return $"Ik denk dat het een {(gender == "M" ? "man" : "vrouw")} was.";

            case ClueType.District:
                return anonymized
                    ? $"Ik zag ze in de buurt van {district}."
                    : $"Ik zag ze in {district}.";

            case ClueType.Postcode2:
                // FORCE precise in Round A, generalized in Round B
                return anonymized
                    ? $"Postcode begon met {Anonymizer.GeneralizePostcode(postcode, 2)}."
                    : $"De postcode was {postcode}.";

            case ClueType.Occupation:
                return anonymized
                    ? $"Het leek alsof ze aan het werk waren."
                    : $"Ik hoorde dat ze werken als {occupation}.";

            default:
                return "Ik weet het niet zeker.";
        }
    }
    public string GetClueLine(Suspect target, bool anonymized, ClueType typeOverride)
    {
        int age = target.age;
        string gender = target.gender;
        string district = target.district;
        string postcode = target.postcode;
        string occupation = target.occupation;

        switch (typeOverride)
        {
            case ClueType.AgeExact:
                // In RoundB we generalize age to a bin; in RoundA we show exact
                return anonymized
                    ? $"Ze leken {Anonymizer.AgeBin(age)}."
                    : $"Ze leken ongeveer {age} jaar.";

            case ClueType.AgeRange:
                return anonymized
                    ? $"Ze leken {Anonymizer.AgeBin(age)}."
                    : $"Ze leken ongeveer {age} jaar.";

            case ClueType.Gender:
                return $"Ik denk dat het een {(gender == "M" ? "man" : "vrouw")} was.";

            case ClueType.District:
                return anonymized
                    ? $"Ik zag ze in de buurt van {district}."
                    : $"Ik zag ze in {district}.";

            case ClueType.Postcode2:
                return anonymized
                    ? $"Postcode begon met {Anonymizer.GeneralizePostcode(postcode, 2)}."
                    : $"De postcode was {postcode}.";

            case ClueType.Occupation:
                return anonymized
                    ? $"Het leek alsof ze aan het werk waren."
                    : $"Ik hoorde dat ze werken als {occupation}.";

            default:
                return "Ik weet het niet zeker.";
        }
    }
}
