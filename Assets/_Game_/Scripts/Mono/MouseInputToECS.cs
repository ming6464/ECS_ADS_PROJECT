using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MouseInputToECS : MonoBehaviour
{
    public bool input3D;
    public float3 speed;
    //
    private EntityManager _entityManager;
    private Entity _entity;
    private PlayerInput _data;
    private Vector3 _halfScreen;
    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        // Tạo một archetype cho thực thể của bạn
        EntityArchetype archetype = _entityManager.CreateArchetype(
            typeof(PlayerInput)
        );

        // Tạo thực thể
        _entity = _entityManager.CreateEntity(archetype);

        // Gán giá trị cho thành phần dữ liệu
        PlayerInput _data = new PlayerInput
        {
            directMove = float2.zero,
            mouseWorldPos = float3.zero,
            mouseScreenPos = float3.zero,
            pullTrigger = false
        };
        _entityManager.SetComponentData(_entity, _data);
        _halfScreen = new Vector3(Screen.width, Screen.height,0f) / 2f;
    }

    void Update()
    {
        var mouseScreenPosition = Input.mousePosition;
        var mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        var subtrackPosMouse = (float3)mouseScreenPosition - _data.mouseScreenPos;
        subtrackPosMouse *= speed;
        (subtrackPosMouse.y, subtrackPosMouse.x) = (subtrackPosMouse.x, subtrackPosMouse.y);
        var angleRota = new float3(Input.GetAxis("Mouse Y"),Input.GetAxis("Mouse X"),0) * speed + _data.angleRota;
        angleRota.x = math.clamp(angleRota.x, -90f, 90f);
        angleRota.z = 0;
        if (!input3D)
        {
            angleRota.x = 0;
        }
        _data.directMove = new float2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _data.mouseScreenPos = mouseScreenPosition;
        _data.angleRota = angleRota;
        _data.mouseWorldPos = mouseWorld;
        _data.pullTrigger = Input.GetMouseButton(0);
        _entityManager.SetComponentData(_entity, _data);
    }
}