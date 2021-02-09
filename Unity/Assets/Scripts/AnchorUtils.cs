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
    
    public static string AudioDir => Path.Combine(Application.persistentDataPath, "Audio");

    public static void Setup()
    {
        if (!File.Exists(anchorFile))
        {
            File.Create(anchorFile).Dispose();
            File.WriteAllText(anchorFile, "{\"anchors\" : [] }");
        }

        if (!Directory.Exists(AudioDir)) 
        {
            Directory.CreateDirectory(AudioDir);
        }
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

    public static AnchorData GetAnchorData(string id)
    {
        return savedAnchors.Find(a => a.ID ==  id);
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
    public string Notes { get; set; }
    public bool HasImage { get => DoesImageExist(); }
    public bool HasAudio { get => DoesAudioExist(); }

    public AnchorData(string id)
    {
        ID = id;
        Timestamp = DateTime.UtcNow.ToString();
    }

    public string GetNotes()
    {
        return string.IsNullOrEmpty(Notes) ? "No notes available" : Notes;
    }

    public void SetNotes(string notes)
    {
        Notes = notes;
    }

    public string GetAudioPath()
    {
        return Path.Combine(AnchorUtils.AudioDir, ID + ".wav");
    }

    public bool DoesAudioExist()
    {
        return File.Exists(GetAudioPath());
    }

    public void DeleteAudio()
    {
        var audioFile = GetAudioPath();
        if (File.Exists(audioFile))
        {
            File.Delete(audioFile);
        }
    }

    public string GetImagePath()
    {
        return Path.Combine(AnchorUtils.AudioDir, ID + ".png");
    }

    public bool DoesImageExist()
    {
        return File.Exists(GetImagePath());
    }

    public void DeleteImage()
    {
        var imageFile = GetImagePath();
        if (File.Exists(imageFile))
        {
            File.Delete(imageFile);
        }
    }
}
/*
{
	"anchors": [{
        "ID":"asd3awe23123",
        "Timestamp": "2/9/2021 5:08:13 AM",
        "Notes": "Just some anchor description"
        "HasImage": true,
        "HasAudio": false
    }, {
        "ID":"awddasdawd",
        "Timestamp": "2/9/2021 6:08:13 AM",
        "Notes": "Just some other anchor description"
        "HasImage": true,
        "HasAudio": true
    }]
}
*/