using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MetricsCalculator
{
    /// <summary>
    /// Everything the HUD wants in one pass.
    /// Values are computed on the "remaining" suspects the HUD passes in.
    /// In Round B, age buckets are coarser so groups should be larger.
    /// </summary>
    public static MetricsResult ComputeAll(IReadOnlyList<Suspect> suspects, string killerId, bool roundB)
    {
        var result = new MetricsResult();

        if (suspects == null || suspects.Count == 0)
            return result; // all zeros

        var groups = GroupByQIs(suspects, roundB); // List<IGrouping<QIKey, Suspect>>
        if (groups.Count == 0)
            return result;

        // K-anonymity stats
        var kSizes = groups.Select(g => g.Count()).OrderBy(n => n).ToArray();
        result.kMin = Math.Max(1, kSizes.First());
        result.kMedian = kSizes[kSizes.Length / 2];
        result.kMax = kSizes.Last();
        result.kAvg = (float)kSizes.Average();

        // L-diversity per group (distinct occupations inside group)
        var lSizes = groups.Select(g =>
                g.Select(s => Safe(s.occupation))
                 .Where(x => !string.IsNullOrWhiteSpace(x))
                 .Distinct(StringComparer.OrdinalIgnoreCase)
                 .Count()
            )
            .OrderBy(n => n)
            .ToArray();

        if (lSizes.Length > 0)
        {
            result.lMin = Math.Max(1, lSizes.First());
            result.lMedian = lSizes[lSizes.Length / 2];
            result.lMax = lSizes.Last();
        }

        // Killer stats (if killer is still among remaining)
        if (!string.IsNullOrEmpty(killerId))
        {
            var killer = suspects.FirstOrDefault(s => s.id == killerId);
            if (killer != null)
            {
                var key = MakeKey(killer, roundB);
                var killerGroup = groups.FirstOrDefault(g => g.Key.Equals(key));
                if (killerGroup != null)
                {
                    result.killerK = killerGroup.Count();
                    result.killerL = killerGroup.Select(s => Safe(s.occupation))
                                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                                .Count();
                }
            }
        }

        result.groupCount = groups.Count;
        result.remainingCount = suspects.Count;

        return result;
    }

    /// <summary>
    /// Back-compat used by older code paths (k,l only).
    /// </summary>
    public static (int k, int l) KLForDataset(IEnumerable<Suspect> suspectsEnum)
    {
        var suspects = suspectsEnum?.ToList() ?? new List<Suspect>();
        if (suspects.Count == 0) return (0, 0);

        bool roundB = GameManager.I != null && GameManager.I.phase == RoundPhase.RoundB;
        var groups = GroupByQIs(suspects, roundB);

        if (groups.Count == 0) return (0, 0);

        int k = Math.Max(1, groups.Min(g => g.Count()));
        int l = Math.Max(1, groups.Min(g =>
            g.Select(s => Safe(s.occupation))
             .Where(x => !string.IsNullOrWhiteSpace(x))
             .Distinct(StringComparer.OrdinalIgnoreCase)
             .Count()
        ));

        return (k, l);
    }

    // ----------------- internals -----------------

    static List<IGrouping<QIKey, Suspect>> GroupByQIs(IReadOnlyList<Suspect> suspects, bool roundB)
    {
        return suspects
            .GroupBy(s => MakeKey(s, roundB))
            .ToList();
    }

    static QIKey MakeKey(Suspect s, bool roundB)
    {
        return new QIKey(
            AgeBucket(s.age, roundB),
            Safe(s.gender),
            Safe(s.district)
        );
    }

    // 5-year buckets in Round A; 10-year buckets in Round B (coarser)
    static string AgeBucket(int age, bool roundB)
    {
        if (age < 0) return "?";
        int w = roundB ? 10 : 5;
        int start = (age / w) * w;
        int end = start + w - 1;
        return $"{start}–{end}";
    }

    static string Safe(string s) => string.IsNullOrEmpty(s) ? "" : s.Trim();

    // Value-type key for grouping
    readonly struct QIKey : IEquatable<QIKey>
    {
        public readonly string ageBucket;
        public readonly string gender;
        public readonly string district;

        public QIKey(string ageBucket, string gender, string district)
        {
            this.ageBucket = ageBucket ?? "";
            this.gender = gender ?? "";
            this.district = district ?? "";
        }

        public bool Equals(QIKey other) =>
            ageBucket == other.ageBucket &&
            string.Equals(gender, other.gender, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(district, other.district, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) => obj is QIKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + ageBucket.GetHashCode();
                h = h * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(gender);
                h = h * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(district);
                return h;
            }
        }
    }

    // What we return to HUD
    public struct MetricsResult
    {
        public int kMin, kMedian, kMax;
        public float kAvg;
        public int lMin, lMedian, lMax;

        public int killerK, killerL;
        public int groupCount, remainingCount;
    }
}
