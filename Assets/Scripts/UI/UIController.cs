using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The callback function type - should take an integer argument that is the button index
public delegate void UIButtonCallback(int btnIdx);

/**
 * Contains functions related to player interaction
 */
public class UIController : MonoBehaviour
{
    public static UIController Get() { return GameObject.FindWithTag("UICanvas").GetComponent<UIController>(); }
    private UIButtonCallback[] mButtonCallbacks;

    void Start()
    {
    }

    void Update()
    {
    }

    public bool IsVisible()
    {
        return transform.Find("dialogView").gameObject.activeInHierarchy;
    }

    public void ShowDialog(int personId)
    {
        Sprite head = GameState.Get().GetPerson(personId).HeadSprite;

        ShowUI(new Sprite[] { head }, "Hi there!", new string[] { "Reply", "Dismiss" }, new UIButtonCallback[] { null, null});
        // Button2 text = Dismiss
        // Button1.SetCallback(HideUI);
        // Button2.SetCallback(HideUI);
    }

    public void ShowMessage(Sprite[] images, string message, string[] buttonTexts, UIButtonCallback[] callbacks)
    {
        ShowUI(images, message, buttonTexts, callbacks);
    }

    public void HideUI()
    {
        transform.Find("dialogView").gameObject.SetActive(false);
    }

    private void ShowUI(Sprite[] sprites, string dialogText, string[] buttonTexts, UIButtonCallback[] callbacks)
    {
        // Make the UI visible
        transform.Find("dialogView").gameObject.SetActive(true);

        // Display sprites
        // Hide extra sprites and create new sprites as needed
        Transform spriteContainer = transform.Find("dialogView/H images");
        for (int i = 0; i < Mathf.Max(sprites.Length, spriteContainer.childCount); i++)
        {
            Image image;
            if (i < spriteContainer.childCount)
            {
                image = spriteContainer.GetChild(i).GetComponent<Image>();
            }
            else
            {
                image = Instantiate(spriteContainer.GetChild(0), spriteContainer).GetComponent<Image>();
            }
            image.gameObject.SetActive(i < sprites.Length);
            if (i < sprites.Length)
            {
                // A null sprite can take up space
                image.color = sprites[i] == null ? new Color(0, 0, 0, 0) : Color.white;
                // Update a visible button
                image.sprite = sprites[i];
            }
        }

        // Display dialog text
        transform.Find("dialogView/V overlay/dialog/text").GetComponent<Text>().text = dialogText;

        // Update buttons
        if (buttonTexts.Length != callbacks.Length) { Debug.LogWarning("buttonTexts and callbacks have different length."); }
        mButtonCallbacks = callbacks;
        // Hide extra buttons and create new buttons as needed
        Transform buttonContainer = transform.Find("dialogView/V overlay/H buttons");
        for (int i=0; i < Mathf.Max(buttonTexts.Length, buttonContainer.childCount); i++)
        {
            Transform button;
            if (i < buttonContainer.childCount)
            {
                button = buttonContainer.GetChild(i);
            } else
            {
                button = Instantiate(buttonContainer.GetChild(0), buttonContainer);
            }
            button.gameObject.SetActive(i < buttonTexts.Length);
            if (i < buttonTexts.Length)
            {
                // Update a visible button
                button.GetChild(0).GetComponent<Text>().text = buttonTexts[i];
            }
        }
    }

    public void OnButtonClick(Button button)
    {
        // Start by hiding the UI even though the callback might cause UI to reappear.
        HideUI();
        // Run any custom callback function with the button index as an argument.
        int i = button.transform.GetSiblingIndex();
        if (mButtonCallbacks[i] != null)
        {
            mButtonCallbacks[i](i);
        }
    }
}
