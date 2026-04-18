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
    
    
    
    
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float pushPower;
    [SerializeField] private float frictionCoefficient = 0.9f; // Friction applied when no input or changing direction

    [Header("Step Up parameters")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float stepHeight = 0.25f;
    [SerializeField] private float stepCheckDistance = 0.04f;
    [SerializeField] private float stepUpAmount = 0.2f;


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
        float xSpeed = moveInput.x * walkSpeed;
        if (Math.Abs(moveInput.x * walkSpeed) < Math.Abs(rb.linearVelocity.x))
            xSpeed = rb.linearVelocity.x * frictionCoefficient; // Apply friction when no input or changing direction

        rb.linearVelocity = new Vector2(xSpeed, rb.linearVelocity.y);
        
        TryStepUp();
    }

    public void OnJump()
    {
        anim.SetTrigger("jump");
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
    }

    private void ReleaseJump()
    {
        if (rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y / 2f);
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
        rb.linearVelocity += pushSum;
    }
    private bool IsGrounded()
    {
        Bounds bounds = col.bounds;

        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y - 0.02f);
        Vector2 boxSize = new Vector2(bounds.size.x * 0.9f, 0.05f);

        return Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer) != null;
    }
    private void TryStepUp()
    {
        if (Mathf.Abs(moveInput.x) < 0.01f) return;
        if (!IsGrounded()) return;
        if (rb.linearVelocity.y > 0.05f) return;

        Vector2 dir = new Vector2(Mathf.Sign(moveInput.x), 0f);

        int hitCount = col.Cast(dir, stepFilter, stepHits, stepCheckDistance);
        if (hitCount == 0) return;

        Vector2 originalPos = rb.position;
        rb.position = originalPos + Vector2.up * stepHeight;

        int raisedHitCount = col.Cast(dir, stepFilter, stepHits, stepCheckDistance);

        rb.position = originalPos;

        if (raisedHitCount == 0)
        {
            Debug.Log("STEP UP TRIGGERED");
            rb.position += Vector2.up * stepUpAmount;
        }
    }

    // private void TryStepUp()
    // {
    //     if (Mathf.Abs(moveInput.x) < 0.01f) return;
    //     if (!IsGrounded()) return;
    //     if (rb.linearVelocity.y > 0.05f) return;

    //     float direction = Mathf.Sign(moveInput.x);
    //     Bounds bounds = col.bounds;

    //     Vector2 lowerCheckPos = new Vector2(
    //         bounds.center.x + direction * (bounds.extents.x + stepCheckDistance),
    //         bounds.min.y + 0.05f
    //     );

    //     Vector2 upperCheckPos = lowerCheckPos + Vector2.up * stepHeight - new Vector2(direction * 0.02f, 0f);

    //     Collider2D lowerHit = Physics2D.OverlapBox(lowerCheckPos, stepLowerCheckSize, 0f, groundLayer);
    //     Collider2D upperHit = Physics2D.OverlapBox(upperCheckPos, stepUpperCheckSize, 0f, groundLayer);
        
    //     Debug.Log("lowerHit:" + (lowerHit == null).ToString());
    //     Debug.Log("upperHit:" + (upperHit == null).ToString());
        
    //     if (lowerHit != null && upperHit == null)
    //     {
    //         Debug.Log("STEP UP TRIGGERED");
    //         rb.position += Vector2.up * stepUpAmount;
    //     }
    // }

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
