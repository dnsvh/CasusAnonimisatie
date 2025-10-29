using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class Suspect {
    public int id; public string name; public int age; public string gender;
    public string postcode; public int district; public string occupation; public string sector;
    public string work_shift; public string sensitive_condition; public bool is_murderer;
}
[System.Serializable] public class SuspectsWrapper { public List<Suspect> items; }

public enum GameMode { Easy, Moderate, Hard, Custom }

[CreateAssetMenu(fileName="AnonymizationSettings", menuName="Anon/Settings")]
public class AnonymizationSettings : ScriptableObject {
    [Header("Generalization")]
    public int ageBucket = 10; // years
    public GeoLevel geoLevel = GeoLevel.District; // Postcode, District, Province
    public JobLevel jobLevel = JobLevel.Sector;   // Exact, Sector

    [Header("Targets / Constraints (display)")]
    public int kTarget = 5;
    public int lTarget = 2;
    public float tThreshold = 0.2f;

    [Header("Noise / DP-style")]
    public bool useNoise = false;
    [Range(0f,2f)] public float epsilon = 1.0f;

    public enum GeoLevel { Postcode, District, Province }
    public enum JobLevel { Exact, Sector }
}
