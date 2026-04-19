using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrow : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.5f; 
    [SerializeField] private Transform firePoint;
    private ProjectilePool pool;


    private PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity;



    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        pool = GetComponent<ProjectilePool>(); 
    }

    private void OnEnable()
    {
        if (GameInput.Instance == null) return;
        GameInput.Instance.AttackPressed += TryAttack;
    }

    private void OnDisable()
    {
        if (GameInput.Instance == null) return;
        GameInput.Instance.AttackPressed -= TryAttack;
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;
    }
    private void TryAttack()
    {
        if (cooldownTimer <= attackCooldown) return;
        if (!playerMovement.CanAttack()) return;

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
        
        nextProjectile.GetComponent<PlayerProjectile>().SetDirection(aim, playerMovement.GetCurrentVelocity());
    }
    private PlayerProjectile FindProjectile()
    {
        return pool.GetNextInactive();
    }

}
