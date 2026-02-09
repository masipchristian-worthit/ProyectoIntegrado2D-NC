using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] public float moveSpeed = 5f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteSide;
    [SerializeField] private Animator anim;

    [Header("Interaction Colliders")]
    public BoxCollider2D InteractColliderSide;
    public BoxCollider2D InteractColliderUp;
    public BoxCollider2D InteractColliderDown;

    // State Variables
    private bool isInteracting = false;
    private bool isPaused = false;

    private enum Direction { Side, Up, Down }
    private Direction currentDirection = Direction.Side;

    // Private References
    Rigidbody2D rb;
    Vector2 movement;

    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = Vector2.zero;

        if (InteractColliderSide) InteractColliderSide.enabled = false;
        if (InteractColliderUp) InteractColliderUp.enabled = false;
        if (InteractColliderDown) InteractColliderDown.enabled = false;
    }

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // --- CORRECCIÓN 1: Chequeo explícito de Diálogo en Update ---
    void Update()
    {
        // Si hay pausa, interacción O DIÁLOGO, no calculamos dirección visual
        bool dialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;

        if (!isPaused && !isInteracting && !dialogueActive && Time.timeScale > 0)
        {
            HandleDirection();
        }
    }

    // --- CORRECCIÓN 2: Chequeo explícito de Diálogo en FixedUpdate ---
    void FixedUpdate()
    {
        bool dialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;

        if (!isPaused && !isInteracting && !dialogueActive && Time.timeScale > 0)
        {
            Movement();
        }
    }

    void Movement()
    {
        Vector2 targetVelocity = new Vector2(movement.x, 0) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + targetVelocity);
    }

    void HandleDirection()
    {
        Vector2 input = movement;

        if (Mathf.Abs(input.y) > 0.1f && Mathf.Abs(input.x) < 0.01f)
        {
            if (anim != null && anim.enabled) anim.enabled = false;

            if (input.y > 0)
            {
                currentDirection = Direction.Up;
                if (spriteUp) spriteRenderer.sprite = spriteUp;
            }
            else
            {
                currentDirection = Direction.Down;
                if (spriteDown) spriteRenderer.sprite = spriteDown;
            }
        }
        else if (Mathf.Abs(input.x) > 0.01f)
        {
            currentDirection = Direction.Side;

            if (anim != null)
            {
                if (!anim.enabled)
                {
                    anim.enabled = true;
                    anim.Update(0f);
                }
                anim.SetBool("isMoving", true);
            }

            Vector3 currentScale = transform.localScale;
            float direction = Mathf.Sign(input.x);
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * direction, currentScale.y, currentScale.z);
        }
        else
        {
            if (currentDirection == Direction.Side)
            {
                if (anim != null)
                {
                    if (!anim.enabled) anim.enabled = true;
                    anim.SetBool("isMoving", false);
                }
            }
        }
    }

    IEnumerator Interact()
    {
        isInteracting = true;

        BoxCollider2D currentCollider = InteractColliderSide;

        switch (currentDirection)
        {
            case Direction.Up: currentCollider = InteractColliderUp; break;
            case Direction.Down: currentCollider = InteractColliderDown; break;
            case Direction.Side: currentCollider = InteractColliderSide; break;
        }

        if (currentCollider != null)
        {
            currentCollider.enabled = true;
            yield return new WaitForSeconds(0.1f);
            currentCollider.enabled = false;
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }

        isInteracting = false;
    }

    #region Input System Callbacks

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Solo interactuamos si no hay pausa ni diálogo activo
            bool dialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;

            if (!isPaused && !dialogueActive && Time.timeScale > 0)
            {
                StartCoroutine(Interact());
            }
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TogglePause();
        }
    }

    public void onAccept(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                DialogueManager.Instance.DisplayNextSentence();
            }
        }
    }

    public void onNavigate(InputAction.CallbackContext context) { }

    public void onEscape(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (pauseMenuPanel != null && pauseMenuPanel.activeInHierarchy)
            {
                TogglePause();
            }
        }
    }

    #endregion

    // --- CORRECCIÓN 3: LÓGICA DE PRIORIDAD AL DESPAUSAR ---
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // AL PAUSAR: Siempre congelamos todo y vamos a UI
            Time.timeScale = 0f;
            if (pauseMenuPanel) pauseMenuPanel.SetActive(true);

            if (InputManager.Instance != null)
                InputManager.Instance.SwitchTo(InputManager.InputMapType.UI);
        }
        else
        {
            // AL DESPAUSAR: Verificamos si debemos volver al juego O al diálogo
            if (pauseMenuPanel) pauseMenuPanel.SetActive(false);

            bool dialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;

            if (dialogueActive)
            {
                // Si hay diálogo: Mantenemos el juego congelado y el Input en UI
                Time.timeScale = 0f;
                if (InputManager.Instance != null)
                    InputManager.Instance.SwitchTo(InputManager.InputMapType.UI);
            }
            else
            {
                // Si NO hay diálogo: Devolvemos el control al jugador
                Time.timeScale = 1f;
                if (InputManager.Instance != null)
                    InputManager.Instance.SwitchTo(InputManager.InputMapType.Gameplay);
            }
        }
    }
}