using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 验电笔
/// 实现用电器接口测试裸露部分电
/// </summary>
public class Electroscope : MonoBehaviour, IElectricComponent
{
    public float HealthPoint { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public uint GetDepthValue()
    {
        throw new System.NotImplementedException();
    }

    public IJoint[] GetNextDepthJoint(IJoint joint)
    {
        throw new System.NotImplementedException();
    }

    public float GetPowerLoad()
    {
        throw new System.NotImplementedException();
    }

    public void Reset()
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
