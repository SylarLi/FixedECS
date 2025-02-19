using Fixed.Numeric;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace Fixed.ECS.TwoD
{
    public partial struct TRSSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAny<Position, Rotation, Scale>().Build();
            var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var job = new UpdateJob
            {
                EntityTypeHandle = state.GetEntityTypeHandle(),
                PositionTypeHandle = state.GetComponentTypeHandle<Position>(true),
                RotationTypeHandle = state.GetComponentTypeHandle<Rotation>(true),
                ScaleTypeHandle = state.GetComponentTypeHandle<Scale>(true),
                TRSTypeHandle = state.GetComponentTypeHandle<TRS>(false),
                CommandBuffer = commandBuffer.AsParallelWriter(),
                LastSystemVersion = state.LastSystemVersion
            };
            state.Dependency = job.ScheduleParallel(query, state.Dependency);
        }

        [BurstCompile]
        private struct UpdateJob : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityTypeHandle;

            [ReadOnly] public ComponentTypeHandle<Position> PositionTypeHandle;

            [ReadOnly] public ComponentTypeHandle<Rotation> RotationTypeHandle;

            [ReadOnly] public ComponentTypeHandle<Scale> ScaleTypeHandle;

            public ComponentTypeHandle<TRS> TRSTypeHandle;

            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            public uint LastSystemVersion;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var hasPosition = chunk.Has(ref PositionTypeHandle);
                var hasRotation = chunk.Has(ref RotationTypeHandle);
                var hasScale = chunk.Has(ref ScaleTypeHandle);
                var changed = hasPosition && chunk.DidChange(ref PositionTypeHandle, LastSystemVersion) ||
                              hasRotation && chunk.DidChange(ref RotationTypeHandle, LastSystemVersion) ||
                              hasScale && chunk.DidChange(ref ScaleTypeHandle, LastSystemVersion);
                if (!changed)
                    return;
                var positions = hasPosition ? chunk.GetNativeArray(ref PositionTypeHandle) : default;
                var rotations = hasRotation ? chunk.GetNativeArray(ref RotationTypeHandle) : default;
                var scales = hasScale ? chunk.GetNativeArray(ref ScaleTypeHandle) : default;
                var hasTRS = chunk.Has(ref TRSTypeHandle);
                var TRSs = hasTRS ? chunk.GetNativeArray(ref TRSTypeHandle) : default;
                var entities = chunk.GetNativeArray(EntityTypeHandle);
                var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out var i))
                {
                    var trs = new TRS
                    {
                        Value = fpmatrix4x4.TRS(hasPosition ? positions[i].Value : fpvec2.zero,
                                                hasRotation ? rotations[i].Value : fp.Zero,
                                                hasScale ? scales[i].Value : fpvec2.one)
                    };
                    if (hasTRS)
                        TRSs[i] = trs;
                    else
                        CommandBuffer.AddComponent(unfilteredChunkIndex, entities[i], trs);
                }
            }
        }
    }
}