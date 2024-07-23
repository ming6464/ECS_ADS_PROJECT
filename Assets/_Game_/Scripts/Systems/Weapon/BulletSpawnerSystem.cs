﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Game_.Scripts.Systems.Weapon
{
    [BurstCompile, UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct BulletSpawnerSystem : ISystemSupport
    {
        private EntityManager _entityManager;
        private Entity _entityWeaponAuthoring;
        private Entity _entityBulletInstantiate;
        private bool _isInit;
        private WeaponProperty _weaponProperty;
        private float _ratioDamage;

        public bool IsInitialized { get; set; }

        public void Init(ref SystemState state) {}
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _entityManager = state.EntityManager;
            if (!_isInit)
            {
                _isInit = true;
                _weaponProperty = SystemAPI.GetSingleton<WeaponProperty>();
                _entityBulletInstantiate = _weaponProperty.entityBullet;
                _entityWeaponAuthoring = SystemAPI.GetSingletonEntity<WeaponProperty>();
            }

            CalculateRatioDamage(ref state);
            SpawnBullet(ref state);
        }
        [BurstCompile]
        public void RequireNecessaryComponents(ref SystemState state)
        {
            state.RequireForUpdate<WeaponProperty>();
        }
        [BurstCompile]
        private void CalculateRatioDamage(ref SystemState state)
        {
            var time = math.clamp((float)SystemAPI.Time.ElapsedTime,_weaponProperty.timeRange.x, _weaponProperty.timeRange.y);
            _ratioDamage = math.remap(_weaponProperty.timeRange.x, _weaponProperty.timeRange.y, _weaponProperty.damageRangeRatio.x, _weaponProperty.damageRangeRatio.y,
                time);
        }
        [BurstCompile]
        private void SpawnBullet(ref SystemState state)
        {
            var bufferBulletSpawn = _entityManager.GetBuffer<BufferBulletSpawner>(_entityWeaponAuthoring);
            if (bufferBulletSpawn.Length == 0) return;
            var bulletSpawnerArr = bufferBulletSpawn.ToNativeArray(Allocator.TempJob);
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var bufferBulletDisables = _entityManager.GetBuffer<BufferBulletDisable>(_entityWeaponAuthoring);
            float time = (float)SystemAPI.Time.ElapsedTime;

            for (int index = 0; index < bulletSpawnerArr.Length; index++)
            {
                var bulletSpawn = bulletSpawnerArr[index];
                float subtractIndex = 0.5f;
                int halfNumberPreShot = (int)math.ceil(bulletSpawn.bulletPerShot / 2f);
                var lt = bulletSpawn.lt;
                var angleRota = MathExt.QuaternionToFloat3(lt.Rotation);
                float damage = bulletSpawn.damage * _ratioDamage;
                float speed = bulletSpawn.speed;

                if (halfNumberPreShot % 2 != 0)
                {
                    InstantiateBullet_L( lt, damage, speed, _entityBulletInstantiate);
                    --halfNumberPreShot;
                    subtractIndex = 0;
                }

                for (int i = 1; i <= halfNumberPreShot; i++)
                {
                    float space = (i - subtractIndex) * (bulletSpawn.parallelOrbit ? bulletSpawn.spacePerBullet : bulletSpawn.spaceAnglePerBullet);
                    if (bulletSpawn.parallelOrbit)
                    {
                        InstantiateParallelBullets_L(bulletSpawn.lt,bulletSpawn.right, damage, speed, space,_entityBulletInstantiate);
                        
                    }
                    else
                    {
                        InstantiateAngularBullets_L(bulletSpawn.lt, damage, speed, space, angleRota,_entityBulletInstantiate);
                        
                    }
                }
            }

            ecb.Playback(_entityManager);
            bulletSpawnerArr.Dispose();
            _entityManager.GetBuffer<BufferBulletSpawner>(_entityWeaponAuthoring).Clear();
            ecb.Dispose();
            
            void InstantiateAngularBullets_L(LocalTransform lt, float damage, float speed, float space, float3 angleRota, Entity entityBulletInstantiate)
            {
                float3 angleRotaNew = angleRota;
                float angle1 = angleRotaNew.y + space;
                float angle2 = angleRotaNew.y - space;
                angleRotaNew.y = angle1;
                lt.Rotation = MathExt.Float3ToQuaternion(angleRotaNew);
                InstantiateBullet_L( lt, damage, speed, entityBulletInstantiate);
                angleRotaNew.y = angle2;
                lt.Rotation = MathExt.Float3ToQuaternion(angleRotaNew);
                InstantiateBullet_L( lt, damage, speed, entityBulletInstantiate);
            }
            
            void InstantiateParallelBullets_L(LocalTransform lt,float3 right, float damage, float speed, float space,Entity entityBulletInstantiate)
            {
                var ltNew = lt;
                ltNew.Position = lt.Position + right * space;
                InstantiateBullet_L( ltNew, damage, speed, entityBulletInstantiate);
                ltNew.Position = lt.Position + right * -space;
                InstantiateBullet_L( ltNew, damage, speed, entityBulletInstantiate);
            }
            
            void InstantiateBullet_L(LocalTransform lt, float damage, float speed, Entity entityBullet)
            {
                Entity entity;
                if (bufferBulletDisables.Length > 0)
                {
                    entity = bufferBulletDisables[0].entity;
                    ecb.RemoveComponent<Disabled>(entity);
                    ecb.AddComponent(entity, new SetActiveSP { state = DisableID.Enable });
                    bufferBulletDisables.RemoveAt(0);
                }
                else
                {
                    entity = ecb.Instantiate(entityBullet);
                }
                ecb.AddComponent(entity, lt);
                ecb.AddComponent(entity, new BulletInfo { damage = damage, speed = speed, startTime = time });
            }
            
        }

        

        [BurstCompile]
        private struct SpawnBulletJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            [ReadOnly] public NativeArray<BufferBulletSpawner> bulletSpawnerArr;
            [ReadOnly] public NativeArray<BufferBulletDisable> bulletDisableArr;
            [ReadOnly] public Entity entityBullet;
            [ReadOnly] public float time;

            public void Execute(int index)
            {
                var bulletSpawn = bulletSpawnerArr[index];
                float subtractIndex = 0.5f;
                int halfNumberPreShot = (int)math.ceil(bulletSpawn.bulletPerShot / 2f);
                var lt = bulletSpawn.lt;
                var angleRota = MathExt.QuaternionToFloat3(lt.Rotation);
                float damage = bulletSpawn.damage;
                float speed = bulletSpawn.speed;

                if (halfNumberPreShot % 2 != 0)
                {
                    Instantiate(index, lt, damage, speed);
                    --halfNumberPreShot;
                    subtractIndex = 0;
                }

                for (int i = 1; i <= halfNumberPreShot; i++)
                {
                    float3 angleRotaNew = angleRota;
                    float angle = (i - subtractIndex) * bulletSpawn.spaceAnglePerBullet;
                    float angle1 = angleRotaNew.y + angle;
                    float angle2 = angleRotaNew.y - angle;
                    angleRotaNew.y = angle1;
                    lt.Rotation = MathExt.Float3ToQuaternion(angleRotaNew);
                    Instantiate(index, lt, damage, speed);
                    angleRotaNew.y = angle2;
                    lt.Rotation = MathExt.Float3ToQuaternion(angleRotaNew);
                    Instantiate(index, lt, damage, speed);
                }
            }

            private void Instantiate(int index, LocalTransform lt, float damage, float speed)
            {
                Entity entity;
                if (bulletDisableArr.Length > index)
                {
                    entity = bulletDisableArr[index].entity;
                    ecb.AddComponent(index, entity, new SetActiveSP { state = DisableID.Enable });
                    ecb.RemoveComponent<Disabled>(index, entity);
                }
                else
                {
                    entity = ecb.Instantiate(index, entityBullet);
                }

                ecb.AddComponent(index, entity, lt);
                ecb.AddComponent(index, entity, new BulletInfo { damage = damage, speed = speed, startTime = time });
            }
        }
    }
}
