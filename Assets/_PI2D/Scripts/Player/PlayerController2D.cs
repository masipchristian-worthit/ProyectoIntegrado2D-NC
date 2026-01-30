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

    [Header("Interaction Colliders")]
    // El collider lateral (original) que se volteará
    public BoxCollider2D InteractColliderSide; 
    // Nuevos colliders para arriba y abajo
    public BoxCollider2D InteractColliderUp;   
    public BoxCollider2D InteractColliderDown; 

    // State Variables
    private bool isInteracting = false;
    private bool isPaused = false;
    
    // Enum interno para controlar la dirección de la mirada
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
        
        // Inicialización de seguridad: Desactivar todos los triggers al inicio
        if (InteractColliderSide) InteractColliderSide.enabled = false;
        if (InteractColliderUp) InteractColliderUp.enabled = false;
        if (InteractColliderDown) InteractColliderDown.enabled = false;
    }

    void Start()
    {
        // Si no asignaste el SpriteRenderer en el inspector, lo buscamos
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!isPaused && !isInteracting)
        {
            HandleDirection();
        }
    }

    void FixedUpdate()
    {
        if (!isPaused && !isInteracting)
        {
            Movement();
        }
    }

    void Movement()
    {
        // MODIFICACIÓN: Multiplicamos por (X, 0) para anular movimiento vertical físico
        Vector2 targetVelocity = new Vector2(movement.x, 0) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + targetVelocity);
    }

    // Nueva función para manejar la lógica visual y de estado
    void HandleDirection()
    {
        // Prioridad al movimiento vertical para el cambio de sprite (como en Zelda/Pokemon)
        if (movement.y > 0.1f && movement.x == 0)
        {
            movement.x = 0; // Aseguramos que no haya movimiento horizontal
            currentDirection = Direction.Up;
            if (spriteUp) spriteRenderer.sprite = spriteUp;
        }
        else if (movement.y < -0.1f && movement.x == 0)
        {
            movement.x = 0; // Aseguramos que no haya movimiento horizontal
            currentDirection = Direction.Down;
            if (spriteDown) spriteRenderer.sprite = spriteDown;
        }
        // Si hay movimiento horizontal
        else if (Mathf.Abs(movement.x) > 0f && movement.y == 0)
        {
            currentDirection = Direction.Side;
            if (spriteSide) spriteRenderer.sprite = spriteSide;

            // Lógica de Flip para Sprite y Collider
            if (movement.x > 0) // Derecha
            {
                transform.localScale = new Vector3(1, 1, 1); // Aseguramos escala positiva
            }
            else // Izquierda
            {
                transform.localScale = new Vector3(-1, 1, 1); // Volteamos horizontalmente
            }
        }
    }

    // COROUTINES
    IEnumerator Interact()
    {
        isInteracting = true;
        
        // Seleccionamos qué collider activar según la dirección actual
        BoxCollider2D currentCollider = InteractColliderSide; // Default

        switch (currentDirection)
        {
            case Direction.Up:
                currentCollider = InteractColliderUp;
                break;
            case Direction.Down:
                currentCollider = InteractColliderDown;
                break;
            case Direction.Side:
                currentCollider = InteractColliderSide;
                break;
        }

        if (currentCollider != null)
        {
            currentCollider.enabled = true;
            // Debug.Log($"Interacting direction: {currentDirection}");
            yield return new WaitForSeconds(0.5f);
            currentCollider.enabled = false;
        }
        else
        {
            // Fallback por si falta asignar algo
            yield return new WaitForSeconds(0.5f);
        }

        isInteracting = false;
    }

    // Player Actions Logic
    void InteractAction()
    {
        if (isInteracting) moveSpeed = 0f;
    }

    //-------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------

    #region Input System Callbacks

    // Gameplay Actions
    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Debug.Log("Interact Pressed");
            StartCoroutine(Interact());
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TogglePause();
            Debug.Log("Pause Pressed");
        }
    }

    // UI Actions

    public void onAccept(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Si hay un diálogo activo, avanzar texto
            if (DialogueManager.Instance != null && DialogueManager.Instance.gameObject.activeInHierarchy)
            {
                DialogueManager.Instance.DisplayNextSentence();
            }
        }
    }

    public void onNavigate(InputAction.CallbackContext context)
    {
        // Vector2 navigationInput = context.ReadValue<Vector2>();
    }

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

    // Lógica de Pausa Manual
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; 
            if(pauseMenuPanel) pauseMenuPanel.SetActive(true);
            GetComponent<PlayerInput>().SwitchCurrentActionMap("UI");
        }
        else
        {
            Time.timeScale = 1f; 
            if(pauseMenuPanel) pauseMenuPanel.SetActive(false);
            GetComponent<PlayerInput>().SwitchCurrentActionMap("Gameplay");
        }
    }
}
        
