using System;
using System.Collections.Generic;
using System.Linq;

public static class Metrics
{
    public static int ComputeK(IEnumerable<(int ageBucket,string gender,string geo,string job)> rows)
    {
        var groups = rows.GroupBy(r => (r.ageBucket, r.gender, r.geo, r.job));
        return groups.Any() ? groups.Min(g => g.Count()) : 0;
    }

    public static int ComputeL(IEnumerable<string> sensitiveValues)
        => new HashSet<string>(sensitiveValues).Count;

    public static float ComputeTCloseness(IEnumerable<string> group, IEnumerable<string> global)
    {
        var p = Dist(group); var q = Dist(global);
        var keys = new HashSet<string>(p.Keys.Concat(q.Keys));
        double tv = 0; foreach (var k in keys) { p.TryGetValue(k, out var a); q.TryGetValue(k, out var b); tv += Math.Abs(a-b); }
        return (float)(tv/2.0);
    }

    static Dictionary<string,double> Dist(IEnumerable<string> data)
    {
        var list = data.ToList(); double n = Math.Max(1, list.Count);
        return list.GroupBy(x=>x).ToDictionary(g=>g.Key, g=>g.Count()/n);
    }
}
