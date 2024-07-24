﻿using Unity.Entities;
using Unity.Mathematics;


public struct LayerStoreComponent : IComponentData
{
    public uint defaultLayer;
    public uint playerLayer;
    public uint characterLayer;
    public uint enemyLayer;
    public uint enemyDieLayer;
    public uint bulletLayer;
    public uint itemLayer;
    public uint itemCanShootLayer;
}

public struct EffectComponent : IComponentData
{
    public EffectID effectID;
    public float3 position;
    public quaternion rotation;
}

public struct SetActiveSP : IComponentData
{
    public DisableID state;
}


public struct SetAnimationSP : IComponentData
{
    public StateID state;
    public float timeDelay;
}

//Enum

public enum EffectID
{
    HitFlash = 0,
    GroundCrack = 1,
    MetalImpact = 2,
}

public enum DisableID
{
    Disable,
    Enable,
    DisableAll,
    EnableAll,
    Destroy,
    DestroyAll,
}


public enum StateID
{
    None,
    Die,
    WaitToPool,
    Enable,
    Run,
    Attack,
    Idle,
    WaitRemove
}

public enum CameraType
{
    FirstPersonCamera,
    ThirstPersonCamera,
}

//Enum

//Events {

//Events }

//other components

public struct AddToBuffer : IComponentData
{
    public int id;
    public Entity entity;
}

public struct TakeDamage : IComponentData
{
    public float value;
}

public struct NotUnique : IComponentData
{
}

public struct New : IComponentData
{
    
}

public struct CanWeapon : IComponentData
{
    
}

public struct DataProperty : IComponentData
{
    
}



//