using System;

public static class Anonymizer
{
    public static SuspectDataset Generalize(SuspectDataset src)
    {
        if (src == null) return null;
        var copy = new SuspectDataset();
        foreach (var s in src.suspects)
        {
            copy.suspects.Add(new Suspect {
                id = s.id,
                name = s.name,
                age = s.age, // we bin in logic/UI when needed
                gender = s.gender,
                district = s.district,
                postcode = GeneralizePostcode(s.postcode, 2), // keep first 2 chars
                occupation = s.occupation,
                eliminated = false
            });
        }
        return copy;
    }

    public static string AgeBin(int age)
    {
        if (age < 25) return "18-24";
        if (age < 30) return "25-29";
        if (age < 40) return "30-39";
        if (age < 50) return "40-49";
        if (age < 60) return "50-59";
        return "60+";
    }

    public static string GeneralizePostcode(string postcode, int prefixLen)
    {
        if (string.IsNullOrEmpty(postcode) || postcode.Length <= prefixLen) return postcode;
        return postcode.Substring(0, prefixLen) + new string('X', Math.Max(0, postcode.Length - prefixLen));
    }
}
