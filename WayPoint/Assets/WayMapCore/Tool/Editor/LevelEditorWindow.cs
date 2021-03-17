/*
 * @Author: delevin.ying 
 * @Date: 2021-03-17 11:53:13 
 * @Last Modified by: delevin.ying
 * @Last Modified time: 2021-03-17 15:09:42
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using Protocol;
using GameEditor;

public class LevelEditorWindow : EditorWindow
{
        const string TARGET_ITEM_PATH = "Assets/Prefab/CfgPrefab.prefab";
        private GameObject _prefab;
        public GameObject prefab
        {
            get
            {
                if (!_prefab)
                {
                    _prefab = AssetDatabase.LoadAssetAtPath(TARGET_ITEM_PATH, typeof(GameObject)) as GameObject;
                }
                return _prefab;
            }
        }
        
        // private Scene mConfigScene;
        private Vector3 mouseDownPosition;
        private long mouseDownTime;
        private string mLevelName = "";

        private LevelConfig mCurrentCfg;

        private static Scene mConfigScene;

        [MenuItem("路点配置/编辑")]
        public static void OpenLevelToolWindow()
        {
            LevelEditorWindow lvcWin = EditorWindow.GetWindow<LevelEditorWindow>("关卡配置");
            lvcWin.Show();
            mConfigScene = EditorSceneManager.OpenScene(Global.SCENE_PATH);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            if(mCurrentCfg == null)
            {
                GameObject obj = GameObject.Find("LevelCfg");
                mCurrentCfg = obj.GetComponent<LevelConfig>();
            }
        }


        private bool isDrawLine = false;
        private bool startDrawLine = false;
        private Transform startTransform = null;
        private Transform endTransform = null;

        private void OnSceneGUI(SceneView sceneView)
        {
            if (Event.current.button == 1 && Event.current.type == EventType.MouseDown)
            {
                mouseDownPosition = Event.current.mousePosition;
                mouseDownTime = DateTime.Now.Ticks;
            }

            if (Event.current.capsLock)
            {
                if (!isDrawLine)
                {
                    isDrawLine = OnShiftDown(sceneView);
                }
            }
            else
            {
                if (isDrawLine)
                {
                    isDrawLine = false;
                    bool result = OnShiftUp(sceneView);
                    if (result)
                    {
                        CreateLine();
                        Selection.activeObject = endTransform.gameObject;
                    }
                    else
                    {
                        // Debug.LogError("DrawLine ----> 失败  " + startTransform + "  " + endTransform);
                    }
                }
            }

            if (Event.current.button == 1 && Event.current.type == EventType.MouseUp && DateTime.Now.Ticks - mouseDownTime < 2000000)
            {
                Vector3 mousePosition = Event.current.mousePosition;
                //无视拖拽事件
                if ((mouseDownPosition - mousePosition).sqrMagnitude > 25)
                {
                    return;
                }

                mousePosition.y = sceneView.position.height - mousePosition.y;

                Vector3 targetPosition = sceneView.camera.transform.position + sceneView.camera.transform.forward * 30;
                Ray ray = sceneView.camera.ScreenPointToRay(mousePosition);

                Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.red, 1);

                RaycastHit hitInfo;

                //命中了才弹出
                if (Physics.Raycast(ray, out hitInfo, 1000, LayerMaskUtil.LAYER_WALL_MASK))
                {
                    Debug.Log("success");
                    targetPosition = hitInfo.point;
                    targetPosition.y += 0.02f;

                    GenericMenu menu = new GenericMenu();      
                    menu.AddItem(new GUIContent("创建路点"), false, CreateWayPoint, new object[] { targetPosition, prefab, mCurrentCfg.wayPointRoot, mConfigScene, 0 });
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }
        }

        private int mTap = 0;
        static string[] taps = { "列表", "当前" };
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(3);
            mTap = GUILayout.Toolbar(mTap, taps);
            mTap = 0;
            switch (mTap)
            {
                case 0:
                    DrawLevelList();
                    break;
                case 1:
                    DrawLevelEditor();
                    break;
            }
            GUILayout.EndVertical();
        }

        private void DrawLevelList()
        {
            GUILayout.Space(3);

            if (GUILayout.Button("保存至本地"))
            {
                Save2Cfg(mLevelName);
            }

            GUILayout.Space(10);
        }

        [MenuItem("Tools/StartMain", true, 1)]
        static bool ValidStartMainScene()
        {
            return !Application.isPlaying;
        }
        private void DrawLevelItem(int index, string name)
        {

        }

        private Vector2 mListPosition;


        private void Save2Cfg(string levelName)
        {
            if (CheckCanEditor() == false)
            {
                Debug.LogError("scene is not load");
                return;
            }
            
            
        }

        private bool OnShiftDown(SceneView sceneView)
        {
            GameObject obj = Selection.activeObject as GameObject;
            if (obj == null) return false;
            if (obj.name.Equals("CfgPrefab(Clone)") == false)
            {
                return false;
            }
            startTransform = obj.transform;
            startDrawLine = true;
            return true;
        }


        private bool OnShiftUp(SceneView sceneView)
        {
            GameObject obj = Selection.activeObject as GameObject;
            if (obj == null) return false;
            if (obj.name.Equals("CfgPrefab(Clone)") == false)
            {
                return false;
            }
            endTransform = obj.transform;
            startDrawLine = false;
            return true;
        }

        private void CreateLine()
        {
            OnCreateLineByTf(startTransform, endTransform);
        }

        private Transform OnCreateLineByTf(Transform startTransform, Transform endTransform)
        {
            WayPointItem startPoint = startTransform.gameObject.GetComponent<WayPointItem>();
            WayPointItem endPoint = endTransform.gameObject.GetComponent<WayPointItem>();
            if (startPoint == null || endPoint == null)
            {
                Debug.LogError("生成失败 ---- startPoint endPoint ");
                return null;
            }
            else
            {
                uint headMapId = startPoint.mapId;
                uint lastMapId = endPoint.mapId;

                // uint type = startPoint.type;

                // uint startType = startPoint.type;
                // uint endType = endPoint.type;
                // if (startType != endType)
                // {
                //     Debug.LogErrorFormat("Error startType != endType  {0}  {1} ", startType, endType);
                //     return null;
                // }

                if (headMapId == 0 && lastMapId == 0)
                {
                    Debug.LogError("生成失败 ---- 两路点都未分配mapId ");
                    return null;
                }
                else if (headMapId != lastMapId && headMapId * lastMapId != 0)
                {
                    //二者都不为0，且不相等
                }
                else
                {
                    //一个为0，另一个不为0
                    if (headMapId == 0)
                    {
                        headMapId = lastMapId;
                    }
                    else
                    {
                        lastMapId = headMapId;
                    }
                }

                GameObject go = GameObject.Instantiate(prefab);
                go.name = "WayLineItem";
                go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                WayLineItem wayLineItem = go.AddComponent<WayLineItem>();
                wayLineItem.head = startTransform;
                wayLineItem.last = endTransform;
                wayLineItem.headMapId = headMapId;
                wayLineItem.lastMapId = lastMapId;
                // wayLineItem.type = type;
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                renderer.material = null;
                go.transform.SetParent(mCurrentCfg.wayLineRoot.transform, false);
                Vector3 position = (startTransform.position + endTransform.position) / 2;
                go.transform.position = position;
                EditorSceneManager.MarkSceneDirty(mConfigScene);
                GameObject startObj = startTransform.gameObject;
                GameObject endObj = endTransform.gameObject;
                bool r1 = AddLineToObj(startObj, go);
                bool r2 = AddLineToObj(endObj, go);
                if (r1 == false || r2 == false)
                {
                    DestroyImmediate(go);
                }
                else
                {
                    startObj.GetComponent<WayPointItem>().mapId = headMapId;
                    endObj.GetComponent<WayPointItem>().mapId = lastMapId;
                    Selection.activeObject = endTransform.gameObject;
                }
                return go.transform;
            }
        }

        private bool AddLineToObj(GameObject obj, GameObject line)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                WayPointItem point = obj.GetComponent<WayPointItem>();
                if (point)
                {
                    return point.AddLine(line);
                }
                else
                {
                    return false;
                }
            }
        }

        private void DrawLevelEditor()
        {

        }

        private bool CheckCanEditor()
        {
            if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals("LevelEditor"))
            {
                return false;
            }
            return true;
        }

        private void CreateWayPoint(object dataArray)
        {
            if (dataArray is object[])
            {
                var array = (object[])dataArray;
                Vector3 position = (Vector3)array[0];
                GameObject prefab = (GameObject)array[1];
                GameObject root = (GameObject)array[2];
                Scene mConfigScene = (Scene)array[3];
                int wayPointType = (int)array[4];
                uint type = 0;//(uint)array[4];
                GameObject go = GameObject.Instantiate(prefab);
                WayPointItem wpi = go.AddComponent<WayPointItem>();
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                renderer.material = null;
                WayPointItem[] wayPointItems = root.GetComponentsInChildren<WayPointItem>();
                uint id = 1;
                if (wayPointItems.Length < 1)
                {
                    id = 1;
                }
                else
                {
                    id = wayPointItems[wayPointItems.Length - 1].ID + 1;
                }
                go.transform.SetParent(root.transform, false);
                go.transform.position = (Vector3)position;
                wpi.ID = id;
                // wpi.type = type;
                Selection.activeObject = go;
                // if (wayPointType == 1)
                // {
                //     wpi.minStandTime = 5;
                //     wpi.maxStandTime = 20;
                // }
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mConfigScene);
            }
            else
            {
                Debug.LogError("CreateWayPoint 传入类型错误:" + dataArray);
            }
        }
    }

