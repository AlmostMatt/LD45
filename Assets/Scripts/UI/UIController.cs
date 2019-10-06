using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

// The callback function type - should take an integer argument that is the button index
public delegate void UIButtonCallback(int btnIdx);
public delegate void UISentenceCallback(Sentence sentence);

/**
 * Contains functions related to player interaction
 */
public class UIController : MonoBehaviour
{
    public static UIController Get() { return GameObject.FindWithTag("UICanvas").GetComponent<UIController>(); }
    private UIButtonCallback[] mButtonCallbacks;
    private UISentenceCallback mSentenceCallback;

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

    // Starts a 1:1 discussion with an information exchange
    public void ShowDialog(int personId)
    {
        PersonState player = GameState.Get().Player;
        PersonState otherPerson = GameState.Get().GetPerson(personId);
        DialogBlock discussion = new DialogBlock(new PersonState[] { player, otherPerson  }, null);
        discussion.QueueDialogue(otherPerson, new Sprite[] { otherPerson.HeadSprite }, "Hi there! Let's share info!");
        // TODO: add buttons for the choice of whether or not to share info.
        // ShowUIMessage(new Sprite[] { head }, "Hi there!", new string[] { "Reply", "Dismiss" }, new UIButtonCallback[] { null, null });
        discussion.QueueInformationExchange();
        discussion.Start();
    }

    // Displays the sentence-construction UI.
    public void AskForSentence(Sprite[] images, UISentenceCallback sentenceCallback)
    {
        ShowUISentence(images, sentenceCallback);
    }

    // Displays a message
    public void ShowMessage(Sprite[] images, string message, string[] buttonTexts, UIButtonCallback[] callbacks)
    {
        ShowUIMessage(images, message, buttonTexts, callbacks);
    }

    public void HideUI()
    {
        transform.Find("dialogView").gameObject.SetActive(false);
        transform.Find("journalText").gameObject.SetActive(false);
    }

    public void ShowJournalButton()
    {
        transform.Find("journalButton").gameObject.SetActive(true);
    }

    public void ToggleJournal()
    {
        GameObject journal = transform.root.gameObject.transform.Find("journalText").gameObject;
        journal.SetActive(!journal.activeSelf);

        if(journal.activeSelf)
        {
            List<PlayerJournal.SentenceHistory> strings = PlayerJournal.GetJournal();
            journal.GetComponent<Text>().text = string.Join("\n", strings);
        }
    }

    private void ShowUIMessage(Sprite[] sprites, string dialogText, string[] buttonTexts, UIButtonCallback[] callbacks)
    {
        // Make the UI visible
        transform.Find("dialogView").gameObject.SetActive(true);
        transform.Find("dialogView/V overlay/dialog").gameObject.SetActive(true);
        transform.Find("dialogView/V overlay/H buttons").gameObject.SetActive(true);
        transform.Find("dialogView/V overlay/H sentenceBuilder").gameObject.SetActive(false);

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
                // Update a visible sprite
                image.sprite = null; // clear sprite to reset size information
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
        for (int i = 0; i < Mathf.Max(buttonTexts.Length, buttonContainer.childCount); i++)
        {
            Transform button;
            if (i < buttonContainer.childCount)
            {
                button = buttonContainer.GetChild(i);
            }
            else
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

    private void ShowUISentence(Sprite[] sprites, UISentenceCallback callback)
    {
        // Make the UI visible
        transform.Find("dialogView").gameObject.SetActive(true);
        transform.Find("dialogView/V overlay/dialog").gameObject.SetActive(false);
        transform.Find("dialogView/V overlay/H buttons").gameObject.SetActive(false);
        transform.Find("dialogView/V overlay/H sentenceBuilder").gameObject.SetActive(true);

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
                // Update a visible sprite
                image.sprite = null; // clear sprite to reset size information
                image.sprite = sprites[i];
            }
        }

        // Set the list of options to the list of discovered words.
        // Default to having discovered the hair colors of people other than yourself.
        HashSet<Noun> knownWords = GameState.Get().Player.knowledge.KnownWords;
        List<string> knownWordStrings = knownWords.ToList().ConvertAll<string>(noun => noun.ToString());
        Transform sentenceBuilder = transform.Find("dialogView/V overlay/H sentenceBuilder");
        Dropdown subjectDropdown = sentenceBuilder.Find("Subject").GetComponent<Dropdown>();
        subjectDropdown.ClearOptions();
        subjectDropdown.AddOptions(knownWordStrings);
        subjectDropdown.RefreshShownValue();
        Dropdown objectDropdown = sentenceBuilder.Find("DirectObject").GetComponent<Dropdown>();
        objectDropdown.ClearOptions();
        objectDropdown.AddOptions(knownWordStrings);
        objectDropdown.RefreshShownValue();

        mSentenceCallback = callback;
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

    public void OnSentenceClick(Button button)
    {
        Transform sentenceBuilder = transform.Find("dialogView/V overlay/H sentenceBuilder");
        string subject = sentenceBuilder.Find("Subject/Label").GetComponent<Text>().text;
        string directObject = sentenceBuilder.Find("DirectObject/Label").GetComponent<Text>().text;
        bool parsedSubject = System.Enum.TryParse(subject, out Noun mySubject);
        if (!parsedSubject)
        {
            Debug.LogWarning("Invalid noun - " + subject);
        }
        bool parsedObject = System.Enum.TryParse(directObject, out Noun myObject);
        if (!parsedObject)
        {
            Debug.LogWarning("Invalid noun - " + directObject);
        }
        Sentence sentence = new Sentence(mySubject, Verb.Is, myObject, Adverb.True);
        if (parsedSubject && parsedObject && sentence.Subject == sentence.DirectObject)
        {
            // Words parsed correctly and the sentence is Invalid!
            // TODO: make the button dynamically gray out when the sentence is invalid
            // This should be doable in Update()
        } else
        {
            HideUI();
            mSentenceCallback(sentence);
        }
    }
}
