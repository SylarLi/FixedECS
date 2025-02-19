using Fixed.Numeric;
using Unity.Entities;

namespace Fixed.ECS.TwoD
{
    public struct TRS : IComponentData
    {
        public fpmatrix4x4 Value;
    }
}