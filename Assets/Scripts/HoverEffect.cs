using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverEffect : MonoBehaviour
{
    private Vector3 HOVER_OFFSET = new Vector3(0f, 0f, 0f); // The problem with position offset is it can move away from the mouse
    private float HOVER_SCALE_MULT = 1.03f;

    private Vector3 originalPos;
    private Vector3 hoverPos;
    private Vector3 originalScale;
    private Vector3 hoverScale;

    // Start is called before the first frame update
    void Start()
    {
        originalPos = transform.localPosition;
        hoverPos = originalPos + HOVER_OFFSET;
        // Alternatively, scale so that absolute size of the associated sprite increased by ~20px.
        originalScale = transform.localScale;
        hoverScale = HOVER_SCALE_MULT * originalScale;
    }

    public void OnMouseOver() // Called every frame that the mouse is over something
    {
        if (PlayerInteraction.Get().CanInteractWithScene())
        {
            transform.localScale = hoverScale;
            transform.localPosition = hoverPos;
        } else
        {
            transform.localScale = originalScale;
            transform.localPosition = originalPos;
        }
    }

    public void OnMouseExit()
    {
        transform.localScale = originalScale;
        transform.localPosition = originalPos;
    }
}
