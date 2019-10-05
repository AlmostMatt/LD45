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
        ShowUI();
        // Button2 text = Dismiss
        // Button1.SetCallback(HideUI);
        // Button2.SetCallback(HideUI);
    }

    public void ShowMessage(string message)
    {
        ShowUI();
        transform.Find("dialogView/V/empty/dialogText").GetComponent<Text>().text = message;
    }

    private void ShowUI()
    {
        transform.Find("dialogView").gameObject.SetActive(true);
    }

    public void HideUI()
    {
        transform.Find("dialogView").gameObject.SetActive(false);
    }
}
