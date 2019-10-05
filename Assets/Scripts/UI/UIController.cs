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

        ShowUI(head, "Hi there!", new string[] { "Reply", "Dismiss" }, new UIButtonCallback[] { null, null});
        // Button2 text = Dismiss
        // Button1.SetCallback(HideUI);
        // Button2.SetCallback(HideUI);
    }

    public void ShowMessage(Sprite imageName, string message, string[] buttonTexts, UIButtonCallback[] callbacks)
    {
        ShowUI(imageName, message, buttonTexts, callbacks);
    }

    public void HideUI()
    {
        transform.Find("dialogView").gameObject.SetActive(false);
    }

    private void ShowUI(Sprite sprite,string dialogText, string[] buttonTexts, UIButtonCallback[] callbacks)
    {
        transform.Find("dialogView/face").GetComponent<Image>().sprite = sprite;
        transform.Find("dialogView/face").gameObject.SetActive(sprite != null);

        if (buttonTexts.Length != callbacks.Length) { Debug.LogWarning("buttonTexts and callbacks have different length."); }
        transform.Find("dialogView").gameObject.SetActive(true);
        transform.Find("dialogView/V/empty/dialogText").GetComponent<Text>().text = dialogText;
        // Update buttons
        mButtonCallbacks = callbacks;
        // Hide extra buttons and create new buttons as needed
        Transform buttonContainer = transform.Find("dialogView/V/H");
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
        int i = button.transform.GetSiblingIndex();
        if (mButtonCallbacks[i] != null)
        {
            mButtonCallbacks[i](i);
        } else
        {
            // a default button response.
            HideUI();
        }
    }
}
