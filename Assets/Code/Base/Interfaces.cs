using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 电气元件接口
/// </summary>
public interface IElectricComponent
{
    //生命值,电器烧毁需要过热并烧毁过程，该值也可以认为是耐热程度
    float HealthPoint { get; set; }
    
  //  /// <summary>
  //  /// 设置输出 是否还需要这个方法，通过电路类已经能够设定
  //  /// </summary>
  //  /// <param name="id">接口ID</param>
    
  ////  Electrocircuit GetOutput(IJoint joint);

    /// <summary>
    /// 返回和这个接头连通的所有接头
    /// </summary>
    /// <param name="joint"></param>
    /// <returns></returns>
    IJoint[] GetNextDepthJoint(IJoint joint);

    /// <summary>
    /// 获取电路深度参数 
    /// 电阻为0的 如电线为1
    /// 有电阻的 99999 以上
    /// </summary>
    /// <returns></returns>
    uint GetDepthValue();
    /// <summary>
    /// 获取负载功率
    /// </summary>
    /// <returns></returns>
    float GetPowerLoad();
    /// <summary>
    /// 重置
    /// </summary>
    void Reset();
}

/// <summary>
/// 连接点
/// </summary>

public interface IJoint
{
    string Name { get; }

    /// <summary>
    /// 是否短路
    /// </summary>
    bool Short { set; get; }

    void AddJoint(IJoint joint);

    void RemoveJoint(IJoint joint);

    IJoint[] GetLinks();

    /// <summary>
    /// 获取电器接口
    /// </summary>
    /// <returns></returns>
    IElectricComponent GetElectricComponent();

    /// <summary>
    /// 接头电路信息
    /// </summary>
    /// <returns></returns>
    Electrocircuit GetJointElectrocircuit();
    /// <summary>
    /// 添加电路信息
    /// </summary>
    /// <param name="electricCurrent"></param>
    void AddElectrocircuit(Electrocircuit electricCurrent);

    /// <summary>
    /// 移除电路信息
    /// </summary>
    /// <param name="electricCurrent"></param>
    void RemoveElectrocircuit(Electrocircuit electricCurrent);

    /// <summary>
    /// 返回和这个接头连通的所有接头
    /// </summary>
    /// <returns></returns>
    IJoint[] GetNextDepthJoint();
    /// <summary>
    /// 重置
    /// </summary>
    void Reset();
    /// <summary>
    /// 刷新
    /// </summary>
    void Refresh();

}

/// <summary>
/// 锚点 一组连接点只能有一个锚点
/// 螺丝钉就是一个典型的锚点
/// </summary>
public interface IAnchorJoint : IJoint
{
    /// <summary>
    /// 锁定后的锚点不能接入新的连接
    /// </summary>
    bool Locked { get; set; }

    bool Fixed { get; }
}

