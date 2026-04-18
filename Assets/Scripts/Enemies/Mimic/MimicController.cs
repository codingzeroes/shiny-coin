using System.Security.AccessControl;
using UnityEngine;

public class MimicController : MonoBehaviour
{
    
    [Header ("References")]
    [SerializeField] LayerMask awakenLayerMask;
    private Rigidbody2D rb;
    private Animator anim;
    
    [Header ("Behavior Parameters")]
    [SerializeField] private float searchDuration;
    [SerializeField] private float searchRadius;

    [Header ("Combat Parameters")]
    [SerializeField] private int attackDamage;
    [SerializeField] private bool isHiding;
    [SerializeField] private float searchTimer;    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        isHiding = true; // Mimic starts in hiding state
        anim.SetBool("hidden", true);
    }
    
    void Update()
    {
        if (!isHiding)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer > searchDuration)
            {
                
                Debug.Log($"Mimic is now hiding.");
                isHiding = true;
                anim.SetBool("hidden", true);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Mimic collided with {collision.gameObject.name}, layer {collision.gameObject.layer}");
        // CHECKS FOR CONTAINS LAYER 
        // - this bitmask comparison should check if our collision gameObject's layer is in our layerMask set 
        if (isHiding && ((awakenLayerMask.value & (1 << collision.gameObject.layer)) != 0))
        {
            ActivateMimic();
        }
        else
        {
            GameObject obj = collision.gameObject; 
            if (obj.tag == "Player")
            {
                obj.GetComponent<Health>().TakeDamage(attackDamage);
                GetComponent<MimicHealth>().TakeDamage(attackDamage);
            }
        }
    }

    void ActivateMimic()
    {
        Debug.Log($"Mimic has been Activated.");
        // TODO: play animation of mimic revealing itself
        isHiding = false;
        anim.SetBool("hidden", false);
        searchTimer = 0f;
        return;
    }
}
