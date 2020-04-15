using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 验电笔
/// 实现用电器接口测试裸露部分
/// 继承用电器
/// 使用方法：建立电笔父节点，添加该脚本，引入电笔触点，通过GetHasProw()查询是否带电
/// </summary>
public class Electroscope : MonoBehaviour, IElectricComponent
{
    public float HealthPoint { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public float power = 0;//电笔功率

    /// <summary>
    /// 验电笔触点
    /// 因为验电笔只有一个触点所以跟电线不同有两头
    /// </summary>
    [Header("电笔触点"), SerializeField]
    public LeadJoint eTestJoint;
    bool hasAmperage = false;//是否有电流

    /// <summary>
    /// 更新触点电流信息
    /// </summary>
    void UpdataAmperageData()
    {
        hasAmperage =  eTestJoint.GetJointElectrocircuit().Amperage > 0 ? true : false;
    }

    /// <summary>
    /// 电笔是否带电
    /// </summary>
    /// <returns></returns>
    public bool GetHasProw()
    {
        return hasAmperage;
    }

    /// <summary>
    /// 深度值
    /// 因为验电笔可以理解为电阻无穷大
    /// </summary>
    /// <returns></returns>
    public uint GetDepthValue()
    {
        return 99999;
    }

    public IJoint[] GetNextDepthJoint(IJoint joint)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 获取功率
    /// </summary>
    /// <returns></returns>
    public float GetPowerLoad()
    {
        return power;
    }

    public void Reset()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //更新触点电流信息
        UpdataAmperageData();
    }
}
