using System.Runtime.InteropServices;
using UnityEngine;

public class MicAccess : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RequestMicPermission();
#endif

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RequestMicPermission();
#else
        Debug.Log("Requesting mic via Unity (non-WebGL)");
#endif
    }
}
