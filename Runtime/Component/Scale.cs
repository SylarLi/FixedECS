using Fixed.Numeric;
using Unity.Entities;

namespace Fixed.ECS.TwoD
{
    public struct Scale : IComponentData
    {
        public fpvec2 Value;
    }
}