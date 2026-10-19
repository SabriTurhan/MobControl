using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquadCommand
{
    public interface ILevelInitable
    {
        void Init(AbstractLevelData data);
    }
}
