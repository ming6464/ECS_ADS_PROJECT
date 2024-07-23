using System;
using System.Globalization;
using Unity.Entities;
using UnityEngine;

public class ItemAuthoring : MonoBehaviour
{
    public ItemType type;
    public TypeUsing typeUsing;
    public int id;
    [Header("Setup")]
    [Space(10)]
    public int count;
    public Operation operation;
    public Transform textPositon;
    [Header("Item Obstacle")]
    public Transform[] spawnPoints;
    [Header("------")] 
    public ItemInfoSetup[] weapons;
    public Animator animator;
    private int _passIdWeapon;
    private ItemType _passItemType;
    private void OnValidate()
    {
        if(Application.isPlaying) return; 
        if ( typeUsing == TypeUsing.canShooting && _passIdWeapon != id || _passItemType != type)
        {
            if (id >= 0)
            {
                foreach (var weapon in weapons)
                {
                    if (weapon.id == id && weapon.itemType == type)
                    {
                        weapon.weapon.SetActive(true);
                        animator.avatar = weapon.avatar;
                        
                    }
                    else
                    {
                        weapon.weapon.SetActive(false);
                    }
                    
                }
                _passIdWeapon = id;
                _passItemType = type;
            }
        }
    }

    private class ItemAuthoringBaker : Baker<ItemAuthoring>
    {
        public override void Bake(ItemAuthoring authoring)
        {
            return;
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity,new ItemInfo()
            {
                id = authoring.id,
                type = authoring.type,
                count = authoring.count,
                hp = authoring.count,
                operation = authoring.operation,
                idTextHp = entity.Index
            });
            
            if (authoring.typeUsing.Equals(TypeUsing.canShooting))
            {
                AddComponent<ItemCanShoot>(entity);
                AddComponent(entity,new TextMeshData()
                {
                    id = entity.Index,
                    text = authoring.count.ToString(),
                    position = authoring.textPositon.position
                });
            }else if (authoring.type.Equals(ItemType.Character))
            {
                string str = "";
                switch (authoring.operation)
                {
                    case Operation.Addition:
                        str = "+";
                        break;
                    case Operation.Subtraction:
                        str = "-";
                        break;
                    case Operation.Multiplication:
                        str = "x";
                        break;
                    case Operation.Division:
                        str = ":";
                        break;
                }
                AddComponent(entity,new TextMeshData()
                {
                    id = entity.Index,
                    text = str +authoring.count,
                    position = authoring.textPositon.position
                });
            }
            
            if (authoring.spawnPoints.Length > 0)
            {
                var buffer = AddBuffer<BufferSpawnPoint>(entity);

                foreach (var pointTf in authoring.spawnPoints)
                {
                    buffer.Add(new BufferSpawnPoint()
                    {
                        value = pointTf.position,
                    });
                }
            }
        }
    }
}

public enum TypeUsing
{
    none,
    canShooting
}

public enum Operation
{
    Addition, Subtraction, Multiplication, Division
}

[Serializable]
public struct ItemInfoSetup
{
    public GameObject weapon;
    public Avatar avatar;
    public int id;
    public ItemType itemType;
}