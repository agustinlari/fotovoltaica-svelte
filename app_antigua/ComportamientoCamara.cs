using UnityEngine;
using System.Collections;

public class CameraHandler : MonoBehaviour
{
	void Start()
	{
		// Verifica y solicita permisos en tiempo de ejecución
		//RequestPermissions();
	}

	void RequestPermissions()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			// Permiso para la cámara
			if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.CAMERA"))
			{
				UnityEngine.Android.Permission.RequestUserPermission("android.permission.CAMERA");
			}

			// Permiso para leer imágenes (Android 13+)
			if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.READ_MEDIA_IMAGES"))
			{
				UnityEngine.Android.Permission.RequestUserPermission("android.permission.READ_MEDIA_IMAGES");
			}

			// Permiso para guardar imágenes en la galería (Android 14+)
			if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.READ_MEDIA_VISUAL_USER_SELECTED"))
			{
				UnityEngine.Android.Permission.RequestUserPermission("android.permission.READ_MEDIA_VISUAL_USER_SELECTED");
			}
		}
	}

	public void TakePicture()
	{
		if (NativeCamera.IsCameraBusy())
		{
			Debug.Log("La cámara está ocupada...");
			return;
		}

		// Llamar a la cámara del dispositivo
		NativeCamera.TakePicture((path) =>
		{
			if (path == null)
			{
				Debug.Log("No se tomó ninguna foto");
				return;
			}

			Debug.Log("Imagen guardada en: " + path);

			// Guardar la imagen en la galería con NativeGallery
			NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(path, "MiApp", "Foto_{0}.jpg");

			Debug.Log("Resultado al guardar la imagen: " + permission);
		}, 1024);
	}
}
