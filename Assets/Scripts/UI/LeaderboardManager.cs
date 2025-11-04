using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class ScoreEntry { public string name; public float time; }
[Serializable]
public class ScoreTable { public List<ScoreEntry> scores = new(); }

public static class LeaderboardManager
{
    const string FileName = "leaderboard.json";

    public static ScoreTable Load()
    {
        var t = DataService.LoadFromPersistent<ScoreTable>(FileName);
        return t ?? new ScoreTable();
    }

    public static void Save(ScoreTable t)
    {
        DataService.SaveToPersistent(FileName, t);
    }

    public static bool MayEnter(float totalTime, int size)
    {
        var t = Load();
        if (t.scores.Count < size) return true;
        return totalTime < t.scores.Max(s => s.time);
    }

    public static bool IsNicknameValid(string nick, string regex)
    {
        return Regex.IsMatch(nick, regex);
    }

    public static void AddScore(string name, float time, int size)
    {
        var t = Load();
        t.scores.Add(new ScoreEntry { name = name, time = time });
        t.scores = t.scores.OrderBy(s => s.time).Take(size).ToList();
        Save(t);
    }
}
