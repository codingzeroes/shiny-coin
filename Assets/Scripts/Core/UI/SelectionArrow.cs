using UnityEngine;
using UnityEngine.UI;


public class SelectionArrow : MonoBehaviour
{
    [SerializeField] private RectTransform[] options;
    // [SerializeField] private AudioClip changeSound;    // sound played when player changes selection
    // [SerializeField] private AudioClip interactSound;  // sound played when player selects an option
    [SerializeField] private float inputCooldown = 0.2f;

    private float cooldownTimer;
    private RectTransform rect;
    private int currentIndex;

    private void Awake()
    {
        currentIndex = 0;
        rect = GetComponent<RectTransform>();
        UpdateArrowPos();
    }
    private void Update()
    {
        if (GameInput.Instance == null) return;

        cooldownTimer -= Time.unscaledDeltaTime;
        float moveY = GameInput.Instance.Move.y;

        if (cooldownTimer <= 0f)
        {
            if (moveY > 0f)
            {
                ChangePosition(-1); // up
            }
            else if (moveY < 0f)
            {
                ChangePosition(1); // down
            }
        }
    }

    private void OnEnable()
    {
        GameInput.Instance.JumpPressed += Interact;
    }
    private void OnDisable()
    {
        GameInput.Instance.JumpPressed -= Interact;
    }

    private void ChangePosition(int direction)
    {
        currentIndex += direction;

        if(currentIndex < 0)
            currentIndex = options.Length - 1;
        else if(currentIndex >= options.Length)
            currentIndex = 0;   
        
        UpdateArrowPos();

        // SoundManager.instance.PlaySound(changeSound);

        cooldownTimer = inputCooldown;
    }

    private void UpdateArrowPos()
    {
        rect.anchoredPosition = new Vector2(
            rect.anchoredPosition.x,
            options[currentIndex].anchoredPosition.y
        );
    }

    private void Interact()
    {
        // SoundManager.instance.PlaySound(interactSound);

        //Access the button component on each option and call its function
        options[currentIndex].GetComponent<Button>().onClick.Invoke();
    }
}
