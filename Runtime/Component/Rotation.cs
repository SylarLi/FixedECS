using Fixed.Numeric;
using Unity.Entities;

namespace Fixed.ECS.TwoD
{
    public struct Rotation : IComponentData
    {
        // Degrees
        public fp Value;
    }
}