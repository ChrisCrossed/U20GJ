using System.Collections;
using System;
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
        ObjectClipStart();

        InputAction_Move = InputSystem.actions.FindAction("Move");
        InputAction_Jump = InputSystem.actions.FindAction("Jump");

        thisRigidbody = gameObject.GetComponent<Rigidbody>();

        CurrentMovingState = MovingState.Running;

        StartCoroutine(Gravity());
    }

    private void Update()
    {
        PlayerInputUpdate();

        ObjectClipUpdate();
    }

    private void FixedUpdate()
    {
        PlayerMoveUpdate();
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

        if(b_JumpPressed && !isJumping)
        {
            StartCoroutine( Jump() );
        }

        

        // print("New Vel: " + thisRigidbody.linearVelocity.magnitude);

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
        print("Jumping");

        isJumping = true;

        float upwardVelocity = 300.0f;

        touchingGround = false;

        // while(b_JumpPressed && !touchingGround)
        while(b_JumpPressed && !touchingGround)
        {
            upwardVelocity -= Time.fixedDeltaTime * 100.0f;

            if(upwardVelocity < 0)
                upwardVelocity = 0;

            thisRigidbody.AddForce(new Vector3(0f, upwardVelocity, 0f));

            yield return new WaitForEndOfFrame();
        }

        isJumping = false;

        yield return null;
    }

    bool touchingGround = false;
    private IEnumerator Gravity()
    {
        int layerMask = LayerMask.GetMask("Ground");
        float playerHeight = gameObject.transform.localScale.y;
        
        RaycastHit _hit;

        while (true)
        {
            float prevY = gameObject.transform.position.y;
            float currY = gameObject.transform.position.y;

            if (prevY < currY)
            {
                //print("Air");
                touchingGround = false;
            }
            else
            {
                if (Physics.Raycast(gameObject.transform.position, Vector3.down, out _hit, playerHeight + 0.1f, layerMask))
                {
                    Debug.DrawLine(gameObject.transform.position, _hit.point, Color.red);

                    if (_hit.distance < playerHeight + 0.1f)
                    {
                        if(!touchingGround)
                        {
                            Vector3 v3_RemoveYVel = thisRigidbody.linearVelocity;
                            v3_RemoveYVel.y = 0f;
                            thisRigidbody.linearVelocity = v3_RemoveYVel;

                            print(_hit.point);
                            /*
                            Vector3 v3_FinalPosition = thisRigidbody.transform.position;
                            v3_FinalPosition.y = _hit.point.y + playerHeight;
                            thisRigidbody.transform.position = v3_FinalPosition;
                            */
                        }

                        //print("Touching Ground");
                        touchingGround = true;
                    }
                    else
                    {
                        //print("Air");
                        touchingGround = false;
                    }
                }
            }

            if(!touchingGround)
            {
                thisRigidbody.AddForce(new Vector3(0f, Physics.gravity.y * Time.fixedDeltaTime * 1000f, 0f));
            }

            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("Collision: " + collision.gameObject.name);

        

        print("Curr Force: " + Physics.gravity.y * Time.fixedDeltaTime * 1000f);
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

    #region Object Clipping Resolution
    public LayerMask obstacleLayers;
    public Color mtvColor = Color.yellow;
    public bool autoResolve = true;
    public bool smoothResolve = true;

    public event Action<Vector3> OnPenetrationStart;
    public event Action<Vector3> OnPenetrationStay;
    public event System.Action OnPenetrationEnd;

    Collider col;
    Vector3 lastCorrection;
    bool resolvingCollision;

    void ObjectClipStart()
    {
        col = GetComponent<Collider>();

        OnPenetrationStart += correction =>
        {
            float penetrationDepth = correction.magnitude;
        };
    }

    void ObjectClipUpdate()
    {
        bool colliding = col.GetPenetrationsInLater(obstacleLayers, out Vector3 correction);
        correction += correction.normalized * 0.001f;
        lastCorrection = colliding ? correction : Vector3.zero;

        if(colliding)
        {
            if (!resolvingCollision) OnPenetrationStart?.Invoke(correction);
            else OnPenetrationStay?.Invoke(correction);

            resolvingCollision = true;

            if(autoResolve)
            {
                Vector3 delta = smoothResolve
                    ? Vector3.Lerp(Vector3.zero, correction, 0.05f)
                    : correction;

                transform.position += delta;
            }

            Debug.Log($"Colliding, MTV = {correction.magnitude:F3}");
        }
        else
        {
            if (resolvingCollision) OnPenetrationEnd?.Invoke();
            resolvingCollision = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<Collider>();
        if (col == null) return;

        if(lastCorrection != Vector3.zero)
        {
            Vector3 start = col.bounds.center;
            Vector3 end = start + lastCorrection;
            Gizmos.color = mtvColor;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.05f);
        }
    }


    #endregion
}
