using System.Collections;
using System;
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

    Rigidbody2D thisRigidbody;
    
    [SerializeField] float moveSpeed = 10f;

    MovingState CurrentMovingState;

    private void Start()
    {
        ObjectClipStart();

        InputAction_Move = InputSystem.actions.FindAction("Move");
        InputAction_Jump = InputSystem.actions.FindAction("Jump");

        thisRigidbody = gameObject.GetComponent<Rigidbody2D>();

        CurrentMovingState = MovingState.Running;

        StartCoroutine( GravityStatus() );
    }

    private void Update()
    {


        PlayerInputUpdate();

        /*
        ObjectClipUpdate();
        */
    }

    private void FixedUpdate()
    {
        PlayerMoveUpdate();

        print("Velocity: " + thisRigidbody.linearVelocity.x);
    }

    Vector3 totalMoveVector;
    Vector3 previousVelocity;
    void PlayerMoveUpdate()
    {
        float accelerationRate = 10.0f;

        float speedDif = moveSpeed - thisRigidbody.linearVelocity.x;

        print("Speed Dif = " + moveSpeed + " - " + thisRigidbody.linearVelocityX + " = " + speedDif);

        float movement = speedDif * accelerationRate;

        print("Movement = " + speedDif + " * " + accelerationRate + " = " + movement);

        thisRigidbody.AddForce(movement * Vector2.right);

        print(" --- ");

        if (b_JumpPressed && !isJumping)
        {
            StartCoroutine( Jump() );
        }
    }

    bool isJumping;
    private IEnumerator Jump()
    {
        if (!touchingGround)
            yield break;

        print("Jumping");

        isJumping = true;

        float startingVelocity = 1000f;

        float time_MAX = 0.25f;
        float time = time_MAX;

        touchingGround = false;

        StartCoroutine( JumpScalingGravity() );

        // while(b_JumpPressed && !touchingGround)
        while(b_JumpPressed && !touchingGround)
        {
            time -= Time.fixedDeltaTime;

            float perc = time / time_MAX;

            float upwardVelocity = Mathf.Lerp(0f, startingVelocity, perc);

            if(time < 0)
                time = 0;

            // print(upwardVelocity);

            thisRigidbody.AddForce(upwardVelocity * Vector2.up);

            yield return new WaitForEndOfFrame();
        }

        isJumping = false;

        yield return null;
    }

    private IEnumerator JumpScalingGravity()
    {
        float time = 0f;
        float time_MAX = 1.0f;

        float gravityStart = 1f;
        float gravityEnd = 50f;

        while(time < time_MAX || !touchingGround)
        {
            time += Time.fixedDeltaTime;
            if(time > time_MAX)
                time = time_MAX;

            float perc = Mathf.Lerp(0f, time_MAX, time / time_MAX);

            float scalingGravity = 25f;
            if (time > 0f)
            {
                scalingGravity = Mathf.Lerp(gravityStart, gravityEnd, perc);
            }

            thisRigidbody.gravityScale = scalingGravity;
            // print(thisRigidbody.gravityScale);

            yield return new WaitForEndOfFrame();
        }

        print("Reaching Here");
        thisRigidbody.gravityScale = 1.0f;
        
        // MoveSpeed is 100f but for some reason capping at 90f. This is just matching that value.
        thisRigidbody.linearVelocity = 70f * Vector2.right;

        yield return null;
    }

    bool touchingGround = false;
    private IEnumerator GravityStatus()
    {
        print("STARTING GRAVITY STATUS");

        int layerMask = LayerMask.GetMask("Ground");
        float playerHeight = gameObject.transform.localScale.y;
        
        //RaycastHit _hit;
        RaycastHit2D _hit;

        while (true)
        {
            float prevY = gameObject.transform.position.y;
            float currY = gameObject.transform.position.y;

            if (prevY < currY)
            {
                // print("Air - CurrY");
                touchingGround = false;
            }
            else
            {
                //if (Physics.Raycast(gameObject.transform.position, Vector3.down, out _hit, playerHeight + 0.1f, layerMask))
                if (_hit = Physics2D.Raycast(gameObject.transform.position, Vector3.down, playerHeight + 0.1f, layerMask))
                {
                    Debug.DrawLine(gameObject.transform.position, _hit.point, Color.red);

                    if (_hit.distance < playerHeight + 0.1f)
                    {
                        // print("Ground");
                        touchingGround = true;
                    }
                    else
                    {
                        // print("Air - Falling");
                        touchingGround = false;
                    }
                }
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
