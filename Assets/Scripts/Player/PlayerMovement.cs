using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D col;
    private SpriteRenderer sprite;

    private Vector2 moveInput;
    private Vector2 pushSum;

    private ProjectilePool pool;



    
    private Vector2 velocity;
    private bool isGrounded;
    private bool jumpHeld;
    private bool jumpPressed;
    private float jumpBufferTimer;
    private float coyoteTimer;
    private int jumpsRemaining;
    private Vector2 groundNormal = Vector2.up;

    private ContactFilter2D moveFilter;
    private RaycastHit2D[] moveHits = new RaycastHit2D[8];
    [Header("Horizontal Movement")]
    [SerializeField] private float maxRunSpeed = 8f;
    [SerializeField] private float groundAcceleration = 90f;
    [SerializeField] private float groundDeceleration = 110f;
    [SerializeField] private float airAcceleration = 55f;
    [SerializeField] private float airDeceleration = 30f;

    [Header("Jump")]
    [SerializeField] private float jumpSpeed = 14f;
    [SerializeField] private float gravity = 42f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;
    [SerializeField] private float lowJumpGravityMultiplier = 2.2f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private int extraJumps = 1;

    [Header("Collision / Grounding")]
    [SerializeField] private float skinWidth = 0.02f;
    [SerializeField] private float groundCheckDistance = 0.08f;
    [SerializeField] private float groundSnapDistance = 0.12f;
    [SerializeField] private float maxGroundAngle = 40f;

    [Header("Step / Slope")]
    [SerializeField] private float stepHeight = 0.22f;
    [SerializeField] private float stepCheckDistance = 0.06f;



    
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float pushPower;
    // [SerializeField] private float frictionCoefficient = 0.9f; // Friction applied when no input or changing direction

    [Header("Step Up parameters")]
    [SerializeField] private LayerMask groundLayer;
    // [SerializeField] private float stepHeight = 0.25f;
    // [SerializeField] private float stepCheckDistance = 0.04f;
    // [SerializeField] private float stepUpAmount = 0.2f;


    private ContactFilter2D stepFilter;
    private RaycastHit2D[] stepHits = new RaycastHit2D[8];
    // [SerializeField] private Vector2 stepLowerCheckSize = new Vector2(0.1f, 0.1f);
    // [SerializeField] private Vector2 stepUpperCheckSize = new Vector2(0.08f, 0.08f);


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        pool = GetComponent<ProjectilePool>();

        stepFilter = new ContactFilter2D();
        stepFilter.useLayerMask = true;
        stepFilter.layerMask = groundLayer;
        stepFilter.useTriggers = false;



        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        moveFilter = new ContactFilter2D();
        moveFilter.useLayerMask = true;
        moveFilter.layerMask = groundLayer;
        moveFilter.useTriggers = false;
    }

    private void OnEnable()
    {
        if (GameInput.Instance == null) return;
        GameInput.Instance.JumpPressed += OnJump;
        GameInput.Instance.JumpReleased += ReleaseJump;
        GameInput.Instance.PushPressed += OnPush;
        GameInput.Instance.PushReleased += ReleasePush;
    }
    private void OnDisable()
    {
        if (GameInput.Instance == null) return;
        GameInput.Instance.JumpPressed -= OnJump;
        GameInput.Instance.JumpReleased -= ReleaseJump;
        GameInput.Instance.PushPressed -= OnPush;
        GameInput.Instance.PushReleased -= ReleasePush;
    }
    

    private void Update()
    {
        moveInput = GameInput.Instance != null ? GameInput.Instance.Move : Vector2.zero;

        // Flip transform based on move direction
        Vector3 scale = transform.localScale;
        if (moveInput.x > 0)
            scale.x = Mathf.Abs(scale.x);
        else if (moveInput.x < 0)
            scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;

        anim.SetBool("run", moveInput != Vector2.zero);

    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        UpdateTimers(dt);
        UpdateGroundState();
        ApplyHorizontal(dt);
        ApplyGravity(dt);
        TryConsumeJump();

        Vector2 delta = velocity * dt;

        MoveHorizontally(ref delta);
        MoveVertically(ref delta);

        rb.MovePosition(rb.position + delta);

        SnapToGround();

        jumpPressed = false;
    }

    // private void FixedUpdate()
    // {
    //     float xSpeed = moveInput.x * walkSpeed;
    //     if (Math.Abs(moveInput.x * walkSpeed) < Math.Abs(rb.linearVelocity.x))
    //         xSpeed = rb.linearVelocity.x * frictionCoefficient; // Apply friction when no input or changing direction

    //     rb.linearVelocity = new Vector2(xSpeed, rb.linearVelocity.y);
        
    //     TryStepUp();
    // }

    // public void OnJump()
    // {
    //     anim.SetTrigger("jump");
    //     rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
    // }
    // Instead of setting velocity directly, we set a flag to be processed in FixedUpdate for better physics consistency and to allow for jump buffering and coyote time.
    public void OnJump()
    {
        jumpPressed = true;
        jumpHeld = true;
    }

    // private void ReleaseJump()
    // {
    //     if (rb.linearVelocity.y > 0f)
    //         rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y / 2f);
    // }

    private void ReleaseJump()
    {
        jumpHeld = false;
    }

    // Push() will repel all of the Projectiles that have yet to hit something,
    // and TODO: ---------   will repel the player from them depending on weight difference. 
    // Coins have weight 0 - i.e. they will not affect player motion unless they hit something.
    
    private void OnPush()
    {
        Debug.Log("Push activated");
        pushSum = Vector2.zero;
        foreach (PlayerProjectile projectile in pool.All)
        {
            if (!projectile.gameObject.activeInHierarchy)
                continue;
            if (projectile.isHit())
            {
                Vector2 pushDir = (transform.position - projectile.transform.position).normalized;
                pushSum += pushDir * pushPower;
            }
            else
            {
                projectile.PushAway((projectile.transform.position - transform.position).normalized, pushPower);
            }
        }
        velocity += pushSum;
        // rb.linearVelocity += pushSum;
    }
    private bool IsGrounded()
    {
        Bounds bounds = col.bounds;

        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y - 0.02f);
        Vector2 boxSize = new Vector2(bounds.size.x * 0.9f, 0.05f);

        return Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer) != null;
    }
    // private void TryStepUp()
    // {
    //     if (Mathf.Abs(moveInput.x) < 0.01f) return;
    //     if (!IsGrounded()) return;
    //     if (rb.linearVelocity.y > 0.05f) return;

    //     Vector2 dir = new Vector2(Mathf.Sign(moveInput.x), 0f);

    //     int hitCount = col.Cast(dir, stepFilter, stepHits, stepCheckDistance);
    //     if (hitCount == 0) return;

    //     Vector2 originalPos = rb.position;
    //     rb.position = originalPos + Vector2.up * stepHeight;

    //     int raisedHitCount = col.Cast(dir, stepFilter, stepHits, stepCheckDistance);

    //     rb.position = originalPos;

    //     if (raisedHitCount == 0)
    //     {
    //         Debug.Log("STEP UP TRIGGERED");
    //         rb.position += Vector2.up * stepUpAmount;
    //     }
    // }
    private bool TryStepUp(Vector2 dir, float horizontalDistance)
    {
        if (!isGrounded) return false;
        if (Mathf.Abs(moveInput.x) < 0.01f) return false;
        if (velocity.y > 0.05f) return false;

        int lowerHitCount = col.Cast(dir, moveFilter, moveHits, stepCheckDistance);
        if (lowerHitCount == 0) return false;

        Vector2 originalPos = rb.position;

        rb.position = originalPos + Vector2.up * stepHeight;
        int raisedHitCount = col.Cast(dir, moveFilter, moveHits, horizontalDistance);
        rb.position = originalPos;

        if (raisedHitCount != 0) return false;

        rb.position = originalPos + Vector2.up * stepHeight;
        return true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Debug.Log($"Contact with {collision.collider.name} at {contact.point}, normal {contact.normal}");
        }
    }
    private void ReleasePush()
    {
        Debug.Log("Push released");
        // pushSum = Vector2.zero;
    }

    public bool CanAttack()
    {
        //TODO: implement
        return true;
    }

    public Vector2 GetCurrentVelocity() => rb.linearVelocity;



    private void UpdateTimers(float dt)
    {
        if (jumpPressed)
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= dt;

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= dt;
    }

    private void UpdateGroundState()
    {
        isGrounded = false;
        groundNormal = Vector2.up;

        int hitCount = col.Cast(Vector2.down, moveFilter, moveHits, groundCheckDistance + skinWidth);

        float bestUpDot = -1f;

        for (int i = 0; i < hitCount; i++)
        {
            Vector2 normal = moveHits[i].normal;
            float angle = Vector2.Angle(normal, Vector2.up);

            if (angle <= maxGroundAngle)
            {
                float upDot = Vector2.Dot(normal, Vector2.up);
                if (upDot > bestUpDot)
                {
                    bestUpDot = upDot;
                    isGrounded = true;
                    groundNormal = normal;
                }
            }
        }

        if (isGrounded && velocity.y < 0f)
            velocity.y = 0f;

        if (isGrounded)
            jumpsRemaining = extraJumps;
    }

    private void ApplyHorizontal(float dt)
    {
        float targetSpeed = moveInput.x * maxRunSpeed;

        float accel;
        if (Mathf.Abs(targetSpeed) > 0.01f)
            accel = isGrounded ? groundAcceleration : airAcceleration;
        else
            accel = isGrounded ? groundDeceleration : airDeceleration;

        velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, accel * dt);

        if (isGrounded)
        {
            Vector2 tangent = new Vector2(groundNormal.y, -groundNormal.x).normalized;
            float tangentSpeed = Vector2.Dot(velocity, tangent);
            Vector2 projected = tangent * tangentSpeed;

            velocity.x = projected.x;
            if (velocity.y <= 0f)
                velocity.y = projected.y;
        }
    }

    private void ApplyGravity(float dt)
    {
        if (isGrounded && velocity.y <= 0f)
            return;

        float gravityScale = 1f;

        if (velocity.y < 0f)
            gravityScale = fallGravityMultiplier;
        else if (velocity.y > 0f && !jumpHeld)
            gravityScale = lowJumpGravityMultiplier;

        velocity.y -= gravity * gravityScale * dt;
    }

    private void TryConsumeJump()
    {
        if (jumpBufferTimer <= 0f)
            return;

        bool canGroundJump = isGrounded || coyoteTimer > 0f;
        bool canDoubleJump = !canGroundJump && jumpsRemaining > 0;

        if (!canGroundJump && !canDoubleJump)
            return;

        if (canDoubleJump)
            jumpsRemaining--;

        isGrounded = false;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        velocity.y = jumpSpeed;

        anim.SetTrigger("jump");
    }

    private void MoveHorizontally(ref Vector2 delta)
    {
        if (Mathf.Abs(delta.x) < 0.0001f)
            return;

        Vector2 dir = new Vector2(Mathf.Sign(delta.x), 0f);
        float distance = Mathf.Abs(delta.x) + skinWidth;

        if (TryStepUp(dir, distance))
            return;

        int hitCount = col.Cast(dir, moveFilter, moveHits, distance);
        if (hitCount == 0)
            return;

        float allowed = Mathf.Abs(delta.x);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = moveHits[i];
            float angle = Vector2.Angle(hit.normal, Vector2.up);

            if (angle > maxGroundAngle)
            {
                float move = Mathf.Max(hit.distance - skinWidth, 0f);
                allowed = Mathf.Min(allowed, move);
            }
        }

        if (allowed < Mathf.Abs(delta.x))
            velocity.x = 0f;

        delta.x = Mathf.Sign(delta.x) * allowed;
    }

    private void MoveVertically(ref Vector2 delta)
    {
        if (Mathf.Abs(delta.y) < 0.0001f)
            return;

        Vector2 dir = new Vector2(0f, Mathf.Sign(delta.y));
        float distance = Mathf.Abs(delta.y) + skinWidth;

        int hitCount = col.Cast(dir, moveFilter, moveHits, distance);
        if (hitCount == 0)
            return;

        float allowed = Mathf.Abs(delta.y);

        for (int i = 0; i < hitCount; i++)
        {
            float move = Mathf.Max(moveHits[i].distance - skinWidth, 0f);
            allowed = Mathf.Min(allowed, move);
        }

        if (delta.y > 0f && allowed < Mathf.Abs(delta.y))
            velocity.y = 0f;

        if (delta.y < 0f && allowed < Mathf.Abs(delta.y))
        {
            velocity.y = 0f;
            isGrounded = true;
        }

        delta.y = Mathf.Sign(delta.y) * allowed;
    }

    private void SnapToGround()
    {
        if (velocity.y > 0f) return;
        if (isGrounded) return;

        int hitCount = col.Cast(Vector2.down, moveFilter, moveHits, groundSnapDistance + skinWidth);
        if (hitCount == 0) return;

        float bestDistance = float.MaxValue;
        Vector2 bestNormal = Vector2.up;
        bool found = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = moveHits[i];
            float angle = Vector2.Angle(hit.normal, Vector2.up);

            if (angle <= maxGroundAngle && hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                bestNormal = hit.normal;
                found = true;
            }
        }

        if (!found) return;

        float snapAmount = Mathf.Max(bestDistance - skinWidth, 0f);
        if (snapAmount <= groundSnapDistance)
        {
            rb.MovePosition(rb.position + Vector2.down * snapAmount);
            isGrounded = true;
            groundNormal = bestNormal;
            velocity.y = 0f;
        }
    }
    // private void OnDrawGizmosSelected()
    // {
    //     if (col == null) return;

    //     float direction = 1f;
    //     if (Application.isPlaying && Mathf.Abs(moveInput.x) > 0.01f)
    //         direction = Mathf.Sign(moveInput.x);

    //     Bounds bounds = col.bounds;

    //     Vector2 lowerCheckPos = new Vector2(
    //         bounds.center.x + direction * (bounds.extents.x + stepCheckDistance),
    //         bounds.min.y + 0.05f
    //     );

    //     Vector2 upperCheckPos = lowerCheckPos + Vector2.up * stepHeight;

    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireCube(lowerCheckPos, stepLowerCheckSize);

    //     Gizmos.color = Color.green;
    //     Gizmos.DrawWireCube(upperCheckPos, stepUpperCheckSize);
    // }

}
