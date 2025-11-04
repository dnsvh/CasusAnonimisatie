using System.Collections.Generic;

[System.Serializable]
public class Suspect {
    public string id;
    public string name;
    public int age;
    public string gender;
    public string district;
    public string postcode;
    public string occupation;
    public bool eliminated;
}

[System.Serializable]
public class SuspectDataset {
    public List<Suspect> suspects = new List<Suspect>();
}
