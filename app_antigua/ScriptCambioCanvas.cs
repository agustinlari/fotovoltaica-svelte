using UnityEngine;

public class SriptCambiaCambas : MonoBehaviour
{
	public GameObject canvasParaAbrir;

	// Método que se llamará al pulsar el botón
	public void AbrirNuevoCanvas()
	{
		if (canvasParaAbrir != null)
		{
			// Desactivar todos los Canvas en la escena
			Canvas[] canvases = FindObjectsOfType<Canvas>();
			foreach (Canvas canvas in canvases)
			{
				if (canvas.gameObject != canvasParaAbrir) // Si el Canvas no es 'Pantalla 2'
				{
					canvas.gameObject.SetActive(false); // Desactivamos el Canvas
				}
			}

			// Activar 'Pantalla 2'
			canvasParaAbrir.SetActive(true);
		}
		else
		{
			Debug.LogWarning("No se ha asignado el Canvas en el Inspector.");
		}
	}
}