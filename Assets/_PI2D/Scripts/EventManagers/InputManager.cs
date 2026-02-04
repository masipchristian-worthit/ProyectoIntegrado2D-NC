using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Generado por Unity (Save Asset)
    private PlayerInputActions controls;

    // Stack para recordar menús anteriores (LIFO)
    private Stack<InputActionMap> mapHistory = new Stack<InputActionMap>();

    public enum InputMapType
    {
        Gameplay,
        UI
    }

    [Header("Debug Info")]
    [SerializeField] private string currentMapName;

    // Variable para almacenar el input de navegación (solicitado)
    private Vector2 navigationInput;

    [Header("UI System")]
    [SerializeField] private InputSystemUIInputModule uiInputModule;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Inicializamos la clase generada C#
            controls = new PlayerInputActions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Validación clínica: Asegurar que el módulo de UI existe
        if (uiInputModule == null)
        {
            // Intentamos buscarlo si no se asignó en inspector
            uiInputModule = FindFirstObjectByType<InputSystemUIInputModule>();
        }

        // Arrancamos en Gameplay
        SwitchTo(InputMapType.Gameplay);
    }

    // -----------------------------------------------------------------------
    // LÓGICA DE CAMBIO DE MAPAS
    // -----------------------------------------------------------------------
    public void SwitchTo(InputMapType type)
    {
        InputActionMap mapToEnable = GetMapFromEnum(type);
        if (mapToEnable == null) return;

        // 1. Desactivar mapa actual (si existe)
        if (mapHistory.Count > 0)
        {
            var activeMap = mapHistory.Peek();
            activeMap.Disable();

            // Si salimos de UI, desuscribimos sus eventos específicos
            if (activeMap.name == "UI") UnsubscribeUIEvents();
        }

        // 2. Activar nuevo mapa
        mapHistory.Push(mapToEnable);
        mapToEnable.Enable();

        // 3. Configuración específica según el mapa
        if (mapToEnable.name == "UI")
        {
            SubscribeUIEvents();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (mapToEnable.name == "Gameplay")
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        currentMapName = mapToEnable.name;
        // Debug.Log($"Switched to: {currentMapName}");
    }

    public void ReturnToPreviousMap()
    {
        // Si solo queda 1 mapa (Gameplay base), no hacemos pop
        if (mapHistory.Count <= 1) return;

        // 1. Sacamos el mapa actual (UI)
        var currentMap = mapHistory.Pop();
        currentMap.Disable();
        if (currentMap.name == "UI") UnsubscribeUIEvents();

        // 2. Reactivamos el anterior (Gameplay)
        var previousMap = mapHistory.Peek();
        previousMap.Enable();

        // Restaurar estado del cursor según el mapa anterior
        bool isUI = previousMap.name == "UI";
        Cursor.visible = isUI;
        Cursor.lockState = isUI ? CursorLockMode.None : CursorLockMode.Locked;

        currentMapName = previousMap.name;
    }

    // -----------------------------------------------------------------------
    // CALLBACKS ESPECÍFICOS (Solicitado)
    // -----------------------------------------------------------------------

    private void SubscribeUIEvents()
    {
        // Buscamos las acciones por el nombre exacto de tu imagen
        controls.UI.Navigate.performed += OnNavigate;
        controls.UI.Navigate.canceled += OnNavigate;

        controls.UI.Escape.performed += OnEscape;
    }

    private void UnsubscribeUIEvents()
    {
        controls.UI.Navigate.performed -= OnNavigate;
        controls.UI.Navigate.canceled -= OnNavigate;

        controls.UI.Escape.performed -= OnEscape;
    }

    // Callback 1: Navegación (La estructura que pediste)
    public void OnNavigate(InputAction.CallbackContext context)
    {
        navigationInput = context.ReadValue<Vector2>();
        // Nota: Unity EventSystem usa esto automáticamente para seleccionar botones,
        // pero aquí guardamos el valor por si quieres hacer lógica custom.
    }

    // Callback 2: Salir (Escape)
    public void OnEscape(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ReturnToPreviousMap();
        }
    }

    // -----------------------------------------------------------------------
    // UTILIDADES
    // -----------------------------------------------------------------------
    private InputActionMap GetMapFromEnum(InputMapType type)
    {
        switch (type)
        {
            case InputMapType.Gameplay: return controls.Gameplay.Get();
            case InputMapType.UI: return controls.UI.Get();
            default: return null;
        }
    }

    private void OnDisable()
    {
        if (controls != null) controls.Disable();
    }
}