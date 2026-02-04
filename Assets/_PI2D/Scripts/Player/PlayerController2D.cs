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
    // 1. SANEAMIENTO DE DATOS: Usamos una copia local. 
    // Jamás modifiques 'movement' aquí, o corromperás la física en FixedUpdate.
    Vector2 input = movement; 
    
    // ---------------------------------------------------------
    // CASO A: Movimiento Vertical Puro (Prioridad visual)
    // ---------------------------------------------------------
    if (Mathf.Abs(input.y) > 0.1f && Mathf.Abs(input.x) < 0.01f)
    {
        // Apagamos el Animator para tener control manual del SpriteRenderer
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
    // ---------------------------------------------------------
    // CASO B: Movimiento Lateral (O Diagonal)
    // ---------------------------------------------------------
    else if (Mathf.Abs(input.x) > 0.01f)
    {
        currentDirection = Direction.Side;

        if (anim != null)
        {
            // Si el animator estaba apagado, lo encendemos y forzamos su actualización
            if (!anim.enabled) 
            {
                anim.enabled = true;
                // CRÍTICO: Esto fuerza a Unity a evaluar la lógica AHORA MISMO,
                // evitando que espere al siguiente frame para aplicar el booleano.
                anim.Update(0f); 
            }
            anim.SetBool("isMoving", true);
        }

        // Lógica de Flip (Escala) manteniendo proporciones
        Vector3 currentScale = transform.localScale;
        float direction = Mathf.Sign(input.x); // Devuelve 1 o -1
        transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * direction, currentScale.y, currentScale.z);
    }
    // ---------------------------------------------------------
    // CASO C: Idle (Quieto)
    // ---------------------------------------------------------
    else 
    {
        // Solo volvemos al Animator si estábamos mirando de lado.
        // Si estábamos mirando arriba/abajo, mantenemos el último sprite estático.
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
        
