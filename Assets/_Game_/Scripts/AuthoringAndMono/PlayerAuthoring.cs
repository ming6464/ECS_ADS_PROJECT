using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public Transform spawnPosition;
    public GameObject playerPrefab;
    public float hp;
    public float speed;
    public float radius;
    public AimType aimType;
    //
    public float speedMoveToNextPoint;
    //
    public bool aimNearestEnemy;
    public float distanceAim;
    public float moveToWardMax;
    public float moveToWardMin;
    public int numberSpawnDefault;
    public float2 spaceGrid;
    //
    public int idWeaponDefault;
    //
    [Header("MODE")] public bool autoMoveOnVehicle;
    public NextDestinationInfo[] nextDestinationInfos;

    class AuthoringBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new PlayerProperty
            {
                characterEntity = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic),
                speed = authoring.speed,
                spawnPosition = authoring.spawnPosition.position,
                spaceGrid = authoring.spaceGrid,
                numberSpawnDefault = authoring.numberSpawnDefault,
                aimType = authoring.aimType,
                characterRadius = authoring.radius,
                idWeaponDefault = authoring.idWeaponDefault,
                distanceAim = authoring.distanceAim,
                moveToWardMin = authoring.moveToWardMin,
                moveToWardMax = authoring.moveToWardMax,
                speedMoveToNextPoint = authoring.speedMoveToNextPoint,
                hp = authoring.hp,
                aimNearestEnemy = authoring.aimNearestEnemy,
                autoMoveOnVehicle = authoring.autoMoveOnVehicle,
            });

            if (authoring.autoMoveOnVehicle)
            {
                var bufferNextDestination = AddBuffer<bufferMoveDestination>(entity);

                foreach (var nextDestination in authoring.nextDestinationInfos)
                {
                    bufferNextDestination.Add(new bufferMoveDestination()
                    {
                        position = nextDestination.point.position,
                        speed = nextDestination.speed,
                    });
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(!autoMoveOnVehicle) return;
        Vector3 passPosition = default;
        for(int i = 0; i < nextDestinationInfos.Length; i++)
        {
            var nextDestination = nextDestinationInfos[i];
            if (i != 0)
            {
                Gizmos.DrawLine(passPosition,nextDestination.point.position);
            }
            passPosition = nextDestination.point.position;
            Gizmos.DrawSphere(passPosition,0.2f);
        }
    }

    [Serializable]
    public struct NextDestinationInfo
    {
        public Transform point;
        public float speed;
    }
}

