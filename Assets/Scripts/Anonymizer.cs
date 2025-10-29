using System;
using UnityEngine;

public static class Anonymizer
{
    public static int ToAgeBucketStart(int age, int bucket)
    {
        if (bucket <= 1) return age;
        return (age / bucket) * bucket;
    }
    public static string FormatAgeBucket(int start, int bucket)
        => bucket <= 1 ? start.ToString() : $"{start}-{start + bucket - 1}";

    public static string GeneralizeGeo(Suspect s, AnonymizationSettings.GeoLevel level)
    {
        return level switch {
            AnonymizationSettings.GeoLevel.Postcode => s.postcode,
            AnonymizationSettings.GeoLevel.District => $"District {s.district}",
            _ => "Province X" // simple demo
        };
    }

    public static string GeneralizeJob(Suspect s, AnonymizationSettings.JobLevel level)
        => level == AnonymizationSettings.JobLevel.Exact ? s.occupation : s.sector;

    public static int AddTinyNoiseToAge(int age, float epsilon)
    {
        // demo: epsilon small -> more chance of +-1 jitter
        var r = new System.Random(Guid.NewGuid().GetHashCode());
        bool jitter = epsilon < 0.75f;
        return Mathf.Clamp(age + (jitter ? (r.Next(0,2)==0?-1:1) : 0), 0, 120);
    }
}
