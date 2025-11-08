using UnityEngine;
using UnityEngine.InputSystem;

// Hello world :D

public class c_PlayerScript : MonoBehaviour
{
    InputAction InputAction_Move;
    InputAction InputAction_Jump;

    Rigidbody thisRigidbody;
    [SerializeField] float moveSpeed = 20f;

    private void Start()
    {
        InputAction_Move = InputSystem.actions.FindAction("Move");
        InputAction_Jump = InputSystem.actions.FindAction("Jump");

        thisRigidbody = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        PlayerInputUpdate();

        PlayerMoveUpdate();
    }

    void PlayerMoveUpdate()
    {
        thisRigidbody.MovePosition(gameObject.transform.position + new Vector3(0, 0, moveSpeed * Time.fixedDeltaTime));
    }

    #region Player Input

    Vector2 v2_PlayerMovementVector;
    bool b_JumpPressed;
    void PlayerInputUpdate()
    {
        v2_PlayerMovementVector = InputAction_Move.ReadValue<Vector2>();

        b_JumpPressed = InputAction_Jump.IsPressed();
    }

    #endregion
}
