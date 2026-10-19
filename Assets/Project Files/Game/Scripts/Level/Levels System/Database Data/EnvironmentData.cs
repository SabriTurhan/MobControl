using UnityEngine;

namespace SquadCommand
{
    [System.Serializable]
    public class EnvironmentData : AbstractData
    {
        [SerializeField] float lengthAlongZ;
        public float LengthAlongZ => lengthAlongZ;
    }
}
