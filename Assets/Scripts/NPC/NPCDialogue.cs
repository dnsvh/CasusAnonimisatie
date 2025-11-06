using UnityEngine;
public class NPCDialogue : MonoBehaviour
{
    [TextArea] public string introDutch = "Hoi! Ik heb iets gezien.";
    public ClueType clueType = ClueType.AgeExact;


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
                    ? $"De persoon leek {Anonymizer.AgeBin(age)}."
                    : $"De persoon leek ongeveer {age} jaar.";

            case ClueType.AgeRange:

                return anonymized
                    ? $"De persoon leek {Anonymizer.AgeBin(age)}."
                    : $"De persoon leek ongeveer {age} jaar.";

            case ClueType.Gender:
                return $"Ik denk dat het een {(gender == "M" ? "man" : "vrouw")} was.";

            case ClueType.District:
                return anonymized
                    ? $"Ik zag die persoon in de buurt van {district}."
                    : $"Ik zag die persoon in {district}.";

            case ClueType.Postcode2:

                return anonymized
                    ? $"Postcode begon met {Anonymizer.GeneralizePostcode(postcode, 2)}."
                    : $"De postcode was {postcode}.";

            case ClueType.Occupation:
                return anonymized
                    ? $"Het leek alsof die persoon aan het werk was."
                    : $"Ik hoorde dat die persoon werkte als {occupation}.";

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

                return anonymized
                    ? $"De persoon leek {Anonymizer.AgeBin(age)}."
                    : $"De persoon leek ongeveer {age} jaar.";

            case ClueType.AgeRange:
                return anonymized
                    ? $"De persoon leek {Anonymizer.AgeBin(age)}."
                    : $"De persoon leek {age} jaar.";

            case ClueType.Gender:
                return $"Ik denk dat het een {(gender == "M" ? "man" : "vrouw")} was.";

            case ClueType.District:
                return anonymized
                    ? $"Ik zag die persoon in de buurt van {district}."
                    : $"Ik zag die persoon in {district}.";

            case ClueType.Postcode2:
                return anonymized
                    ? $"Postcode begon met {Anonymizer.GeneralizePostcode(postcode, 2)}."
                    : $"De postcode was {postcode}.";

            case ClueType.Occupation:
                return anonymized
                    ? $"Het leek alsof die persoon aan het werk was."
                    : $"Ik hoorde dat die persoon werkt als {occupation}.";

            default:
                return "Ik weet het niet zeker.";
        }
    }
}
