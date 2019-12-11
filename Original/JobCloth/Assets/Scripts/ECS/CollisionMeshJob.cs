﻿
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[ExecuteAlways]
[AlwaysUpdateSystem]
[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(AccumulateForces_System))]
public class CollisionMesh_System : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entities.WithoutBurst().ForEach((ClothComponent cloth, ref LocalToWorld localToWorldParam) =>
        {
            var vertices = cloth.CurrentClothPosition;
            var oldVertices = cloth.PreviousClothPosition;
            var localToWorld = localToWorldParam.Value;
            var worldToLocal = math.inverse(localToWorldParam.Value);

            for (int i = 0; i < vertices.Length; ++i)
            {
                float3 oldVert = oldVertices[i];
                float3 vert = vertices[i];

                float3 worldPos = math.mul(localToWorld, new float4(vert, 1)).xyz;

                if (worldPos.y < 0f)
                {
                    float3 oldWorldPos = math.mul(localToWorld, new float4(oldVert, 1)).xyz;
                    oldWorldPos.y = (worldPos.y - oldWorldPos.y) * .5f;
                    worldPos.y = 0f;
                    vert = math.mul(worldToLocal, new float4(worldPos, 1)).xyz;
                    oldVert = math.mul(worldToLocal, new float4(oldWorldPos, 1)).xyz;
                }

                vertices[i] = vert;
                oldVertices[i] = oldVert;
            }
        }).Run();

        return inputDeps;
    }
}