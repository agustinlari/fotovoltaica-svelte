using UnityEngine;

public class PermissionsManager : MonoBehaviour
{
	void Start()
	{
		// Verificar que estamos en Android
		if (Application.platform == RuntimePlatform.Android)
		{
			// Verificar permisos de lectura/escritura en almacenamiento externo
			if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageRead))
			{
				UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageRead);
			}

			if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
			{
				UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
			}

			// Verificar permisos específicos para fotos y videos en Android 13+
			if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.READ_MEDIA_IMAGES"))
			{
				UnityEngine.Android.Permission.RequestUserPermission("android.permission.READ_MEDIA_IMAGES");
			}

			if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.READ_MEDIA_VIDEO"))
			{
				UnityEngine.Android.Permission.RequestUserPermission("android.permission.READ_MEDIA_VIDEO");
			}
		}
	}
}