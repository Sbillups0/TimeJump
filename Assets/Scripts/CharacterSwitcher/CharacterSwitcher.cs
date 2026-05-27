using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Characters")]
    public GameObject Beanball;
    public GameObject Bjorn;
    public GameObject Eres;

    private GameObject activeCharacter;
    void Start()
    {
        Beanball.GetComponent<PlayerController>().enabled = false;
        Bjorn.GetComponent<PlayerController>().enabled = false;
        Eres.GetComponent<PlayerController>().enabled = false;
        SwitchTo(Beanball);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SwitchTo(GameObject character)
    {
        if (activeCharacter != null)
        {
            activeCharacter.GetComponent<PlayerController>().enabled = false;
        }
        activeCharacter = character;
        activeCharacter.GetComponent<PlayerController>().enabled = true;

        Debug.Log("Switched to " + activeCharacter.name);
    }

    public void OnSwap(InputValue value)
    { 
        if (value.isPressed)
        {
            if (activeCharacter == Beanball)
            {
                SwitchTo(Bjorn);
            }
            else if (activeCharacter == Bjorn)
            {
                SwitchTo(Eres);
            }
            else if (activeCharacter == Eres)
            {
                SwitchTo(Beanball);
            }
        }
    }
}


