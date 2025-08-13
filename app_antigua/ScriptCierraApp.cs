using UnityEngine;

public class ScriptCierraApp : MonoBehaviour
{
	// Método que se llamará al pulsar el botón
	public void Cerrar()
	{
		// Verifica si estamos en el editor de Unity
#if UNITY_EDITOR
		// Si estamos en el editor, se detiene la ejecución
		UnityEditor.EditorApplication.isPlaying = false;
#else
            // Si estamos en la compilación final (juego en PC, Android, iOS, etc.), cerramos la aplicación
            Application.Quit();
#endif
	}
}
