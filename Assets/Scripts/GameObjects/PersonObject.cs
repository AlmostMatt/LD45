using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonObject : MonoBehaviour
{
    public int personId;

    // Start is called before the first frame update
    void Start()
    {
        // destroy self if not in this room
        if(!GameState.Get().IsPersonInCurrentRoom(personId))
        {
            Destroy(gameObject);
        }
        PersonState person = GameState.Get().GetPerson(personId);
        GetComponent<SpriteRenderer>().sprite = person.PersonSprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        Debug.Log("Person " + personId + " selected");
        PlayerInteraction.Get().StartDialog(personId);
    }
}
