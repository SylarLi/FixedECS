using Fixed.Numeric;
using Unity.Entities;

namespace Fixed.ECS.TwoD
{
    public struct Position : IComponentData
    {
        public fpvec2 Value;
    }
}