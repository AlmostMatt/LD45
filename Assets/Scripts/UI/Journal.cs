using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Journal : MonoBehaviour
{
    public void ToggleJournal()
    {
        UIController.Get().ToggleJournal();
    }

}
