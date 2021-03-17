using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadWayMap : MonoBehaviour
{

    private List<Vertex> points;
    // Start is called before the first frame update
    void Start()
    {
        LevelConfigTool levelCfg = LevelConfigTool.GetConfigByName("level_01");

        Debug.Log("---------------- " + levelCfg.config.PointsCount);

        WayPointManager.instance = new WayPointManager(levelCfg.config);

        // points = new List<Vertex>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        if (WayPointManager.instance == null) return;

        if (points == null)
        {
            points = new List<Vertex>();
            WayPointManager.instance.GetVertexList(ref points);
        }


        foreach (var point in points)
        {
            Gizmos.color = VertexGraph.PointColors[point.Data.mapId];
            Gizmos.DrawSphere(point.Data.position, 0.55f);

            foreach (var item in point.neighbours)
            {
                Gizmos.color = VertexGraph.PointColors[item.Data.mapId];

                Gizmos.DrawLine(point.Data.position, item.Data.position);
            }
        }
    }
}
