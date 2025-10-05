using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Collections;
using Unity.Netcode; // Netcode definitions are still not in use, multiplayer is still under development.
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class ControlJugador : NetworkBehaviour // In English:PlayerControl // 
{
    [Header("Configuración")]
    public float velocidad = 5f;
    public float velocidadSalto = 8f;
    public float gravedad = 20f;
    public TextMeshProUGUI nombreUI;
    public NetworkVariable<FixedString64Bytes> nombreJugador = new NetworkVariable<FixedString64Bytes>();

    private Vector3 movimiento = Vector3.zero;
    private CharacterController controlador;

    [Header("Input System")]
    public PlayerInput playerInput;
    private InputAction movimientoAction;
    private InputAction saltoAction;
    private InputAction pauseAction;
    private InputAction sprintAction;

    [Header("UI")]
    public GameObject pauseMenu;

    public static bool isGamePaused = false;
    private VidaController vidaController;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            string nombre = MainMenuController.ObtenerNombreJugador();
            nombreJugador.Value = new FixedString64Bytes(nombre);
            if (nombreUI != null)
                nombreUI.text = nombre;
        }

        // Suscribirse al cambio de nombre para todos los clientes
        nombreJugador.OnValueChanged += ActualizarNombreUI;
    }

    [ServerRpc]
    public void ActualizarNombreServerRpc(string nuevoNombre)
    {
        nombreJugador.Value = nuevoNombre;
    }

    void Start()
    {
        vidaController = Object.FindFirstObjectByType<VidaController>();
        controlador = GetComponent<CharacterController>();

        // Referenciar acciones del Input System
        movimientoAction = playerInput.actions["Move"];
        saltoAction = playerInput.actions["Jump"];
        pauseAction = playerInput.actions["Pause"];
        sprintAction = playerInput.actions["Sprint"];
    }

    void Update()
    {
        if (vidaController != null && vidaController.vida <= 0f)
            return;
        if (controlador.isGrounded)
        {
            Vector2 inputMovimiento = movimientoAction.ReadValue<Vector2>();
            float moverX = inputMovimiento.x;
            float moverZ = inputMovimiento.y;

            // Detectar si hay movimiento (WASD) y si Ctrl está presionado
            bool estaMoviendo = inputMovimiento.magnitude > 0.1f;
            bool estaCorriendo = sprintAction.IsPressed() && estaMoviendo;

            float velocidadActual = estaCorriendo ? velocidad * 1.8f : velocidad;

            movimiento = new Vector3(moverX, 0, moverZ);
            movimiento = transform.TransformDirection(movimiento);
            movimiento *= velocidadActual;

            if (saltoAction.triggered)
            {
                movimiento.y = velocidadSalto;
            }
        }

        movimiento.y -= gravedad * Time.deltaTime;
        controlador.Move(movimiento * Time.deltaTime);
    }

    public void CambiarNombreDesdeUI(string nuevoNombre)
    {
        nombreJugador.Value = nuevoNombre;
        nombreUI.text = nuevoNombre;
        MainMenuController.GuardarNombreJugador(nuevoNombre);
        ActualizarNombreServerRpc(nuevoNombre);
    }

    private void ActualizarNombreUI(FixedString64Bytes anterior, FixedString64Bytes nuevo)
    {
        if (nombreUI != null)
            nombreUI.text = nuevo.ToString();
    }

    private void OnDisable()
    {
        nombreJugador.OnValueChanged -= ActualizarNombreUI;
    }
}
