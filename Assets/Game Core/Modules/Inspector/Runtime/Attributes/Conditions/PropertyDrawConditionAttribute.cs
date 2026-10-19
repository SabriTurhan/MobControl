using System;

namespace SquadCommand
{
    public class PropertyConditionAttribute : BaseAttribute
    {
        public PropertyConditionAttribute(Type targetAttributeType) : base(targetAttributeType)
        {
        }
    }
}
