using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * Contains functions related to player interaction
 */
public class UIController : MonoBehaviour
{
    public static UIController Get() { return GameObject.FindWithTag("UICanvas").GetComponent<UIController>(); }

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
        ShowUI(null, "Hi there!", new string[] { "Reply", "Dismiss" });
        // Button2 text = Dismiss
        // Button1.SetCallback(HideUI);
        // Button2.SetCallback(HideUI);
    }

    public void ShowMessage(string message)
    {
        ShowUI(null, message, new string[] {"Continue"});
    }

    public void HideUI()
    {
        transform.Find("dialogView").gameObject.SetActive(false);
    }

    private void ShowUI(Sprite sprite,string dialogText, string[] buttonTexts) // TODO: button callbacks
    {
        transform.Find("dialogView").gameObject.SetActive(true);
        transform.Find("dialogView/V/empty/dialogText").GetComponent<Text>().text = dialogText;
        // Update buttons
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
                button.GetChild(0).GetComponent<Text>().text = buttonTexts[i];
            }
        }
    }
}
