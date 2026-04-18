using UnityEngine;

public class FollowCamera: MonoBehaviour
{
    [Header ("References")]
    [SerializeField] private Transform followTarget;

    [Header("Camera Follow Parameters")]
    [SerializeField] private float followSpeed;
    [SerializeField] private float smoothTime;
    [SerializeField] private float lookAheadDistance;
    [SerializeField] private float lookUpDistance;

    [Header("Static Region Parameters")]
    // RELATIVE Bounds - to camera/screen, not world position
    [SerializeField] private float srHorizontalBound;
    [SerializeField] private float srVerticalBound;
    // Width and height below are technically 'half-width' and 'half-height' - redacted for clarity.
    [SerializeField] private float srWidth;
    [SerializeField] private float srHeight;
    
    // Center of the Static Region - world position
    private Vector2 srCenter;
    private Vector3 velocity = Vector3.zero;
    private bool inXBounds;
    private bool inYBounds;
    


    /*
    We define a Static Region (SR) around the player. When the player is within this region, the camera does not move. 

    When the player moves outside of this region:
    1. The SR follows the player.
        The center of SR moves to keep the player within the SR box.
    2. The camera follows the SR.
        If the SR is hitting one of its bounds, the camera moves to keep the player within the SR.
            Visually, the SR does not move here, but it is still physically following the player.
        
    TODO - Camera lookahead function - Shift SR bounds so that the camera looks ahead in the direction of player movement. 
    This should be a simple shift of the SR bounds in the direction of player movement, 
    by a distance equal to the lookahead distance variable.

    */
    private void Start()
    {
        if (followTarget != null)
        {
            srCenter = followTarget.position;
        } 
        else
            Debug.LogError("Follow target not assigned in FollowCamera script.");
    }
    private void LateUpdate()
    {        
        /* Update Static Region Center */
        CheckInStaticRegion();
        if (!inXBounds || !inYBounds)
        {
            float targetxPos = srCenter.x;
            float targetyPos = srCenter.y;

            if (!inXBounds)
            {
                if (followTarget.position.x > srCenter.x) 
                // player is to the right of SR 
                    targetxPos = followTarget.position.x - srWidth;
                else 
                // player is to the left of SR 
                    targetxPos = followTarget.position.x + srWidth;
            }
            if (!inYBounds)
            {
                if (followTarget.position.y > srCenter.y) 
                // player is above SR
                    targetyPos = followTarget.position.y - srHeight;
                else 
                // player is below SR 
                    targetyPos = followTarget.position.y + srHeight;
            }
            srCenter = new Vector2(targetxPos, targetyPos); 
        }
        float targetxPosCam = transform.position.x;
        float targetyPosCam = transform.position.y;
        
        
        /* Update Camera Position */
        if (Mathf.Abs(srCenter.x - transform.position.x) > srHorizontalBound) // check relative position bounding
        {
            // we want to target the camera position that puts the srCenter AT the horizontal bound
            targetxPosCam = srCenter.x - Mathf.Sign(srCenter.x - transform.position.x) * srHorizontalBound;
        }
        if (Mathf.Abs(srCenter.y - transform.position.y) > srVerticalBound)
        {
            targetyPosCam = srCenter.y - Mathf.Sign(srCenter.y - transform.position.y) * srVerticalBound;
        }
        transform.position = Vector3.SmoothDamp(transform.position, 
        new Vector3(targetxPosCam, targetyPosCam, transform.position.z),     
        ref velocity,
        smoothTime);
    }

    /*
    Sets inXBounds and inYBounds based on whether the followTarget is within the SR bounds.
    */
    private void CheckInStaticRegion()
    {
        inXBounds = true;
        inYBounds = true;
        if ((followTarget.position.x > srCenter.x + srWidth) || 
        (followTarget.position.x < srCenter.x - srWidth))
            inXBounds = false; 
        if ((followTarget.position.y > srCenter.y + srHeight) ||
        (followTarget.position.y < srCenter.y - srHeight))
            inYBounds = false;
    }


    private void OnDrawGizmos()
    {
        // Static Region box
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3(srCenter.x, srCenter.y, 0f),
            new Vector3(srWidth * 2f, srHeight * 2f, 0f)
        );

        // Camera bounds box around current camera position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3(transform.position.x, transform.position.y, 0f),
            new Vector3(srHorizontalBound * 2f, srVerticalBound * 2f, 0f)
        );

        // Optional: line from camera center to static region center
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            new Vector3(transform.position.x, transform.position.y, 0f),
            new Vector3(srCenter.x, srCenter.y, 0f)
        );
    }
}
