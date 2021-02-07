// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { 
        get {
            return instance;
        } 
    }
    private static AudioManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        #if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
        #endif

        audioSource = GetComponent<AudioSource>();
    }

    AudioClip currClip;
    AudioSource audioSource;

    public void StartRecording()
    {
        currClip = null;
        currClip = Microphone.Start(null, false, 10, 44100);
    }

    public bool StopSaveRecording(string filepath)
    {
        if (currClip != null)
        {
            Microphone.End(null);
            return SavWav.Save(filepath, currClip);
        }
        return false;
    }

    public void PlayRecording(string audioPath)
    {
        if (audioSource.clip == null)
        {
            StartCoroutine(AudioManager.Instance.LoadAudio(audioPath, (audio) => {
                if (audio == null) {
                    UIManager.Instance.SetDebugText("Error loading audio clip");
                } else {
                    audioSource.clip = audio;
                    audioSource.Play();
                }
            }));
        }
        else
        {
            audioSource.Play();
        }
    }

    public void ClearAudioClip()
    {
        audioSource.clip = null;
    }

    // Loading the .wav file from local storage, and calls onDone with the AudioClip
    public IEnumerator LoadAudio(string audioPath, Action<AudioClip> onDone) 
    {
        using (var webReq = UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.WAV))
        {
            yield return webReq.SendWebRequest();

            if (webReq.isNetworkError || webReq.isHttpError)
            {
                Debug.LogError(webReq.error);
                yield break;
            }

            DownloadHandlerAudioClip dlHandler = (DownloadHandlerAudioClip)webReq.downloadHandler;

            if (dlHandler.isDone)
            {
                AudioClip audioClip = dlHandler.audioClip;

                if (audioClip != null)
                {
                    audioClip = DownloadHandlerAudioClip.GetContent(webReq);
                    onDone?.Invoke(audioClip);
                    
                }
                else
                {
                    onDone?.Invoke(null);
                }
            }
            else
            {
                onDone?.Invoke(null);
            }
        }
    }
}