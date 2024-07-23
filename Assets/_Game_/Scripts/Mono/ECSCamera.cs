using Unity.Entities;
using UnityEngine;

public class ECSCamera : MonoBehaviour
{
    public Camera mainCamera;
    public Transform parentCamera;
    public float speedChangeCamera;
    
    public CameraType defaultType;
    //
    public Vector3 positionFirstPersonCamera;
    public Quaternion rotationFirstPersonCamera;
    public LayerMask layerCullingForFirstCam;
    [Space(3)]
    public Vector3 positionThirstPersonCamera;
    public Quaternion rotationThirstPersonCamera;
    public LayerMask layerCullingForThirstCam;
    //
    
    private Transform _mainCameraTf;
    private float _progressChangeCamera;
    private CameraType _curCameraType;
    

    private Vector3 _nextPosition;
    private Quaternion _nextRotation;

    private bool _addEvent;
    
    private void Awake()
    {
        _mainCameraTf = mainCamera.GetComponent<Transform>();
        _curCameraType = defaultType;
    }

    private void Start()
    {
        _progressChangeCamera = 1;
        SetUpCamera();
        UpdatePositionCam();
    }
    
    private void Update()
    {

        if (!_addEvent)
        {
            UpdateHybrid updateHybrid = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<UpdateHybrid>();
            if(updateHybrid == null) return;
            updateHybrid.DataPlayer += UpdateParentCamera;
            _addEvent = true;
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            switch (_curCameraType)
            {
                case CameraType.FirstPersonCamera:
                    _curCameraType = CameraType.ThirstPersonCamera;
                    break;
                case CameraType.ThirstPersonCamera:
                    _curCameraType = CameraType.FirstPersonCamera;
                    break;
            }
            _progressChangeCamera = 0;
            SetUpCamera();
        }

        if (_progressChangeCamera < 1)
        {
            UpdatePositionCam();
        }
    }

    private void UpdateParentCamera(Vector3 position, Quaternion rotation)
    {
        parentCamera.position = position;
        parentCamera.rotation = rotation;
    }

    private void SetUpCamera()
    {
        int layer;
        if (_curCameraType.Equals(CameraType.FirstPersonCamera))
        {
            layer = layerCullingForFirstCam;
        }else
        {
            layer = layerCullingForThirstCam;
        }

        mainCamera.cullingMask = layer;
    }

    private void UpdatePositionCam()
    {
        _progressChangeCamera = Mathf.Clamp(_progressChangeCamera + speedChangeCamera * Time.deltaTime,0,1);
        switch (_curCameraType)
        {
            case CameraType.ThirstPersonCamera:
                _nextPosition = Vector3.Lerp(positionFirstPersonCamera, positionThirstPersonCamera,
                    _progressChangeCamera);
                _nextRotation = Quaternion.Lerp(rotationFirstPersonCamera,rotationThirstPersonCamera,_progressChangeCamera);
                break;
            case CameraType.FirstPersonCamera:
                _nextPosition = Vector3.Lerp(positionThirstPersonCamera, positionFirstPersonCamera,
                    _progressChangeCamera);
                _nextRotation = Quaternion.Lerp(rotationThirstPersonCamera,rotationFirstPersonCamera,_progressChangeCamera);
                break;
        }
        _mainCameraTf.localPosition = _nextPosition;
        _mainCameraTf.localRotation = _nextRotation;
    }

    #region Func ContextMenu

    [ContextMenu("LoadDataFirstCamera")]
    private void LoadDataFirstCamera()
    {
        var tf = mainCamera.transform;
        rotationFirstPersonCamera = tf.localRotation;
        positionFirstPersonCamera = tf.localPosition;
    }
    
    [ContextMenu("LoadDataThirstCamera")]
    private void LoadDataThirstCamera()
    {
        var tf = mainCamera.transform;
        rotationThirstPersonCamera = tf.localRotation;
        positionThirstPersonCamera = tf.localPosition;
    }

    #endregion
    
}