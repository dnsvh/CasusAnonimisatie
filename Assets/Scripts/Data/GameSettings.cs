using UnityEngine;

[CreateAssetMenu(fileName="GameSettings", menuName="Casus/Settings")]
public class GameSettings : ScriptableObject
{
    [System.Serializable]
    public class UIStrings {
        public string timer = "Tijd";
        public string suspectList = "Verdachten";
        public string accuse = "Beschuldig";
        public string metrics = "Privacy-metrics";
        public string kAnon = "k-anonimiteit";
        public string lDiv = "l-diversiteit";
        public string leaderboard = "Ranglijst";
        public string back = "Terug";
        public string save = "Opslaan";
        public string nickname = "Bijnaam";
    }
    public UIStrings ui = new UIStrings();
}
