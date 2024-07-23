﻿using System;
 using _Game_.Scripts.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Game_.Scripts.Systems.Other
{
    [BurstCompile,UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct ItemSystem : ISystem
    {
        private NativeArray<BufferTurretObstacle> _buffetObstacle;
        private EntityManager _entityManager;
        private bool _isInit;
        private float _time;

        #region OnCreate

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            RequireNecessaryComponents(ref state);
        }
        [BurstCompile]
        private void RequireNecessaryComponents(ref SystemState state)
        {
            state.RequireForUpdate<PlayerInfo>();
        }

        #endregion
        

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (_buffetObstacle.IsCreated)
                _buffetObstacle.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if(!CheckAndInit(ref state)) return;
            _time += SystemAPI.Time.DeltaTime;
            CheckObstacleItem(ref state);
            CheckItemShooting(ref state);
        }

        [BurstCompile]
        private bool CheckAndInit(ref SystemState state)
        {
            if (_isInit) return true;
            _isInit = true;
            _entityManager = state.EntityManager;
            _buffetObstacle = SystemAPI.GetSingletonBuffer<BufferTurretObstacle>().ToNativeArray(Allocator.Persistent);
            _time = (float)SystemAPI.Time.ElapsedTime;
            return false;
        }

        [BurstCompile]
        private void CheckItemShooting(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);


            var time = (float)SystemAPI.Time.ElapsedTime;
            float timeEffect = 0.1f;

            foreach (var (hitCheckOverride, hitTime, entity) in SystemAPI
                         .Query<RefRW<HitCheckOverride>, RefRW<HitCheckTime>>().WithEntityAccess().WithAll<ItemInfo>().WithNone<Disabled,TakeDamage>())
            {
                if(time - hitTime.ValueRO.time < timeEffect) continue;
                hitCheckOverride.ValueRW.Value -= 1;

                if (hitCheckOverride.ValueRO.Value > 0)
                {
                    hitTime.ValueRW.time = time - timeEffect / 2f;
                }
                else
                {
                    ecb.RemoveComponent<HitCheckTime>(entity);
                }
            }
            
            
            var hitCheckTime = new HitCheckTime()
            {
                time = time,
            };
            foreach (var (itemInfo, takeDamage,hitCheckOverride, entity) in SystemAPI.Query<RefRW<ItemInfo>, RefRO<TakeDamage>,RefRW<HitCheckOverride>>()
                         .WithEntityAccess().WithNone<Disabled,AddToBuffer>())
            {
                itemInfo.ValueRW.hp -= (int)takeDamage.ValueRO.value;
                ecb.RemoveComponent<TakeDamage>(entity);
                ecb.AddComponent(entity,new TextMeshData()
                {
                    id = itemInfo.ValueRO.idTextHp,
                    text = itemInfo.ValueRO.hp.ToString(),
                });
                if (itemInfo.ValueRO.hp <= 0)
                {
                    var entityNEw = ecb.CreateEntity();
                    var entityChangeText = ecb.CreateEntity();
                    ecb.AddComponent(entityChangeText, new TextMeshData()
                    {
                        id = itemInfo.ValueRO.idTextHp,
                        text = "0",
                    });
                    if (_entityManager.HasBuffer<BufferSpawnPoint>(entity))
                    {
                        var buffer = ecb.AddBuffer<BufferSpawnPoint>(entityNEw);
                        buffer.CopyFrom(_entityManager.GetBuffer<BufferSpawnPoint>(entity));
                    }
                    ecb.AddComponent(entityNEw,new ItemCollection()
                    {
                        count = itemInfo.ValueRO.count,
                        entityItem = entityNEw,
                        id = itemInfo.ValueRO.id,
                        type = itemInfo.ValueRO.type,
                        operation = itemInfo.ValueRO.operation,
                    });
                    ecb.AddComponent(entity,new SetActiveSP()
                    {
                        state = DisableID.DestroyAll
                    });
                }
                else
                {
                    if (_entityManager.HasComponent<HitCheckTime>(entity))
                    {
                        var hitCheck = _entityManager.GetComponentData<HitCheckTime>(entity);
                        if(time - hitCheck.time < timeEffect) continue;
                    }
                    
                    
                    
                    var value = hitCheckOverride.ValueRW.Value;
                    
                    value += 1;
                    if (value > 2)
                    {
                        value = 1;
                    }

                    if (value - 1 == 0)
                    {
                        hitCheckTime.time -= timeEffect / 2f;
                    }
                    ecb.AddComponent(entity,hitCheckTime);
                    hitCheckOverride.ValueRW.Value = value;
                }
            }
            
            ecb.Playback(_entityManager);
            ecb.Dispose();
        }
        [BurstCompile]
        private void CheckObstacleItem(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (collection,entity) in SystemAPI.Query<RefRO<ItemCollection>>().WithEntityAccess()
                         .WithNone<Disabled, SetActiveSP>())
            {
                switch (collection.ValueRO.type)
                {
                    case ItemType.ObstacleTurret:
                        SpawnTurret(ref state,ref ecb,collection.ValueRO);
                        ecb.AddComponent(entity,new SetActiveSP()
                        {
                            state = DisableID.Disable
                        });
                        break;
                }
                
            }
            ecb.Playback(_entityManager);
            ecb.Dispose();
        }
        [BurstCompile]
        private BufferTurretObstacle GetTurret(int id)
        {
            foreach (var i in _buffetObstacle)
            {
                if (i.id == id) return i;
            }

            return new BufferTurretObstacle()
            {
                id = -1,
            };
        }
        
        [BurstCompile]
        private void SpawnTurret(ref SystemState state,ref EntityCommandBuffer ecb,ItemCollection itemCollection)
        {
            BufferTurretObstacle buffetObstacle = default;
            bool check = false;
            foreach (var obs in _buffetObstacle)
            {
                if(obs.id != itemCollection.id) continue;
                buffetObstacle = obs;
                check = true;
                break;
            }
            if(!check) return;
            var turret = GetTurret(itemCollection.id);
            if(turret.id == -1) return;
            var points = _entityManager.GetBuffer<BufferSpawnPoint>(itemCollection.entityItem);
            LocalTransform lt = new LocalTransform()
            {
                Scale = 1,
                Rotation = quaternion.identity
            };
            foreach (var point in points)
            {
                var newObs = ecb.Instantiate(buffetObstacle.entity);
                lt.Position = point.value;
                ecb.AddComponent(newObs,lt);
                ecb.AddComponent(newObs,new TurretInfo()
                {
                    id = itemCollection.id,
                    type = ObstacleType.Turret,
                    timeLife = turret.timeLife,
                    startTime = _time,
                });
            }
            points.Clear();
        }
    }
}