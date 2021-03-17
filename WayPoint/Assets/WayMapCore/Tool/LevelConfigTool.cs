
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;


public class LevelConfigTool
{
    private static Dictionary<string, LevelConfigTool> _cacheCfgDic = new Dictionary<string, LevelConfigTool>();

    public static LevelConfigTool GetConfigByName(string name)
    {
        if (_cacheCfgDic.ContainsKey(name) && _cacheCfgDic[name] != null)
        {
            return _cacheCfgDic[name];
        }

        string path = string.Format("{0}/{1}/{2}.bytes", UnityEngine.Application.streamingAssetsPath, "Levels", name);

        return new LevelConfigTool(path);
    }

    public WayMapCfg config;

    private LevelConfigTool(string path)
    {
        var bytes = Toolkit.LoadFile(path);

        config = WayMapCfg.ParseFrom(bytes);
    }    
}
