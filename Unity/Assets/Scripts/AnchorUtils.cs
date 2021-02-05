// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;
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

    public static void SaveAnchor(AnchorData anchor)
    {
        try
        {
            var data = GetAnchorsData();
            var anchors = data["anchors"].Value<JArray>();
            var jsonObj = JObject.Parse(JsonConvert.SerializeObject(anchor, Formatting.Indented));
            anchors.Add(jsonObj);
            data["anchors"] = anchors;
            SaveFile(data);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    static List<AnchorData> savedAnchors;
    public static List<string> GetSavedAnchorIdentifiers()
    {
        savedAnchors = new List<AnchorData>();
        var data = GetAnchorsData();        
        var anchors = data["anchors"].Value<JArray>();
        List<string> res = new List<string>();
        foreach (var anchor in anchors) {
            try 
            {
                AnchorData obj = JsonConvert.DeserializeObject<AnchorData>(anchor.ToString());                    
                savedAnchors.Add(obj);
                res.Add(obj.ID);

            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
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

    public static void DeleteAnchorsFile()
    {
        File.Delete(anchorFile);
    }
}

public class AnchorData
{
    public string ID { get; private set; }
    public string Timestamp { get; private set; }
    public string Notes { get; private set; }
    public bool HasImage { get; private set; }
    public bool HasAudio { get; private set; }

    public AnchorData(string id, string notes)
    {
        ID = id;
        Timestamp = DateTime.UtcNow.ToString();
        Notes = notes;
    }
}
/*
{
	"anchors": ["a", "b", "c"]
}
*/