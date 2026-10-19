using System.Collections.Generic;

namespace SquadCommand
{
    [System.Serializable]
    public class CardSave
    {
        public string id;
        public int level;
        public int copies;
    }

    [System.Serializable]
    public class CampaignSave : ISaveObject
    {
        public List<CardSave> units = new List<CardSave>();
        public List<CardSave> weapons = new List<CardSave>();

        public List<string> squad = new List<string>();
        public string weapon;

        public int[] buildings = new int[0];

        public bool introSeen;
        public List<int> storyBeatsSeen = new List<int>();

        public long lastIncomeTicks;
        public int pendingChests;

        public void Flush() { }
    }
}
