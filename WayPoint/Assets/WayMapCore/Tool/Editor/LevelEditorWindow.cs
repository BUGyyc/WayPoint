/*
 * @Author: delevin.ying 
 * @Date: 2021-03-17 11:53:13 
 * @Last Modified by: delevin.ying
 * @Last Modified time: 2021-03-17 14:55:15
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
            EditorApplication.playModeStateChanged += OnStateChange;
            if(mCurrentCfg == null)
            {
                GameObject obj = GameObject.Find("LevelCfg");
                mCurrentCfg = obj.GetComponent<LevelConfig>();
            }
        }

        private void OnStateChange(PlayModeStateChange obj)
        {

                
            
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (Event.current.button == 1 && Event.current.type == EventType.MouseDown)
            {
                mouseDownPosition = Event.current.mousePosition;
                mouseDownTime = DateTime.Now.Ticks;
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
                wpi.type = type;
                Selection.activeObject = go;
                if (wayPointType == 1)
                {
                    wpi.minStandTime = 5;
                    wpi.maxStandTime = 20;
                }
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mConfigScene);
            }
            else
            {
                Debug.LogError("CreateWayPoint 传入类型错误:" + dataArray);
            }
        }
    }

