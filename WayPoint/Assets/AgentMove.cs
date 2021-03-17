using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMove : MonoBehaviour
{
    CharacterController characterController;

    List<Vertex> targetList;

    List<Vector3> moveList;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        targetList = new List<Vertex>();
        moveList = new List<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        InputListener();

        MoveLogic();
    }

    private void MoveLogic()
    {
        if (moveList == null || moveList.Count == 0) return;

        Vector3 curTarget = moveList[0];
        if (Vector3.Distance(curTarget, this.transform.position) < 0.2f)
        {
            moveList.RemoveAt(0);
        }
        else
        {
            characterController.Move(curTarget);
        }
    }

    private void InputListener()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            //命中了才弹出
            if (Physics.Raycast(ray, out hitInfo, 1000, LayerMaskUtil.LAYER_WALL_MASK))
            {
                Debug.LogFormat("click  {0}", hitInfo.point);

                Vertex startV = WayPointManager.instance.FindNearVertex(this.transform.position, 1);
                Vertex endV = WayPointManager.instance.FindNearVertex(hitInfo.point, 1);

                if (startV == null || endV == null)
                {
                    Debug.LogErrorFormat("取点失败");
                    return;
                }
                //规划新路径
                targetList.Clear();
                moveList.Clear();
                WayPointManager.instance.FindPath(ref targetList, startV, endV, this.transform.position);
                foreach (var item in targetList)
                {
                    moveList.Add(item.Data.position);
                }
            }
        }
    }
}
