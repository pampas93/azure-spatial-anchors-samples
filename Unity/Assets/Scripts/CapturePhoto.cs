// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using System.IO;
using System.Collections;
using UnityEngine;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class CapturePhoto : MonoBehaviour
{
    public static CapturePhoto Instance { 
        get {
            return instance;
        } 
    }
    private static CapturePhoto instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private GameObject parent;
    [SerializeField] private Renderer liveRenderer;
    WebCamTexture webCamTexture;
    [SerializeField] GameObject mainCamera;

    private void Start()
    {
        #if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        #endif

        webCamTexture = new WebCamTexture();
        liveRenderer.material.mainTexture = webCamTexture;
    }

    public void StartCamera()
    {
        mainCamera.SetActive(false);
        parent.SetActive(true);

        webCamTexture.Play();
        transform.localEulerAngles = new Vector3(0, 0, 360 - webCamTexture.videoRotationAngle);
    }

    public void Capture(string filepath, Action onDone = null)
    {
        if (File.Exists(filepath))
        {
            File.Delete(filepath);
        }

        StartCoroutine(TakePhoto(filepath, onDone));
    }

    IEnumerator TakePhoto(string filepath, Action onDone)
    {
        yield return new WaitForEndOfFrame(); 

        Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        // Since the camera on each device is portrait/landscape, we rotate the captured photo accordingly
        var rotateBy = 360 - webCamTexture.videoRotationAngle;
        Texture2D rotatedPhoto;
        switch(rotateBy) {
            case 90: rotatedPhoto = rotate90(photo);
                break;
            case 180: rotatedPhoto = rotate180(photo);
                break;
            case 270: rotatedPhoto = rotate270(photo);
                break;
            default:
                rotatedPhoto = photo;
                break;
        }

        //Encode to a PNG
        byte[] bytes = rotatedPhoto.EncodeToPNG();
        File.WriteAllBytes(filepath, bytes);

        mainCamera.SetActive(true);
        parent.SetActive(false);
        webCamTexture.Stop();
        onDone?.Invoke();
    }

    public Sprite GetImageSprite(string filepath)
    {
        if (File.Exists(filepath))
        {
            byte[] data = File.ReadAllBytes(filepath);
            Texture2D myTex = new Texture2D(1, 1);
            if (myTex.LoadImage(data))
            {
                return Sprite.Create(myTex, new Rect(Vector2.zero, new Vector2(myTex.width, myTex.height)), Vector2.zero);
            }
        }
        return null;
    }

#region Rotate Texture2D 90,180 or 270 based on Device Camera's videoRotationAngle
// From: https://forum.unity.com/threads/webcamtexture-flipping-and-rotating-90-degree-ios-and-android.143856/#post-3178752
    private Texture2D rotate90(Texture2D orig) 
    {
        print("doing rotate90");
        Color32[] origpix = orig.GetPixels32(0);
        Color32[] newpix = new Color32[orig.width * orig.height];
        for(int c = 0; c < orig.height; c++) {
        for(int r = 0; r < orig.width; r++) {
            newpix[orig.width * orig.height - (orig.height * r + orig.height) + c] =
            origpix[orig.width * orig.height - (orig.width * c + orig.width) + r];
        }
        }
        Texture2D newtex = new Texture2D(orig.height, orig.width, orig.format, false);
        newtex.SetPixels32(newpix, 0);
        newtex.Apply();
        return newtex;
    }
    
    private Texture2D rotate180(Texture2D orig) 
    {
        print("doing rotate180");
        Color32[] origpix = orig.GetPixels32(0);
        Color32[] newpix = new Color32[orig.width * orig.height];
        for(int i = 0; i < origpix.Length; i++) {
        newpix[origpix.Length - i - 1] = origpix[i];
        }
        Texture2D newtex = new Texture2D(orig.width, orig.height, orig.format, false);
        newtex.SetPixels32(newpix, 0);
        newtex.Apply();
        return newtex;
    }

    private Texture2D rotate270(Texture2D orig) 
    {
        print("doing rotate270");
        Color32[] origpix = orig.GetPixels32(0);
        Color32[] newpix = new Color32[orig.width * orig.height];
        int i = 0;
        for(int c = 0; c < orig.height; c++) {
        for(int r = 0; r < orig.width; r++) {
            newpix[orig.width * orig.height - (orig.height * r + orig.height) + c] = origpix[i];
            i++;
        }
        }
        Texture2D newtex = new Texture2D(orig.height, orig.width, orig.format, false);
        newtex.SetPixels32(newpix, 0);
        newtex.Apply();
        return newtex;
  }
#endregion
}
