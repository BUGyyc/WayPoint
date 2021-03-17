/*
 * @Author: delevin.ying 
 * @Date: 2020-07-15 19:25:25 
 * @Last Modified by: delevin.ying
 * @Last Modified time: 2021-03-17 16:21:39
https://www.itread01.com/content/1508047085.html
 */


using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;


public class VertexGraph
{

    public static readonly int V_BASE1 = 1;

    public static readonly int V_BASE2 = 1000;

    public static readonly int V_BASE3 = 1000000;

    //所有的顶点
    private List<Vertex> vertices;
    //所有的边 包含权重值
    private Dictionary<int, float> edges;
    //存储已知的最短路径
    private Dictionary<int, List<Vertex>> paths = null;

    public static readonly Color[] PointColors = new Color[]{
            Color.yellow,
            Color.blue,
            Color.cyan,
            Color.gray,
            Color.green,
            Color.red,
            Color.black,
            Color.white,
            Color.cyan,
            Color.magenta
        };

    public VertexGraph()
    {
        vertices = new List<Vertex>(512);
        edges = new Dictionary<int, float>(512);
        paths = new Dictionary<int, List<Vertex>>(512);
    }

    public void AddVertex(Vertex vertex)
    {
        vertices.Add(vertex);
    }

    public List<Vertex> GetVertices()
    {
        return vertices;
    }

    public Vertex GetVertexById(uint id)
    {
        foreach (var item in vertices)
        {
            if (item.Data.id == id)
            {
                return item;
            }
        }
        return null;
    }

    public Vertex GetVertexByTransform(Transform tf)
    {
        foreach (var item in vertices)
        {
            if (item.Data.position == tf.position)
            {
                return item;
            }
        }
        return null;
    }

    public void AddEdge(Vertex start, Vertex end, float cost, bool isOneWay, uint type = 0)
    {
        //有向图
        int key1 = GetKey(start, end, type);
        int key2 = GetKey(end, start, type);
        if (HasEdge(key1))// || HasEdge(key2)
        {
            Debug.LogError("图中已经包含边");
        }
        else
        {
            //TODO:暂时用距离当作权重
            //edges[key1] = Vector3.Distance(v1.Data.position, v2.Data.position);//(v1.Data.position - v2.Data.position).magnitude;
            edges[key1] = cost;
            if (isOneWay)
            {
                edges[key2] = 1000 + cost;
            }
            else
            {
                edges[key2] = cost;
            }

        }
    }

    public void DeleteEdge(Vertex v1, Vertex v2, uint type = 0)
    {
        int key1 = GetKey(v1, v2, type);
        int key2 = GetKey(v2, v1, type);
        if (HasEdge(key1))
        {
            edges[key1] = float.MaxValue;
        }
        else if (HasEdge(key2))
        {
            edges[key2] = float.MaxValue;
        }
        else
        {
            Debug.Log("图中不包含边  " + key1 + "  or " + key2);
        }
    }

    public bool HasEdge(Vertex v1, Vertex v2, uint type)
    {
        int key1 = GetKey(v1, v2, type);
        int key2 = GetKey(v2, v1, type);
        return HasEdge(key1) || HasEdge(key2);
    }

    public bool HasEdge(int key)
    {
        return edges.ContainsKey(key);
    }

    /// <summary>
    /// 两点的权重值
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public float GetVertexLineValue(Vertex origin, Vertex target, uint type = 0)
    {
        int key1 = GetKey(origin, target, type);
        if (edges.ContainsKey(key1))
        {
            return edges[key1];
        }

        int key2 = GetKey(target, origin, type);
        if (edges.ContainsKey(key2))
        {
            return edges[key2];
        }
        else
        {
            return float.MaxValue;
        }
    }

    public float GetMoveCost(Vertex origin, Vertex target, uint type = 0)
    {
        int key1 = GetKey(origin, target, type);
        if (edges.ContainsKey(key1))
        {
            return edges[key1];
        }
        return float.MaxValue;
    }

    private int GetKey(Vertex v1, Vertex v2, uint type)
    {
        return (int)(v1.Data.id * V_BASE1 + v2.Data.id * V_BASE2 + type * V_BASE3);
    }

    public uint KeyToValue(int key, int pos)
    {
        switch (pos)
        {
            case 1:
                key = key % V_BASE2;
                return (uint)(key / V_BASE1);
            case 2:
                key = key % V_BASE3;
                return (uint)(key / V_BASE2);
            case 3:
                return (uint)(key / V_BASE3);
            default:
                break;
        }
        return 0;
    }

    /// <summary>
    /// 设置两点的权重值
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="value"></param>
    public void SetVertexsValue(Vertex origin, Vertex target, float value, uint type = 0)
    {
        int key1 = GetKey(origin, target, type);
        int key2 = GetKey(target, origin, type);
        if (edges.ContainsKey(key1))
        {
            edges[key1] = value;
        }
        else if (edges.ContainsKey(key2))
        {
            edges[key2] = value;
        }
        else
        {
            edges[key1] = value;
        }
    }

    public int FindPath(ref List<Vertex> path, Vertex origin, Vertex target, uint type = 0)
    {
        int key1 = GetKey(origin, target, type);
        int key2 = GetKey(target, origin, type);
        if (paths.ContainsKey(key1))
        {
            foreach (var item in paths[key1])
            {
                path.Add(item);
            }
            return 1;
        }
        else if (paths.ContainsKey(key2))
        {
            //需要深度拷贝 ， 翻转一下
            foreach (var item in paths[key2])
            {
                path.Add(item);
            }
            path.Reverse();
            return 2;
        }
        else
        {
            FindPathActual(ref path, origin, target, type);
            return 0;
        }
    }
    public void FindPathActual(ref List<Vertex> foundPath, Vertex start, Vertex target, uint type)
    {
        List<Vertex> openSet = new List<Vertex>();
        HashSet<Vertex> closedSet = new HashSet<Vertex>();
        openSet.Add(start);
        while (openSet.Count > 0)
        {
            Vertex currentNode = openSet[0];
            for (int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost &&
                    openSet[i].hCost < currentNode.hCost))
                {
                    if (currentNode.Data.id != openSet[i].Data.id)
                    {
                        currentNode = openSet[i];
                    }
                }
            }
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            if (currentNode.Data.id == target.Data.id)
            {
                foundPath = RetracePath(start, currentNode);
                break;
            }

            foreach (Vertex neighbor in GetNeighbours(currentNode))
            {
                if (!closedSet.Contains(neighbor))
                {
                    float newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbor, type);
                    if (newMovementCostToNeighbour < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbour;
                        neighbor.hCost = GetDistance(neighbor, target, type);
                        neighbor.PreVertex = currentNode;
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
        }
    }

    private List<Vertex> RetracePath(Vertex startNode, Vertex endNode)
    {
        List<Vertex> path = new List<Vertex>();
        Vertex currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.PreVertex;
        }
        path.Reverse();
        return path;
    }

    private List<Vertex> GetNeighbours(Vertex curr)
    {
        if (curr == null)
        {
            return null;
        }
        return curr.neighbours;
    }

    private float GetDistance(Vertex va, Vertex vb, uint type)
    {
        int key1 = GetKey(va, vb, type);
        if (edges.ContainsKey(key1))
        {
            return edges[key1];
        }

        Vector3 posA = va.Data.position;
        Vector3 posB = vb.Data.position;
        int distX = (int)Mathf.Abs(posA.x - posB.x);
        int distZ = (int)Mathf.Abs(posA.z - posB.z);
        int distY = (int)Mathf.Abs(posA.y - posB.y);
        if (distX > distZ)
        {
            return 14 * distZ + 10 * (distX - distZ) + 10 * distY;
        }
        return 14 * distX + 10 * (distZ - distX) + 10 * distY;
    }
}
