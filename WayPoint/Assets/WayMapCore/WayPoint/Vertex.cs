/*
 * @Author: delevin.ying 
 * @Date: 2020-07-15 19:21:23 
 * @Last Modified by: delevin.ying
 * @Last Modified time: 2021-03-17 12:25:02
 */

using System.Collections.Generic;

public class Vertex
{
    public VertexObj Data;
    public bool isVisited;
    public Vertex PreVertex;
    public Vertex NextVertex;
    public float gCost;
    public float hCost;
    public float fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    public List<Vertex> neighbours;
    public Vertex()
    {
        neighbours = new List<Vertex>();
    }
}
