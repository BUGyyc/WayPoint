using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LayerMaskUtil
{
    public static readonly int LAYER_WALL = LayerMask.NameToLayer("Wall");
    public static readonly int LAYER_WALL_MASK = LayerMask.GetMask("Wall");
}


