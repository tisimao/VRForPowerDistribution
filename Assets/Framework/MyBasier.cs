using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 我的贝塞尔曲线
/// </summary>
public class MyBezier
{
    float power = 1f;
    /// <summary>
    /// 曲线弯曲程度
    /// </summary>
    public float Power
    {
        set
        {
            power = Mathf.Max(1f, value);
        }
        get
        {
            return power;
        }
    }

    public float AllLength { get => allLength; }

    /// <summary>
    /// 所有的点
    /// </summary>
    List<Transform> allPoints = new List<Transform>();
    /// <summary>
    /// 所有点的权重
    /// </summary>
    List<float> pointLengths = new List<float>();
    /// <summary>
    /// 总长度
    /// </summary>
    float allLength = 0;
    public MyBezier(Transform[] points )
    {
        if (points == null)
        {
            throw (new NullReferenceException());
        }
        allPoints.AddRange(points);
        //从起点开始计算权重
        CalculateLength(0);
    }

    public MyBezier(List<Transform> points)
    {
        if (points == null)
        {
            throw (new NullReferenceException());
        }
        allPoints.AddRange(points);
        //从起点开始计算权重
        CalculateLength(0);
    }

    public void AddPoint(Transform point)
    {
        if(point == null)
        {
            throw (new NullReferenceException());
        }
        allPoints.Add(point);
        if (allPoints.Count - 2 > 0)
        {
            //从前一个开始重新计算权重
            CalculateLength(allPoints.Count - 2);
        }
        else
        {
            //从起点开始计算权重
            CalculateLength(0);
        }
    }

    public void AddPointAt(int index, Transform point)
    {
        if (point == null)
        {
            throw (new NullReferenceException());
        }
        allPoints.Insert(index,point);
        //从起点开始计算权重
        CalculateLength(index);
    }

    public Transform GetPointAt(int index)
    {
        if(allPoints.Count > index)
        {
            return allPoints[index];
        }
        else
        {
            return null;
        }
    }

    public int GetCount()
    {
        return allPoints.Count;
    }

    public void RemoveAt(int index)
    {
        allPoints.RemoveAt(index);
        CalculateLength(index - 1);
    }
    /// <summary>
    /// 递归计算每个点对应的里程到最后
    /// </summary>
    /// <param name="index"></param>
    void CalculateLength(int index)
    {
        float currentLength;
        if(index > 0)
        {
            currentLength = Vector3.Distance(allPoints[index - 1].position, allPoints[index].position);
            allLength = pointLengths[index - 1];
        }
        else
        {
            //第一个点是起点
            currentLength = 0;
            allLength = 0;
        }
        //累积权重
        allLength += currentLength;
        if (pointLengths.Count - 1 < index)
        {
            pointLengths.Add(allLength);
        }
        else
        {
            pointLengths[index] = allLength;
        }
        //遍历到最后清除多余数据
        if(allPoints.Count - 1 == index)
        {
            if (allPoints.Count < pointLengths.Count)
            {
                pointLengths.RemoveRange(allPoints.Count, pointLengths.Count - allPoints.Count);
            }
        }
        //递归到最后一个
        if(index +1 < allPoints.Count)
        {
            CalculateLength(index + 1);
        }
    }


    /// <summary>
    /// 计算一个贝塞尔曲线上点
    /// </summary>
    /// <param name="from">起点</param>
    /// <param name="to">终点</param>
    /// <param name="fromDirection">起点方向</param>
    /// <param name="toDirection">终点方向</param>
    /// <param name="t">0~1</param>
    /// <returns></returns>
    public static Vector3 Basier(Vector3 from,Vector3 to, Vector3 fromDirection, Vector3 toDirection, float t,float power)
    {
        Vector3 p0 = from;
        Vector3 p1 = from + fromDirection * power;
        Vector3 p2 = to - toDirection * power;
        Vector3 p3 = to;
        //贝塞尔三次公式
        return p0 * Mathf.Pow(1f - t, 3f) + 3f * p1 * t * Mathf.Pow(1f - t, 2f) + 3f * p2 * t * t * (1 - t) + p3 * Mathf.Pow(t, 3f);
    }

    /// <summary>
    /// 求某一点的切线方向
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="fromDirection"></param>
    /// <param name="toDirection"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector3 BezierTangent(Vector3 from, Vector3 to, Vector3 fromDirection, Vector3 toDirection, float t, float power)
    {

        Vector3 p0 = from;
        Vector3 p1 = from + fromDirection * power;
        Vector3 p2 = to - toDirection * power;
        Vector3 p3 = to;

        float u = 1 - t;
        float uu = u * u;
        float tu = t * u;
        float tt = t * t;

        Vector3 P = p0 * 3 * uu * (-1.0f);
        P += p1 * 3 * (uu - 2 * tu);
        P += p2 * 3 * (2 * tu - tt);
        P += p3 * 3 * tt;

        //返回单位向量
        return P.normalized;
    }

    bool GetFromTo(Vector3 pos,out Transform from,out Transform to,out float time,out float allTime)
    {
        bool haveValue = false;
        float high = float.MaxValue;
        from = null; to = null; time = 0; allTime = 1f;
        for (int i = 0; i < allPoints.Count - 1; i++)
        {
            Vector3 f = allPoints[i].position;
            Vector3 t = allPoints[i + 1].position;
            Vector3 pf = pos - f;
            Vector3 pt = pos - t;
            Vector3 dis = t - f;
            Vector3 p = Vector3.Project(pf,dis);
            float h = (pf - p).magnitude;
            if(h < high && Vector3.Dot(pf, dis )>0 && Vector3.Dot(pt, -dis) > 0)
            {
                haveValue = true;
                from = allPoints[i];
                to = allPoints[i + 1];
                time = p.magnitude / dis.magnitude;
                allTime = Mathf.Lerp(pointLengths[i], pointLengths[i + 1], time)/allLength;
            }
        }
        return haveValue;
    }



    public Vector3 GetPosition(float t)
    {
        if (allPoints.Count == 0)
        {
            return Vector3.zero;
        }
        int index = 0;
        if (t > 0)
        {
            if (t < 1f)
            {
                float currentLength = t * allLength;
                float tempT = 0;
                //找到对应的位置
                for (int i = 0; i < pointLengths.Count; i++)
                {
                    if (currentLength < pointLengths[i])
                    {
                        index = i;
                        tempT = (currentLength - pointLengths[i - 1]) / (pointLengths[i] - pointLengths[i - 1]);
                        break;
                    }
                }


                return Basier(allPoints[index - 1].position, allPoints[index].position, allPoints[index - 1].forward, allPoints[index].forward, tempT, Power);
            }
            else
            {
                return allPoints[allPoints.Count - 1].position;
            }
        }
        return allPoints[0].position;
    }
    public Vector3 GetPosition(Vector3 pos, out float allTime)
    {
        Transform from, to;
        float time;
        if (GetFromTo(pos, out from, out to, out time , out allTime))
        {
            return Basier(from.position, to.position, from.forward, to.forward, time,Power);
        }
        else
        {
            return allPoints[allPoints.Count -1].position;
        }
    }

    public Vector3 GetDirection(float t)
    {
        if (allPoints.Count == 0)
        {
            return Vector3.forward;
        }
        int index = 0;
        if (t > 0)
        {
            if (t < 1f)
            {
                float currentLength = t * allLength;
                float tempT = 0;
                //找到对应的位置
                for (int i = 0; i < pointLengths.Count; i++)
                {
                    if (currentLength < pointLengths[i])
                    {
                        index = i;
                        tempT = (currentLength - pointLengths[i - 1]) / (pointLengths[i] - pointLengths[i - 1]);
                        break;
                    }
                }
                return BezierTangent(allPoints[index - 1].position, allPoints[index].position, allPoints[index - 1].forward, allPoints[index].forward, tempT, Power);
            }
            else
            {
                return allPoints[allPoints.Count - 1].forward;
            }
        }
        return allPoints[0].forward;
    }
    public Vector3 GetDirection(Vector3 pos, out float allTime)
    {
        Transform from, to;
        float time;
        if (GetFromTo(pos, out from, out to, out time, out allTime))
        {
            return BezierTangent(from.position, to.position, from.forward, to.forward, time, Power);
        }
        else
        {
            return allPoints[allPoints.Count - 1].forward;
        }
    }

}