using UnityEngine;

public class ScriptCierraApp : MonoBehaviour
{
	// M�todo que se llamar� al pulsar el bot�n
	public void Cerrar()
	{
		// Verifica si estamos en el editor de Unity
#if UNITY_EDITOR
		// Si estamos en el editor, se detiene la ejecuci�n
		UnityEditor.EditorApplication.isPlaying = false;
#else
            // Si estamos en la compilaci�n final (juego en PC, Android, iOS, etc.), cerramos la aplicaci�n
            Application.Quit();
#endif
	}
}
