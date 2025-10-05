using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Configuración")]
    public Transform playerBody;
    public bool bloquearCamara = false; // Se puede activar desde otro script
    private float xRotation = 0f;
    public PlayerInput playerInput;
    private InputAction lookAction;    
    public float mouseSensitivity = 100f;
    public float joystickSensitivity = 100f;
    public bool Debug1 = false;
    public GameObject ScrollBarNoFunciona;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        lookAction = playerInput.actions["Look"];
    }

    void Update()
    {
        if (Debug1 == true)
        {
            ScrollBarNoFunciona.SetActive(true);
        }
        if (!gameObject.activeInHierarchy || bloquearCamara) return;

        bool bloquearPorMenu = SceneGUIController.instance != null && SceneGUIController.instance.MenuIsActive;
        bool bloquearPorChat = SceneGUIController.instance != null && SceneGUIController.instance.panelChat.activeSelf;

        if (bloquearPorMenu || bloquearPorChat)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        float sensibilidadX = mouseSensitivity;
        float sensibilidadY = mouseSensitivity;

        // Leer entrada de la acción "Look" (mouse + stick derecho)
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        // Detectar si el dispositivo activo es Gamepad
        if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
        {
            sensibilidadX = joystickSensitivity;
            sensibilidadY = joystickSensitivity;
        }

        float lookX = lookInput.x * sensibilidadX * Time.deltaTime;
        float lookY = lookInput.y * sensibilidadY * Time.deltaTime;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * lookX);
    }
}
