using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

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

    private static string UNSELECTED_DROPDOWN_VALUE = "?";

    void Start()
    {
    }

    void Update()
    {
        // Enable or disable the speak button based on whether or not the current sentence is valid.
        Button speakButton = transform.Find("dialogView/V overlay/H sentenceBuilder/Button").GetComponent<Button>();
        speakButton.interactable = IsValidSentence();
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
        // TODO: make this conditional on the AI having something to share
        discussion.QueueInfoExchangeRequest(otherPerson, new Sprite[] { otherPerson.HeadSprite }, "Hi! Want to exchange information?", AudioClipIndex.HI);
        discussion.Start();
    }

    // Displays the sentence-construction UI.
    public void AskForSentence(Sprite[] images, UISentenceCallback sentenceCallback, List<string> subjectOverrides = null, List<string> objectOverrides = null)
    {
        ShowUISentence(images, sentenceCallback, subjectOverrides, objectOverrides);
    }

    // Displays a message
    public void ShowMessage(PersonState speaker, Sprite[] images, string message, string[] buttonTexts, UIButtonCallback[] callbacks)
    {
        ShowUIMessage(speaker, images, message, buttonTexts, callbacks);
    }

    public void HideUI()
    {
        transform.Find("dialogView").gameObject.SetActive(false);
        transform.Find("journalContent").gameObject.SetActive(false);
    }

    public void ShowJournalButton()
    {
        transform.Find("journalButton").gameObject.SetActive(true);
    }

    public void ShowJournal()
    {
        transform.Find("journalContent").gameObject.SetActive(false);
        ToggleJournal();
    }

    public void ToggleJournal()
    {
        GameObject journal = transform.root.gameObject.transform.Find("journalContent").gameObject;
        journal.SetActive(!journal.activeSelf);

        if(journal.activeSelf)
        {
            List<PlayerJournal.SentenceHistory> strings = PlayerJournal.GetJournal();
            if(strings.Count > 0)
                journal.GetComponentInChildren<Text>().text = "• " + string.Join("\n• ", strings);
        }
    }

    private void ShowUIMessage(PersonState speaker, Sprite[] sprites, string dialogText, string[] buttonTexts, UIButtonCallback[] callbacks)
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
        if (speaker != null)
        {
            transform.Find("dialogView/V overlay/dialog/nameplate/text").GetComponent<Text>().text = speaker.PublicName;
        }
        transform.Find("dialogView/V overlay/dialog/nameplate/text").gameObject.SetActive(speaker != null);
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
        if (buttonTexts.Length == 1)
        {
            EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(buttonContainer.GetChild(0).gameObject);
        } else
        {
            EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
        }
    }

    private void ShowUISentence(Sprite[] sprites, UISentenceCallback callback, List<string> subjectOverrides = null, List<string> objectOverrides = null)
    {
        // Show the Journal, so that the player can give a good clue
        ShowJournal();

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
        List<Noun> knownWords = GameState.Get().Player.knowledge.KnownWords.ToList();
        knownWords.Sort();
        List<string> knownWordStrings = knownWords.ConvertAll<string>(noun => noun.ToString());

        Transform sentenceBuilder = transform.Find("dialogView/V overlay/H sentenceBuilder");
        Dropdown subjectDropdown = sentenceBuilder.Find("Subject").GetComponent<Dropdown>();
        subjectDropdown.ClearOptions();
        if (subjectOverrides != null)
        {
            subjectDropdown.AddOptions(subjectOverrides);
        }
        else
        {
            subjectDropdown.AddOptions(knownWordStrings);
        }
        subjectDropdown.RefreshShownValue();

        Dropdown objectDropdown = sentenceBuilder.Find("DirectObject").GetComponent<Dropdown>();
        objectDropdown.ClearOptions();
        if(objectOverrides != null)
        {
            objectDropdown.AddOptions(objectOverrides);
        }
        else
        {
            List<string> tmp = new List<string>();
            tmp.Add(UNSELECTED_DROPDOWN_VALUE);
            objectDropdown.AddOptions(tmp);
            objectDropdown.AddOptions(knownWordStrings);
        }
        objectDropdown.RefreshShownValue();

        mSentenceCallback = callback;
        EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(subjectDropdown.gameObject);
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
        if (IsValidSentence())
        {
            Sentence sentence = GetInputSentence();
            HideUI();
            mSentenceCallback(sentence);
        }
        // Otherwise, keep the UI open
    }

    private Sentence GetInputSentence()
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

        bool useHas = mySubject.UseHas() || myObject.UseHas();
        Verb verb = useHas ? Verb.Has : Verb.Is;
        Sentence sentence = new Sentence(mySubject, verb, myObject, Adverb.True);
        return sentence;
    }

    private bool IsValidSentence()
    {
        Transform sentenceBuilder = transform.Find("dialogView/V overlay/H sentenceBuilder");
        string subject = sentenceBuilder.Find("Subject/Label").GetComponent<Text>().text;
        string directObject = sentenceBuilder.Find("DirectObject/Label").GetComponent<Text>().text;
        if (directObject.Equals(UNSELECTED_DROPDOWN_VALUE))
        {
            return false;
        }
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
        // If something failed to parse, consider the sentence valid anyways rather than breaking the game
        // Don't allow sentences that compare objects of the same type
        return (parsedSubject == false || parsedObject == false || mySubject.Type() != myObject.Type());
    }
}
