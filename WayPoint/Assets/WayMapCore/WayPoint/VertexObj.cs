
/*
 * @Author: delevin.ying 
 * @Date: 2020-07-15 19:22:13 
 * @Last Modified by: delevin.ying
 * @Last Modified time: 2021-03-17 12:25:44
 */
using UnityEngine;
using System.Collections.Generic;


public class VertexObj
{
    public uint id;
    public uint mapId;//所属mapId，区分类型
    public Vector3 forward;
    public float maxStandTime;
    public float minStandTime;
    public float passLineTime;
    public float value;
    public string flag;
    public Vector3 position;
    //边的数量
    public uint lineCount;

    public bool needOffset;
    public float offsetValue;
}

/// <summary>
/// 带比较距离的路点
/// </summary>
public struct VertexPro
{
    public Vertex vertex;
    public float distance;
    public VertexPro(Vertex v, float f)
    {
        vertex = v;
        distance = f;
    }
}
