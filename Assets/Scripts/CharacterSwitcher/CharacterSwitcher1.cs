using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwitcher1 : MonoBehaviour
{
    [Header("Characters")]
    public GameObject Beanball;
    public GameObject Bjorn;
  
    public Camera mainCamera;
    public Vector3 cameraOffset = new Vector3(0, 0, -10);

    public GameObject activeCharacter;

    void Start()
    {
        Debug.Log("Starting");
        Bjorn.GetComponent<PlayerMovement>().enabled = false;
        Debug.Log("About to switch");
        SwitchTo(Beanball);
        Debug.Log("Switched to Beanball ");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (activeCharacter != null)
        {
            Vector3 targetPos = activeCharacter.transform.position;
            mainCamera.transform.position = new Vector3(
                targetPos.x + cameraOffset.x,
                targetPos.y + cameraOffset.y,
                cameraOffset.z
            );
        }
    }

    void SwitchTo(GameObject character)
    {
        
        if (activeCharacter != null)
        {
            GameObject oldCharacter = activeCharacter;
            //Swapping pos:
            Vector3 tempPos = oldCharacter.transform.position;
            Debug.Log("This is the current position" + tempPos);
            tempPos.y += 1;
            oldCharacter.transform.position = character.transform.position;
            character.transform.position = tempPos;
            Debug.Log ("This is the position after swap" + character.transform.position);

            var oldInp = activeCharacter.GetComponent<PlayerInput>();
            if (oldInp != null)
            {
                oldInp.enabled = false;
            }
            DisableController(activeCharacter);

        }

        activeCharacter = character;

        var newInp = activeCharacter.GetComponent<PlayerInput>();
        if (newInp != null)
        {
            newInp.enabled = true;
        }
        EnableController(activeCharacter);

        Debug.Log("Switched to " + activeCharacter.name);
    }

    public void OnSwap(InputValue value)
    {
        Debug.Log("Swap Is Running");
        if (value.isPressed)
        {
            if (activeCharacter == Beanball)
            {
                SwitchTo(Bjorn);
            }
            else if (activeCharacter == Bjorn)
            {
                SwitchTo(Beanball);
            }
            
            
        }
    }

    void EnableController(GameObject character)
    {
        if (character == Beanball)
        {
            Beanball.GetComponent<PlayerController2D>().enabled = true;
        }
        else if (character == Bjorn)
        {
            Bjorn.GetComponent<PlayerMovement>().enabled = true;
        }
        
    }

    void DisableController(GameObject character)
    {
        if (character == Beanball)
        {
            Beanball.GetComponent<PlayerController2D>().enabled = false;
            Beanball.GetComponent<PlayerController2D>().ResetMovementInput();
        }
        else if (character == Bjorn)
        {
            Bjorn.GetComponent<PlayerMovement>().enabled = false;
        }
        
    }
}


