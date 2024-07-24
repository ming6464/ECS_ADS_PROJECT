using System;
using System.Globalization;
using Unity.Entities;
using UnityEngine;

public class ItemAuthoring : MonoBehaviour
{
    [Tooltip("Xác định loại item của đối tượng.")]
    public ItemType type;

    [Tooltip("Xác định cách sử dụng của item.")]
    public TypeUsing typeUsing;

    [Tooltip("ID duy nhất của item.")]
    public int id;

    [Header("Setup")]
    [Space(10)]
    [Tooltip("Nếu item là loại bắn đạn để sử dụng thì số này sẽ là số hp của nó\nNếu item là loại khi sử dụng sẽ tăng số lượng nhân vật thì số này sẽ là số để tính toán")]
    public int count;

    [Tooltip("Xác định loại phép tính đến item.")]
    public Operation operation;

    [Tooltip("Vị trí của text liên quan đến item.")]
    public Transform textPositon;

    [Tooltip("Xác định liệu item có xoay theo hướng người chơi hay không.")]
    public bool followPlayer;

    [Header("Item Obstacle")]
    [Tooltip("Mảng các điểm spawn cho item.\nví dụ: item pháo, khi đây là mảng các vị trí sẽ spawn ra chúng")]
    public Transform[] spawnPoints;

    [Header("-----Phần này bỏ qua-----")]
    [Tooltip("Mảng các thiết lập thông tin item cho vũ khí.")]
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
            var textObj = authoring.textPositon;
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity,new ItemInfo()
            {
                id = authoring.id,
                type = authoring.type,
                count = authoring.count,
                hp = authoring.count,
                operation = authoring.operation,
                idTextHp = entity.Index,
            });
            
            AddComponent<HitCheckOverride>(entity);
            
            if (authoring.typeUsing.Equals(TypeUsing.canShooting))
            {
                AddComponent<ItemCanShoot>(entity);
                AddComponent(entity,new TextMeshData()
                {
                    id = entity.Index,
                    text = authoring.count.ToString(),
                    position = textObj.parent.position,
                    offset =  textObj.position - textObj.parent.position,
                    textFollowPlayer = authoring.followPlayer
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
                    position = textObj.parent.position,
                    offset =  textObj.position - textObj.parent.position,
                    textFollowPlayer = authoring.followPlayer
                    
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