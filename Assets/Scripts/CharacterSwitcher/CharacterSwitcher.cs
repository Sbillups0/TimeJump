using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Characters")]
    public GameObject Beanball;
    public GameObject Bjorn;
    public GameObject Eres;
    public Camera mainCamera;
    public Vector3 cameraOffset = new Vector3(0, 0, -10);

    private GameObject activeCharacter;

    void Start()
    {
        Debug.Log("Starting");
        Beanball.GetComponent<PlayerController2D>().enabled = false;
        Bjorn.GetComponent<PlayerMovement>().enabled = false;
        Eres.GetComponent<PlayerController>().enabled = false;
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
                SwitchTo(Eres);
            }
            else if (activeCharacter == Eres)
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
        else if (character == Eres)
        {
            Eres.GetComponent<PlayerController>().enabled = true;
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
        else if (character == Eres)
        {
            Eres.GetComponent<PlayerController>().enabled = false;
        }
    }
}


