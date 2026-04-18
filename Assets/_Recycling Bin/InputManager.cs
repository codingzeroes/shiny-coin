using UnityEngine;

public class InputManager: MonoBehaviour
{
    // This class is no longer used as GameInput now directly manages the PlayerControls instance.
    
    
    
    
    // REDACTED - REMOVE InputManager object
    /*
    public static InputManager Instance { get; private set; }
    public PlayerControls Controls { get; private set; }

    private void Awake()
    {
        Debug.Log("InputManager Awake ran");
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Controls = new PlayerControls();
    }

    private void OnEnable()
    {
        Controls.Player.Enable();
    }

    private void OnDisable()
    {
        Controls.Player.Disable();
    }

    private void OnDestroy()
    {
        if (Instance == this && Controls != null)
        {
            Controls.Player.Disable();
            Controls.Dispose();
            Instance = null;
        }
    }
    */
}
