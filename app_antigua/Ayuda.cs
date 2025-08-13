using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ImageHoverAndClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Sprite highlightedSprite;        // Imagen al pasar el ratón
    public Canvas targetCanvas;             // Canvas que quieres mostrar
    private Image imageComponent;
    private Sprite originalSprite;
    public ComunicacionDB aux;

    void Start()
    {
        imageComponent = GetComponent<Image>();
        if (imageComponent != null)
        {
            originalSprite = imageComponent.sprite;
        }

        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(false); // Asegúrate de que esté desactivado al inicio
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightedSprite != null)
        {
            imageComponent.sprite = highlightedSprite;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        imageComponent.sprite = originalSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
        if (targetCanvas != null)
        {
            imageComponent.sprite = originalSprite;
            aux.AbrirNuevoCanvas(targetCanvas);
        }
    }
}
