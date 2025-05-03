using UnityEngine;
using System.Runtime.InteropServices;

public class TextToSpeechManager : MonoBehaviour
{
    public void Speak(string message)
    {
        #if UNITY_ANDROID
        SpeakAndroid(message);
        #elif UNITY_IOS
        SpeakIOS(message);
        #else
        Debug.Log($"Text-to-Speech not supported on this platform. Message: {message}");
        #endif
    }

    private void SpeakAndroid(string message)
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaObject tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, null))
                {
                    tts.Call("speak", message, 0, null, null);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Android Text-to-Speech failed: {e.Message}");
        }
    }

    [DllImport("__Internal")]
    private static extern void _SpeakIOS(string message);

    private void SpeakIOS(string message)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _SpeakIOS(message);
        }
        else
        {
            Debug.LogWarning("Text-to-Speech is not supported on this platform.");
        }
    }
}
