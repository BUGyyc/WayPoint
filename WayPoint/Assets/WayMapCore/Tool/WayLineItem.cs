/*
 * @Author: delevin.ying 
 * @Date: 2021-03-17 12:20:32 
 * @Last Modified by:   delevin.ying 
 * @Last Modified time: 2021-03-17 12:20:32 
 */

using UnityEngine;

namespace GameEditor
{
    [ExecuteInEditMode]
    public class WayLineItem : MonoBehaviour
    {
        public Transform head;
        public Transform last;
        public bool isSelf = true;
        [Header("start所属Map ID")]
        public uint headMapId = 0;
        [Header("end所属Map ID")]
        public uint lastMapId = 0;
        [Header("权重值")]
        public float Value = 0;
        [Header("是否自动设置权重值")]
        public bool isAuto = true;
        public uint type = 0;

        [Header("是否是单方向路线")]
        public bool IsOneWay = true;

        public Transform GetOtherTransform(Transform node)
        {
            if (node == null)
            {
                Debug.LogError("GetOtherTransform 出错 ");
                return null;
            }
            else if (node.position == head.position)
            {
                return last;
            }
            else if (node.position == last.position)
            {
                return head;
            }
            else
            {
                Debug.LogError("GetOtherTransform 出错  未找到 ");
                return null;
            }
        }
        int step = 0;
        private void Update()
        {
            step++;
            if (step > 5)
            {
                step = 0;
                if (head == null || last == null)
                {
                    //
                    Value = float.MaxValue;
                }
                else
                {
                    Vector3 pos = (head.position + last.position) / 2;
                    this.gameObject.transform.position = pos;

                    if (isAuto)
                    {
                        if (head.gameObject.GetComponent<WayPointItem>().mapId == last.gameObject.GetComponent<WayPointItem>().mapId)
                        {
                            Value = (head.position - last.position).magnitude;
                        }
                        else
                        {
                            Value = 1000f;//TODO：暂定比较大的代价
                        }

                    }
                }
            }


        }

        private void OnDestroy()
        {
            if (isSelf)
            {
                RemoveBySelf();
            }
        }

        public void Remove(GameObject source)
        {
            isSelf = false;
            //通知其他Node
            if (head.gameObject.GetInstanceID() == source.GetInstanceID())
            {
                DeleteLine(last);
            }
            else if (last.gameObject.GetInstanceID() == source.GetInstanceID())
            {
                DeleteLine(head);
            }
            else
            {

            }
            DestroyImmediate(this.gameObject);
        }

        private void OnDrawGizmos()
        {
            if (IsOneWay) {
                var dir = last.position - head.position;
                var center1 = transform.position;
                var tmp = dir.normalized;
                var tmp1 = Quaternion.AngleAxis(150, Vector3.up) * tmp;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(center1, center1 + tmp1);
                var tmp2 = Quaternion.AngleAxis(-150, Vector3.up) * tmp;
                Gizmos.DrawLine(center1, center1 + tmp2);
            }
        }

        private void BeDeleteLine(Transform transform)
        {
            Debug.LogError("广播移除 -----》  " + transform);
            if (transform == null) return;
            GameObject obj = transform.gameObject;
            if (obj)
            {
                WayPointItem point = obj.GetComponent<WayPointItem>();
                if (point)
                {
                    point.DeleteNode(this.gameObject);
                }
            }
        }

        public void RemoveBySelf()
        {
            DeleteLine(head);
            DeleteLine(last);
        }

        private void DeleteLine(Transform transform)
        {
            // Debug.LogError("广播移除 -----》  " + transform);
            if (transform == null) return;
            GameObject obj = transform.gameObject;
            if (obj)
            {
                WayPointItem point = obj.GetComponent<WayPointItem>();
                if (point)
                {
                    point.DeleteNode(this.gameObject);
                }
            }
        }

        /// <summary>
        /// 判断是否是同一条线
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns></returns>
        public static bool CheckLine(GameObject l1, GameObject l2)
        {
            if (l1 == null || l2 == null)
            {
                Debug.LogError("l1l2  null");
                return false;
            }
            else
            {

                WayLineItem item1 = l1.GetComponent<WayLineItem>();
                WayLineItem item2 = l2.GetComponent<WayLineItem>();
                if (item1 == null || item2 == null)
                {
                    Debug.LogError("item1 item2 = null");
                    return false;
                }
                else
                {
                    int headId1 = item1.head.gameObject.GetInstanceID();
                    int headId2 = item2.head.gameObject.GetInstanceID();
                    int lastId1 = item1.last.gameObject.GetInstanceID();
                    int lastId2 = item2.last.gameObject.GetInstanceID();

                    if (headId1 == headId2 && lastId1 == lastId2)
                    {
                        // Debug.LogError("item1 item2 线重复");
                        return true;
                    }
                    else if (headId1 == lastId2 && headId2 == lastId1)
                    {
                        // Debug.LogError("item1 item2 线重复");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
