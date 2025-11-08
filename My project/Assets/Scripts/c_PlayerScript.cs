using System.Collections;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.InputSystem;

// Hello world :D

public enum MovingState
{
    Running,
    Dashing,
    WallJumping,
    Snapback,
    Dead
}

public class c_PlayerScript : MonoBehaviour
{
    InputAction InputAction_Move;
    InputAction InputAction_Jump;

    Rigidbody thisRigidbody;
    [SerializeField] float moveSpeed = 100f;

    MovingState CurrentMovingState;

    private void Start()
    {
        InputAction_Move = InputSystem.actions.FindAction("Move");
        InputAction_Jump = InputSystem.actions.FindAction("Jump");

        thisRigidbody = gameObject.GetComponent<Rigidbody>();

        CurrentMovingState = MovingState.Running;
    }

    private void Update()
    {
        PlayerInputUpdate();

        PlayerMoveUpdate();
    }

    private void FixedUpdate()
    {
        
    }

    Vector3 totalMoveVector;
    Vector3 previousVelocity;
    void PlayerMoveUpdate()
    {
        previousVelocity = thisRigidbody.linearVelocity;

        if(thisRigidbody.linearVelocity.magnitude < moveSpeed )
        {
            totalMoveVector = previousVelocity + new Vector3(0, 0, moveSpeed);

            thisRigidbody.AddForce(totalMoveVector - previousVelocity);
        }

        print("New Vel: " + thisRigidbody.linearVelocity.magnitude);

        // thisRigidbody.linearVelocity = totalMoveVector - previousVelocity;

        /*
        previousVelocity = thisRigidbody.linearVelocity;

        totalMoveVector = new Vector3();

        if(CurrentMovingState == MovingState.Running)
        {
            totalMoveVector += new Vector3(0, 0, moveSpeed);
        }

        if (b_JumpPressed && !isJumping)
        {
            StartCoroutine(Jump());
        }

        print("New Vel: " + totalMoveVector);
        print("Old Vel: " + previousVelocity);
        print("Total Vel: " + (totalMoveVector - previousVelocity));
        print("---");

        totalMoveVector = (totalMoveVector - previousVelocity) * Time.deltaTime;

        // thisRigidbody.AddForce(totalMoveVector - previousVelocity);
        thisRigidbody.MovePosition(gameObject.transform.position + totalMoveVector);
        */
    }

    bool isJumping;
    private IEnumerator Jump()
    {
        isJumping = true;

        float upwardVelocity = 20.0f;

        while(b_JumpPressed)
        {
            Vector3 jumpVector = new Vector3();

            if (upwardVelocity > 0f)
            {
                jumpVector = gameObject.transform.position + new Vector3(0, upwardVelocity * Time.deltaTime, 0f);

                upwardVelocity -= Time.deltaTime * 2.0f;

                if(upwardVelocity < 0f)
                    upwardVelocity = 0f;
            }

            jumpVector += Physics.gravity * Time.deltaTime;

            thisRigidbody.linearVelocity = jumpVector;

            yield return new WaitForEndOfFrame();
        }

        isJumping = false;

        yield return null;
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
