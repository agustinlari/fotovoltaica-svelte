using UnityEngine;

public class VibrationManager : MonoBehaviour
{
    public static void Vibrate(long milliseconds = 30)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Método universal que funciona en todas las versiones de Unity
        using (var unityPlayer = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var currentActivity = unityPlayer.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity"))
        using (var vibrator = currentActivity.Call<UnityEngine.AndroidJavaObject>("getSystemService", "vibrator"))
        {
            if (GetAndroidVersion() >= 26) // Android 8.0+ (Oreo)
            {
                using (var vibrationEffect = new UnityEngine.AndroidJavaClass("android.os.VibrationEffect"))
                {
                    var effect = vibrationEffect.CallStatic<UnityEngine.AndroidJavaObject>(
                        "createOneShot", 
                        milliseconds, 
                        80 // Amplitud (1-255)
                    );
                    vibrator.Call("vibrate", effect);
                }
            }
            else // Versiones antiguas
            {
                vibrator.Call("vibrate", milliseconds);
            }
        }
#endif
    }

    private static int GetAndroidVersion()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var version = new UnityEngine.AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
#else
        return 0;
#endif
    }

    // Ejemplo de vibración corta para botones
    public static void VibrateButton() => Vibrate(15);
}