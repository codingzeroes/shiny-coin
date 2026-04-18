using UnityEngine;
using UnityEngine.InputSystem;
using System;

/* Handles all player input actions */
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private SpriteRenderer sprite;

    private Vector2 moveInput;
    private Vector2 pushSum;

    [Header("Input References")]
    [SerializeField] private PlayerControls controls;
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference jump;    
    [SerializeField] private InputActionReference push;    
    [SerializeField] private InputActionReference attack;
    
    
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float pushPower;
    [SerializeField] private float frictionCoefficient = 0.9f; // Friction applied when no input or changing direction

    [Header("Attack Parameters")]
    [SerializeField] private float attackCooldown = 0.5f; 
    [SerializeField] private Transform firePoint;
    private ProjectilePool pool;
    private float cooldownTimer = Mathf.Infinity;


    //CHECK THESE??? ---------------
    
    //private PlayerMovement playerMovement;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        pool = GetComponent<ProjectilePool>();
        //playerMovement = GetComponent<PlayerMovement>();

    }

    
    private void OnEnable()
    {
        GameInput.Instance.JumpPressed += Jump;
        GameInput.Instance.JumpReleased += ReleaseJump;
        GameInput.Instance.PushPressed += Push;
        GameInput.Instance.PushReleased += ReleasePush;
        GameInput.Instance.AttackPressed += TryAttack;

    }
    private void OnDisable()
    {
        if (GameInput.Instance == null) return;
        GameInput.Instance.JumpPressed -= Jump;
        GameInput.Instance.JumpReleased -= ReleaseJump;
        GameInput.Instance.PushPressed -= Push;
        GameInput.Instance.PushReleased -= ReleasePush;
        GameInput.Instance.AttackPressed -= TryAttack;
    }
    

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        moveInput = move.action.ReadValue<Vector2>();

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
        if (Math.Abs(moveInput.x * walkSpeed) < Math.Abs(body.linearVelocity.x))
            xSpeed = body.linearVelocity.x * frictionCoefficient; // Apply friction when no input or changing direction

        body.linearVelocity = new Vector2(xSpeed, body.linearVelocity.y);

    }

    private void TryAttack()
    {
        if (cooldownTimer <= attackCooldown) return;
        if (!CanAttack()) return;

        Attack();
    }
    private void Attack()
    {
        cooldownTimer = 0f;

        GameObject nextProjectile = FindProjectile().gameObject;
        if (nextProjectile == null) {Debug.LogError("No projectile found"); return;}

        nextProjectile.transform.position = firePoint.position;

        // use aim - default to facing direction if no input
        Vector2 aim = GameInput.Instance.Move;
        if (aim.sqrMagnitude < 0.001f)
            aim = new Vector2(Mathf.Sign(transform.localScale.x), 0f);

        nextProjectile.GetComponent<PlayerProjectile>().SetDirection(aim, GetCurrentVelocity());
    }

    private PlayerProjectile FindProjectile()
    {
        return pool.GetNextInactive();
    }

    public void Jump()
    {
        anim.SetTrigger("jump");
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
    }

    private void ReleaseJump()
    {
        if (body.linearVelocity.y > 0f)
            body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y / 2f);
    }

    // Push() will repel all of the Projectiles that have yet to hit something,
    // and TODO: ---------   will repel the player from them depending on weight difference. 
    // Coins have weight 0 - i.e. they will not affect player motion unless they hit something.
    
    private void Push()
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
        body.linearVelocity += pushSum;
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

    public Vector2 GetCurrentVelocity() => body.linearVelocity;
}
