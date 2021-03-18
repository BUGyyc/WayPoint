using Protocol;
using UnityEngine;
using System.Collections.Generic;
using Google.ProtocolBuffers.Collections;

public static class VectorUtil
{
    public static Float3 ToFloat3(this Vector3 src)
    {
        return new Float3()
        {
            X = src.x,
            Y = src.y,
            Z = src.z
        };
    }

    public static Vector3 ToVector3(this Float3 src)
    {
        return new Vector3(src.X, src.Y, src.Z);
    }
}
