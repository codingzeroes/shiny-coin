using UnityEngine;

public class PlayerProjectile: MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxDuration = 3f;
    private float lifetime;
    private Vector2 moveDir;
    private bool hit;

    private BoxCollider2D col;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 pushSum;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        pushSum = Vector2.zero;
    }
    

    private void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime > maxDuration) gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (hit) {rb.linearVelocity = Vector2.zero; return;}
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        hit = true;
        col.enabled = false;
        MakeKinematic();

        // TODO: Animation, damage, collision effects
    }
    private void MakeKinematic()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
    public void SetDirection(Vector2 _direction, Vector2 _playerVelocity)
    {
        if (col == null) col = GetComponent<BoxCollider2D>();
        lifetime = 0;
        hit = false;
        col.enabled = true;
        moveDir = _direction.normalized;

        
        gameObject.SetActive(true);

        rb.linearVelocity = moveDir * speed + _playerVelocity;
        transform.right = _direction;
    }

    // Takes in a normalized push direction, and a pushPower. Applies impulse force to projectile
    public void PushAway(Vector2 _pushDir, float _pushPower)
    {
        pushSum = _pushDir * _pushPower;
        rb.linearVelocity += pushSum;
    }
    public bool isHit() => hit;
    private void Deactivate() => gameObject.SetActive(false);
}
