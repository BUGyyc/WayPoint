using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMove : MonoBehaviour
{
    CharacterController characterController;

    List<Vertex> targetList;

    List<Vector3> moveList;

    float Gravity = 9.8f;

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

        float deltaY = 0;
        if (characterController.isGrounded == false)
        {
            deltaY = -1 * Time.deltaTime * Gravity;
        }
        else
        {
            deltaY = 0;
        }


        if (moveList == null || moveList.Count == 0) return;

        Vector3 curTarget = moveList[0];
        float dis = Vector3.Distance(curTarget, this.transform.position);
        if (dis < 1.5f)
        {
            moveList.RemoveAt(0);
        }
        else
        {
            transform.LookAt(new Vector3(curTarget.x, transform.position.y, curTarget.z));
            Vector3 moveDir = transform.forward * UnityEngine.Time.deltaTime * 10;
            //Y轴计算
            moveDir += new Vector3(0, deltaY, 0);
            characterController.Move(moveDir);
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
