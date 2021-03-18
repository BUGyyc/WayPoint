using System.Collections.Generic;
using System.IO;
using Protocol;
using UnityEngine;
using System;
using GameEditor;

namespace GameEditor
{
    [ExecuteInEditMode]
    public class LevelConfig : MonoBehaviour
    {
        public GameObject wayPointRoot;
        public GameObject wayLineRoot;
        private void Start()
        {
            if (!wayPointRoot)
            {
                wayPointRoot = new GameObject("wayPointRoot");
                wayPointRoot.transform.SetParent(this.transform);
            }

            if (!wayLineRoot)
            {
                wayLineRoot = new GameObject("wayLineRoot");
                wayLineRoot.transform.SetParent(this.transform);
            }
        }

        //导出
        public void Export(string path, string name)
        {
            WayMapCfg wayMapCfg = new WayMapCfg();
            AddWayMapToCfg(wayMapCfg);
            var bytes = wayMapCfg.ToByteArray();
            if (Toolkit.SaveFile(bytes, path + name + ".bytes"))
            {
                UnityEngine.Debug.Log("保存完毕:" + path);
            }
        }

        private void AddWayMapToCfg(WayMapCfg wayMapCfg)
        {
            Debug.Log("路点数据 导入cfg ");
            WayPointItem[] points = wayPointRoot.GetComponentsInChildren<WayPointItem>();
            WayLineItem[] lines = wayLineRoot.GetComponentsInChildren<WayLineItem>();
            foreach (var item in points)
            {
                WayPointCfg wayPointCfg = new WayPointCfg();
                wayPointCfg.Id = item.ID;
                wayPointCfg.Position = item.transform.position.ToFloat3();
                wayPointCfg.MapId = item.mapId;
                wayMapCfg.AddPoints(wayPointCfg);
            }
            Debug.Log("开始导出线  ");
            foreach (var item in lines)
            {
                WayPointItem headPoint = item.head.transform.gameObject.GetComponent<WayPointItem>();
                WayPointCfg head = GetWayPointCfg(headPoint.ID, wayMapCfg);
                WayPointItem lastPoint = item.last.transform.gameObject.GetComponent<WayPointItem>();
                WayPointCfg last = GetWayPointCfg(lastPoint.ID, wayMapCfg);
                WayLineCfg wayLineCfg = new WayLineCfg();
                if (head == null || last == null)
                {
                    Debug.LogErrorFormat("error export line {0}  {1} ", head, last);
                }
                else
                {
                    wayLineCfg.Head = head;
                    wayLineCfg.Last = last;
                    wayLineCfg.Cost = item.Value;
                    wayLineCfg.IsOneWay = item.IsOneWay;
                }
                wayMapCfg.AddLines(wayLineCfg);
            }
            Debug.Log("导出路点导航数据  ");
        }

        private WayPointCfg GetWayPointCfg(uint id, WayMapCfg wayMapCfg)
        {
            foreach (var item in wayMapCfg.Points)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
