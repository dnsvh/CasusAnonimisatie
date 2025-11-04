using UnityEngine;

public class NPCDialogue : MonoBehaviour {
    [TextArea] public string introDutch = "Hoi! Ik zag iets…";
    public ClueType clueType = ClueType.AgeExact;

    // No hard Suspect dependency; just pass the fields we need
    public string GetClueLine(int killerAge, string killerGender, string killerDistrict, string killerPostcode, string killerOccupation, bool anonymized) {
        switch (clueType) {
            case ClueType.AgeExact:
                return anonymized ? $"Ze leken {NPCInteractable.AgeBin(killerAge)}."
                                  : $"Ze leken {killerAge} jaar oud.";
            case ClueType.AgeRange:
                return $"Ze leken {NPCInteractable.AgeBin(killerAge)}.";
            case ClueType.Gender:
                return $"Het was iemand van het geslacht {killerGender}.";
            case ClueType.District:
                return anonymized ? $"Ik denk uit dezelfde wijk, misschien met postcode die met {SafePrefix(killerPostcode,2)} begint."
                                  : $"Ik denk uit de wijk {killerDistrict}.";
            case ClueType.Postcode2:
                return anonymized ? $"Postcode begon met {SafePrefix(killerPostcode,2)}."
                                  : $"Postcode was exact {killerPostcode}.";
            case ClueType.Occupation:
                return anonymized ? $"Beroep? Geen idee, kan van alles zijn."
                                  : $"Ik meen een {killerOccupation} te hebben gezien.";
        }
        return "";
    }

    string SafePrefix(string s, int n) => string.IsNullOrEmpty(s) || s.Length < n ? s : s.Substring(0, n);
}
