// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class AnchorUtils
{
    static string anchorFile => Path.Combine(Application.persistentDataPath, "anchors.json");

    public static void Setup()
    {
        if (!File.Exists(anchorFile))
        {
            File.Create(anchorFile).Dispose();
            File.WriteAllText(anchorFile, "{\"anchors\" : [] }");
        }
        Debug.Log(anchorFile);
    }

    public static void SaveAnchor(string id)
    {
        var data = GetAnchorsData();
        var anchors = data["anchors"].Value<JArray>();
        anchors.Add(id);
        data["anchors"] = anchors;
        SaveFile(data);
    }

    public static List<string> GetSavedAnchorIdentifiers()
    {
        string str = File.ReadAllText(anchorFile);
        var anchors = JObject.Parse(str)["anchors"].Value<JArray>();
        List<string> res = new List<string>();
        foreach (var anchor in anchors) {
            res.Add(anchor.Value<string>());
        }
        return res;
    }

    private static JObject GetAnchorsData()
    {
        string str = File.ReadAllText(anchorFile);
        return JObject.Parse(str);
    }

    private static void SaveFile(JObject data)
    {
        File.WriteAllText(anchorFile, data.ToString());
    }

}
/*
{
	"anchors": ["a", "b", "c"]
}
*/