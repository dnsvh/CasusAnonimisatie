using System.Collections.Generic;
using System.Linq;

public static class MetricsCalculator
{
    // Very basic k/l: group by age bin + gender + postcode-2
    public static (int k, int l) KLForDataset(IEnumerable<Suspect> data)
    {
        if (data == null) return (0, 0);
        var groups = data
            .GroupBy(s => (AgeBin(s.age), s.gender, Post2(s.postcode)))
            .ToList();

        int k = groups.Count == 0 ? 0 : groups.Min(g => g.Count());
        int l = groups.Count == 0 ? 0 : groups.Min(g => g.Select(x => x.occupation).Distinct().Count());

        return (k, l);
    }

    static string AgeBin(int age)
    {
        if (age < 25) return "18-24";
        if (age < 30) return "25-29";
        if (age < 40) return "30-39";
        if (age < 50) return "40-49";
        if (age < 60) return "50-59";
        return "60+";
    }

    static string Post2(string p)
    {
        if (string.IsNullOrEmpty(p)) return "";
        return p.Length < 2 ? p : p.Substring(0, 2);
    }
}
