using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    [Tooltip("Vị trí spawn của người chơi.")]
    public Transform spawnPosition;

    [Tooltip("Prefab của nhân vật.")]
    public GameObject characterPrefab;

    [Tooltip("Điểm HP của người chơi.")]
    public float hp;

    [Tooltip("Tốc độ di chuyển của người chơi.")]
    public float speed;

    [Tooltip("Bán kính của mỗi nhân vật để khác định va chạm")]
    public float radius;

    [Tooltip("Loại mục tiêu.")]
    public AimType aimType;

    [Tooltip("Tốc độ di chuyển của các nhân vật đến vị trí của hàng ngũ của mình")]
    public float speedMoveToNextPoint;

    [Tooltip("Xác định có tự động nhắm vào kẻ thù gần nhất hay không.")]
    public bool aimNearestEnemy;

    [Tooltip("Xác định liệu có xoay 3D hay không.")]
    public bool rota3D;

    [Tooltip("Khoảng cách để nhắm mục tiêu khi bật chế độ tự động nhắm.")]
    public float distanceAim;

    [Tooltip("Tốc độ xoay tối đa.")]
    public float moveToWardMax;

    [Tooltip("Tốc độ xoay tối thiểu.")]
    public float moveToWardMin;

    [Tooltip("Số lượng nhân vật khi bắt đầu.")]
    public int numberSpawnDefault;

    [Tooltip("Khoảng cách giữa các nhân vật.")]
    public float2 spaceGrid;

    [Tooltip("ID vũ khí của người chơi khi bắt đầu game.")]
    public int idWeaponDefault;
    [Header("MODE")]
    public bool autoMove;
    public NextDestinationInfo[] nextDestinationInfos;

    class AuthoringBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new PlayerProperty
            {
                characterEntity = GetEntity(authoring.characterPrefab, TransformUsageFlags.Dynamic),
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
                autoMoveOnVehicle = authoring.autoMove,
                rota3D = authoring.rota3D,
            });

            if (authoring.autoMove)
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
        if(!autoMove) return;
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

