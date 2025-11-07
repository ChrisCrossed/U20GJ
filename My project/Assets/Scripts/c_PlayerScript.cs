using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// Hello world :D

public class c_PlayerScript : MonoBehaviour
{
    InputActionAsset InputAsset;
    InputAction InputAction_Move;

    private void Start()
    {
        InputAsset.FindActionMap("Player").Enable();

        InputAction_Move = InputSystem.actions.FindAction("Move");
        // InputAction_Move.AddBinding("<Keyboard&Mouse>/WASD");
        InputAction_Move.Enable();
    }

    private void Update()
    {
        PlayerInputUpdate();
    }

    #region Player Input

    Vector2 v2_PlayerMovementVector;
    void PlayerInputUpdate()
    {
        // v2_PlayerMovementVector = new Vector2();

        v2_PlayerMovementVector = InputAction_Move.ReadValue<Vector2>();

        print(InputAction_Move.ReadValue<Vector2>());

        if( v2_PlayerMovementVector != Vector2.zero )
        {
            print(v2_PlayerMovementVector);
        }
    }

    #endregion
}
