/*
 * @Author: delevin.ying 
 * @Date: 2020-06-23 15:27:44 
 * @Last Modified by: delevin.ying
 * @Last Modified time: 2021-03-17 15:02:14
 */

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace GameEditor
{
    [ExecuteInEditMode]
    public class WayPointItem : MonoBehaviour
    {
        public uint mapId;
        public uint id;
        public List<GameObject> lines;//记录所有的线
        private List<Transform> neights;
        public uint ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }


        public uint lineCount
        {
            get
            {
                return (uint)lines.Count;
            }
        }

        private GUIStyle style = null;


        private void Start()
        {
            style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
        }

        private void OnEnable()
        {
            UpdateNeights();
            UpdatePointId();
        }

        private void UpdatePointId()
        {
            ID = (uint)this.transform.GetSiblingIndex() + 1;
        }

        public bool AddLine(GameObject line)
        {
            if (lines == null)
            {
                lines = new List<GameObject>();
            }
            foreach (var item in lines)
            {
                //检查是否已经包含
                bool result = WayLineItem.CheckLine(item, line);
                if (result == true)
                {
                    return false;
                }
            }
            lines.Add(line);
            UpdateNeights();
            return true;
        }

        public void SetId(uint id)
        {

        }

        private int stepCount = 0;
        public void StartHighLight()
        {
            stepCount = 20;
            StartCoroutine("HighLightEffect");
        }

        IEnumerator HighLightEffect()
        {
            while (stepCount > 0)
            {
                stepCount--;
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }

        public void DeleteNode(GameObject line)
        {
            if (lines == null)
            {
                lines = new List<GameObject>();
            }
            foreach (var item in lines)
            {
                if (item.GetInstanceID() == line.GetInstanceID())
                {
                    lines.Remove(item);
                    break;
                }
                else
                {

                }
            }
            UpdateNeights();
        }

        private void UpdateNeights()
        {
            neights = new List<Transform>();
            if (lines == null)
            {
                lines = new List<GameObject>();
            }
            foreach (var item in lines)
            {
                if (item == null)
                {
                    Debug.LogError("item === null  this.transform " + this.transform.position);
                }
                WayLineItem wayLineItem = item.GetComponent<WayLineItem>();
                Transform transform = wayLineItem.GetOtherTransform(this.transform);
                neights.Add(transform);
            }
        }

        private void OnDrawGizmos()
        {
            DrawWayPoint();
        }

        private void DrawWayPoint()
        {
            if (mapId == 0) return;
            int colorIndex = (int)mapId - 1;
            colorIndex = colorIndex % 10;//(0-9);
            colorIndex = Mathf.Min(colorIndex, VertexGraph.PointColors.Length - 1);
            colorIndex = Mathf.Max(0, colorIndex);
            Gizmos.color = VertexGraph.PointColors[colorIndex];

            Gizmos.DrawSphere(this.transform.position, 0.55f);

            Gizmos.color = Color.red;
            if (stepCount <= 0)
            {
                Gizmos.DrawWireSphere(this.transform.position, 0.01f);
            }
            else if (stepCount % 2 == 1)
            {
                Gizmos.DrawWireSphere(this.transform.position, 1.8f);
            }
            else if (stepCount % 2 == 0)
            {
                Gizmos.DrawWireSphere(this.transform.position, 1f);
            }

            Gizmos.color = VertexGraph.PointColors[colorIndex];
            if (neights == null || neights.Count < 1)
            {
                //
            }
            else
            {
                foreach (var item in neights)
                {
                    if (item != null && item.gameObject.GetComponent<WayPointItem>().mapId != mapId)
                    {
                        //不相等图，说明是不同类型图的临界边
                        Gizmos.color = new Color32(250, 75, 75, 200);
                    }
                    if (item == null)
                    {
                        Debug.LogError("路点故障   item === null   " + this.transform.position);
                    }
                    if (this.transform == null)
                    {
                        Debug.LogError("this.transform == null ");
                    }
                    Gizmos.DrawLine(this.transform.position, item.position);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var item in lines)
            {
                if (item != null)
                {
                    WayLineItem line = item.GetComponent<WayLineItem>();
                    line.Remove(this.gameObject);
                }
            }
        }
    }
}
