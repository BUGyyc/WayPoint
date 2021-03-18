using Google.ProtocolBuffers.Collections;
using Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// 1.isInEye：性能瓶颈
/// </summary>
public static class Toolkit
{
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
}

