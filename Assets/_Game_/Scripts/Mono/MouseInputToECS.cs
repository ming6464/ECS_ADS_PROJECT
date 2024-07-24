using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MouseInputToECS : MonoBehaviour
{
    public bool rotaWithMouse;
    //
    private EntityManager _entityManager;
    private Entity _entity;
    private PlayerInput _data;
    private Transform _cameraTf;
    private Camera _camera;
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
            pullTrigger = false
        };
        _entityManager.SetComponentData(_entity, _data);
        _camera = Camera.main;
        _cameraTf = _camera.transform;
    }

    void LateUpdate()
    {
        var mousePositionScreen = Input.mousePosition;
        mousePositionScreen.z = 20;
        var mousePositionWorld = Camera.main.ScreenToWorldPoint(mousePositionScreen);
        _data.directMove = new float2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _data.pullTrigger = Input.GetMouseButton(0);
        _data.directMouse = _camera.ScreenPointToRay(Input.mousePosition).direction;
        _data.mousePosition = mousePositionWorld;
        _entityManager.SetComponentData(_entity, _data);
        RotaWithCam(mousePositionWorld);
    }

    private void RotaWithCam(Vector3 mousePositionWorld)
    {
        return;
        if(!rotaWithMouse) return;
        var parentCam = _cameraTf.parent;
        var positionSet = parentCam.position + Vector3.up;
        var directRota = Vector3.Normalize(mousePositionWorld- positionSet);
        _cameraTf.rotation = Quaternion.LookRotation(directRota);
    }
}