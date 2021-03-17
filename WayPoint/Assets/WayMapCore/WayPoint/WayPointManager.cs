/*
 * @Author: delevin.ying 
 * @Date: 2020-07-17 15:15:47 
 * @Last Modified by: delevin.ying
 * @Last Modified time: 2021-03-17 16:21:12
 */

using Protocol;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;


public class WayPointManager
{
    public static WayPointManager instance;
    private WayMapCfg mWayMapCfg;
    private VertexGraph vertexGraph;
    // private Dictionary<uint, VertexGraph> mThingVertexGraph;
    private List<List<Vertex>> localEdges = new List<List<Vertex>>();
    private Dictionary<uint, int> vertexNumberDic = new Dictionary<uint, int>();
    /// <summary>
    /// 根据类型细分的点集合
    /// </summary>
    private Dictionary<uint, List<Vertex>> vertexDic = new Dictionary<uint, List<Vertex>>();


    public WayPointManager(WayMapCfg cfg)
    {
        mWayMapCfg = cfg;
        InitVertexGraph();
    }

    /// <summary>
    /// 初始化导航路点数据
    /// </summary>
    private void InitVertexGraph()
    {
        vertexGraph = new VertexGraph();
        localEdges.Clear();
        //创建点数据
        foreach (var item in mWayMapCfg.Points)
        {
            Vertex vertex = new Vertex();
            VertexObj vertexObj = new VertexObj();
            vertexObj.id = item.Id;
            vertexObj.position = item.Position.ToVector3();
            vertexObj.mapId = item.MapId;
            if (item.HasForward && item.Forward.ToVector3() != Vector3.zero)
            {
                vertexObj.forward = item.Forward.ToVector3();
            }
            if (item.HasMinStandTime && item.MinStandTime > 0)
            {
                vertexObj.minStandTime = item.MinStandTime;
            }
            if (item.HasMaxStandTime && item.MaxStandTime > 0)
            {
                vertexObj.maxStandTime = item.MaxStandTime;
            }
            vertexObj.passLineTime = item.PassLineTime;
            vertexObj.lineCount = item.LineCount;
            vertexObj.needOffset = item.NeedOffset;
            vertexObj.offsetValue = item.OffsetValue;
            vertex.Data = vertexObj;
            vertexGraph.AddVertex(vertex);
        }
        //创建边
        foreach (var item in mWayMapCfg.Lines)
        {
            WayPointCfg headPoint = item.Head;
            WayPointCfg lastPoint = item.Last;
            Vertex head = vertexGraph.GetVertexById(headPoint.Id);
            Vertex last = vertexGraph.GetVertexById(lastPoint.Id);
            if (head == null || last == null) continue;
            List<Vertex> list = new List<Vertex>();
            list.Add(head);
            list.Add(last);
            localEdges.Add(list);
            vertexGraph.AddEdge(head, last, item.Cost, item.IsOneWay);

            head.neighbours.Add(last);
            last.neighbours.Add(head);
        }
    }

    private VertexGraph GetVertexGraph(uint type)
    {
        return vertexGraph;
    }

    public void GetVertexList(ref List<Vertex> list, uint typeId = 0)
    {
        VertexGraph vGraph = GetVertexGraph(typeId);
        list = vGraph.GetVertices();
    }

    public void GetLocalEdges(ref List<List<Vertex>> list)
    {
        list = localEdges;
    }

    // public Vertex GetVertexById(uint id, uint typeId = 0)
    // {
    //     VertexGraph vGraph = GetVertexGraph(typeId);
    //     if (vGraph == null)
    //     {
    //         return null;
    //     }
    //     else
    //     {
    //         return vGraph.GetVertexById(id);
    //     }
    // }

    public void GetEdgeVertex(ref List<Vertex> list, uint mapId, uint typeId = 0)
    {
        VertexGraph vGraph = GetVertexGraph(typeId);
        foreach (var item in vGraph.GetVertices())
        {
            if (item.Data.mapId == mapId)
            {
                if (item.Data.lineCount == 1)
                {
                    list.Add(item);
                }
            }
        }
    }

    public void FindPath(ref List<Vertex> path, Vector3 origin, Vector3 target, uint typeId, uint wayMapId, bool test = false)
    {
        Vertex start = FindNearVertex(origin, wayMapId, typeId);
        Vertex end = FindNearVertex(target, wayMapId, typeId);
        FindPath(ref path, start, end, Vector3.zero, 0, typeId, test);
    }

    public void FindPath(ref List<Vertex> path, Vertex origin, Vertex target, Vector3 start, uint entityId, uint typeId = 0, bool test = false)
    {
        VertexGraph vertexGraph = GetVertexGraph(typeId);

        if (vertexGraph == null)
        {
            Debug.LogError("FindPath vertexGraph == null error  ");
        }
        else
        {
            if (origin == null || target == null)
            {
                Debug.LogErrorFormat("搜索失败 {0} {1}", origin, target);
                return;
            }
            int type = vertexGraph.FindPath(ref path, origin, target);

            if (test)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in path)
                {
                    sb.Append(item.Data.id.ToString());
                    sb.Append("(");
                    sb.Append(item.Data.position.ToString());
                    sb.Append(")");
                    sb.Append(" -> ");
                }
                //float dis = Vector3.Distance(start, origin.Data.position);
                Debug.LogFormat("entityId:{0}  FindPath 查找路径 path: {1}", entityId, sb.ToString());
            }

        }
    }

    /// <summary>
    /// 从指定图，指定类型中取出一个离目标最近的路点
    /// </summary>
    /// <param name="position"></param>
    /// <param name="wayMapId"></param>
    /// <param name="typeId"></param>
    /// <returns></returns>
    public Vertex FindNearVertex(Vector3 position, uint wayMapId, uint typeId = 0)
    {
        VertexGraph vGraph = GetVertexGraph(typeId);
        int index = 0;
        float min = float.MaxValue;
        int minIndex = -1;
        List<Vertex> vertices = vGraph.GetVertices();

        List<Vertex> targetList = null;

        Dictionary<uint, List<Vertex>> dic = vertexDic;//(typeId == 0) ? vertexDic : thingVertexDic;

        if (dic.ContainsKey(wayMapId))
        {
            targetList = dic[wayMapId];
        }
        else
        {
            //缓存起来
            targetList = new List<Vertex>();
            foreach (var item in vertices)
            {
                if (item.Data.mapId == wayMapId || wayMapId == 0)
                {
                    targetList.Add(item);
                }
            }
            dic[wayMapId] = targetList;
        }

        foreach (var item in targetList)
        {
            float compare = Vector3.Distance(position, item.Data.position);//(position - item.Data.position).magnitude;
            if (compare < min)
            {
                min = compare;
                minIndex = index;
            }
            index++;
        }
        return (minIndex == -1) ? null : targetList[minIndex];
    }

    /// <summary>
    /// 根据类型和Id 随机取一个
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="typeId"></param>
    /// <returns></returns>
    public Vertex GetRandomVertex(uint mapId, bool getStandFirst, uint typeId = 0)
    {
        VertexGraph vGraph = GetVertexGraph(typeId);
        List<Vertex> vertices = vGraph.GetVertices();
        List<Vertex> targetList = new List<Vertex>();

        Dictionary<uint, List<Vertex>> dic = vertexDic;//(typeId == 0) ? vertexDic : thingVertexDic;
        if (dic.ContainsKey(mapId))
        {
            targetList = dic[mapId];
        }
        else
        {
            //缓存起来
            targetList = new List<Vertex>();
            foreach (var item in vertices)
            {
                if (item.Data.mapId == mapId || mapId == 0)
                {
                    targetList.Add(item);
                }
            }
            dic[mapId] = targetList;
        }

        if (targetList.Count < 1)
        {
            Debug.LogError("WayMap 搜索数量不够 " + mapId);
            return null;
        }
        else if (targetList.Count == 1)
        {
            return targetList[0];
        }
        else
        {
            if (getStandFirst)
            {
                int step = 0;
                int max = targetList.Count;
                Vertex target = targetList[UnityEngine.Random.Range(0, targetList.Count - 1)];
                //随机了很多次都没有，就别找了
                while (step < max && target.Data.maxStandTime <= 0)
                {
                    target = targetList[UnityEngine.Random.Range(0, targetList.Count - 1)];
                    step++;
                }
                return target;
            }
            else
            {
                return targetList[UnityEngine.Random.Range(0, targetList.Count - 1)];
            }
        }
    }


    /// <summary>
    /// 获取某种类型路点的数量
    /// </summary>
    /// <returns></returns>
    public int GetVertexNumberByTypeId(uint mapId, uint typeId = 0)
    {
        VertexGraph vGraph = GetVertexGraph(typeId);
        if (vertexNumberDic.ContainsKey(mapId))
        {
            return vertexNumberDic[mapId];
        }
        else
        {
            int number = 0;
            foreach (var item in vGraph.GetVertices())
            {
                if (item.Data.mapId == mapId)
                {
                    number++;
                }
            }
            vertexNumberDic.Add(mapId, number);
            return number;
        }
    }

    /// <summary>
    /// 得到按距离取到的路点列表，由近到远
    /// </summary>
    /// <param name="typeId"></param>
    /// <param name="position"></param>
    public void GetSortVertexListByTypeId(uint mapId, Vector3 position, ref List<VertexPro> resultList, uint typeId = 0)
    {
        VertexGraph vGraph = GetVertexGraph(typeId);
        resultList.Clear();
        foreach (var item in vGraph.GetVertices())
        {
            if (mapId == item.Data.mapId || mapId == 0)
            {
                resultList.Add(new VertexPro(item, Vector3.Distance(item.Data.position, position)));
            }
        }
        resultList.Sort((a, b) => a.distance.CompareTo(b.distance));
    }
}
