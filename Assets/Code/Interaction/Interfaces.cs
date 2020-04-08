using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 能够被手碰触的物品
/// </summary>
public interface ITouchable
{
    /// <summary>
    /// 当被手碰触时
    /// </summary>
    /// <param name="hand"></param>
    void OnTouch(HandScript hand);
}




public enum GrabType
{
    Big,
    Small,
    BigForward,
    SmallForward
}

/// <summary>
/// 能够拿起的物品接口
/// </summary>
public interface IGrabable
{
    GrabType GrabType { get; }

    bool IsGrabable { get; }

    bool IsGrabing { get; }
    
    /// <summary>
    /// 拿起
    /// </summary>
    /// <param name="hand"></param>
    bool Hold(Collider handOn, HandScript hand);

    /// <summary>
    /// 扔下
    /// </summary>
    /// <param name="hand"></param>
    void Drop(HandScript hand);
}

/// <summary>
/// 工具
/// 电笔 电起子 
/// </summary>
public interface ITooles : IGrabable
{
    /// <summary>
    /// 开关
    /// </summary>
    bool SwitchOn { get; set; }

}

/// <summary>
/// 固定接口 两个固定点互相固定
/// </summary>
public interface IFixation
{
    string Name { get; }

    /// <summary>
    /// 相对另一个固定物是否能是否能移动
    /// </summary>
    bool Unmovable(IFixation other);

    /// <summary>
    /// 一般情况下 相同的组能够互相固定
    /// </summary>
    /// <returns></returns>
    int GetFixationGroup();
    /// <summary>
    /// 获取位置 手部会根据粗细给予不同的节点 指尖 或 手心
    /// </summary>
    /// <returns></returns>
    Transform GetTransform(IFixation other);

    /// <summary>
    /// 获取位置的相对偏移
    /// </summary>
    /// <returns></returns>
    Vector3 GetTransformOffset(IFixation other);

    /// <summary>
    /// 获取位置的相对旋转
    /// </summary>
    /// <returns></returns>
    Quaternion GetTransformRotate(IFixation other);

    /// <summary>
    /// 固定
    /// </summary>
    /// <param name="fixationable"></param>
    bool AddFixation(IFixation other,Transform node);
    void RemoveFixation(IFixation other);
}


