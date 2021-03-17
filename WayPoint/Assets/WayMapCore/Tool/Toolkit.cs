using Google.ProtocolBuffers.Collections;
using Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


    /// <summary>
    /// 1.isInEye：性能瓶颈
    /// </summary>
    public static class Toolkit
    {
        public delegate void NoParamCallBack();
        public delegate void ParamCallBack<T>(T t);
        public delegate void ParamCallBack<T1, T2>(T1 t1, T2 t2);

        public static T Find<T>(this PopsicleList<T> list, System.Predicate<T> match)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (match.Invoke(list[i])) return list[i]; 
            }

            return default;
        }

        public static int FindIndex<T>(this PopsicleList<T> list, System.Predicate<T> match)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (match.Invoke(list[i])) return i;
            }

            return -1;
        }

        public static List<T> FindAll<T>(this PopsicleList<T> list, System.Predicate<T> match)
        {
            List<T> result = new List<T>();

            for (int i = 0; i < list.Count; i++)
            {
                if (match.Invoke(list[i]))
                {
                    result.Add(list[i]);
                }
            }

            return result;
        }

        public static void CopyList<T>(this IList<T> list, List<T> targetList, ParamCallBack<T> addCallBack, ParamCallBack<int> removeCallBack)
        {
            //排除多余的
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];

                var index = targetList.FindIndex(x => x.Equals(item));

                if (index < 0)
                {
                    removeCallBack(i);
                    //list.removeCallBack(i);
                    i--;
                }
            }

            for (int i = 0; i < targetList.Count; i++)
            {
                var item = targetList[i];

                bool existInList = false;

                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].Equals(item))
                    {
                        existInList = true;
                        break;
                    }
                }

                if (!existInList)
                {
                    addCallBack(item);
                }
            }
        }

        public static byte[] LoadFile(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                BinaryReader reader = new BinaryReader(fs);

                int numBytesToRead = (int)fs.Length;

                byte[] bytes = reader.ReadBytes(numBytesToRead);

                reader.Close();
                fs.Close();

                return bytes;

            }
            catch (FileNotFoundException ioEx)
            {
                UnityEngine.Debug.LogError(ioEx.Message);
            }

            return null;
        }

        public static bool SaveFile(byte[] bytes, string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);

            try
            {
                BinaryWriter bw = new BinaryWriter(fs);

                bw.Write(bytes);

                fs.Close();
                fs.Dispose();
            }
            catch (IOException e)
            {

                UnityEngine.Debug.LogError(e.Message);

                return false;
            }

            return true;
        }

        //是否处在视野之中
        public static bool isInEye(Matrix4x4 vp, Vector3 position, Vector3 cameraPos, float bias = 2.2f)
        {
            Vector4 homogeneous = position;                 //计算投影之后的齐次坐标
            homogeneous.w = 1;
            homogeneous = vp * homogeneous;
            homogeneous.x = homogeneous.x / homogeneous.w;
            homogeneous.y = homogeneous.y / homogeneous.w;
            homogeneous.z = homogeneous.z / homogeneous.w - 1f;

            if (Mathf.Abs(homogeneous.x) < 1.05f && Mathf.Abs(homogeneous.y) < 1.05f && homogeneous.z > 0)
            {
                Vector3 dir = position - cameraPos;

                float distance = dir.magnitude;

                dir.Normalize();

                Ray ray = new Ray(position, -dir);

                RaycastHit hitInfo;

                if (!Physics.Raycast(ray, out hitInfo, distance, LayerMaskUtil.LAYER_WALL_MASK) || (cameraPos - hitInfo.point).sqrMagnitude < bias * bias)
                {
                    return true;
                }
            }

            return false;
        }

        public static float RangeAngleTo360(float rawAngle)
        {
            if (rawAngle < 0) rawAngle = 360 + rawAngle;
            else if (rawAngle > 360) rawAngle -= 360;
            return rawAngle;
        }

        /// <summary>
        /// 获取匹配的角度值
        /// </summary>
        /// <returns></returns>
        public static int GetMatchDegree(float source)
        {
            var index = source / 45;
            var result = (int)index * 45;
            var mid = Mathf.Abs(source % 45) > 22.5f ? 45 : 0;

            result += source > 0 ? mid : -mid;
            return result;
        }

        public static float GetDistanceOfPlane(Vector3 pointOfPlane, Vector3 normal, Vector3 point)
        {
            return Vector3.Dot(point - pointOfPlane, normal);
        }

        // public static BoxColliderData GetCollider(BoxCollider collider)
        // {
        //     var box = new BoxColliderData();

        //     box.Center = collider.center.ToFloat3();
        //     box.Size = collider.size.ToFloat3();
        //     box.IsTrigger = collider.isTrigger;

        //     return box;
        // }

        // public static void InitCollider(BoxCollider collider, BoxColliderData colliderData)
        // {
        //     collider.center = colliderData.Center.ToVector3();
        //     collider.size = colliderData.Size.ToVector3();
        //     collider.isTrigger = colliderData.IsTrigger;
        // }
    }

