using TMPro;
using UnityEngine;

public class SceneGUIController : MonoBehaviour
{
    // Referencias
    public static SceneGUIController instance;
    public GameObject optionsMenu;
    public GameObject pauseMenu;
    private bool menuIsActive = false;
    public bool MenuIsActive => menuIsActive; // propiedad pública de solo lectura
    public FPSCounter fpsCounter;
    public TMPro.TextMeshProUGUI botonFPSLabel;
    public VidaController vidaController;
    public GameObject panelChat;
    public bool ChatActivo => panelChat.activeSelf;
    public GameObject Hotbar;
    public GameObject HotbarSelector;
    public GameObject HotbarVistasPrevias;
    public GameObject Hearts;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        vidaController = Object.FindFirstObjectByType<VidaController>();
        optionsMenu.SetActive(false);
        menuIsActive = false;
        panelChat.SetActive(false);
        pauseMenu.SetActive(false);
        Hotbar.SetActive(true);
        HotbarSelector.SetActive(true);
        HotbarVistasPrevias.SetActive(true);
        Hearts.SetActive(true);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (vidaController != null && vidaController.vida <= 0f)
                return;
                
            menuIsActive = !menuIsActive;
            pauseMenu.SetActive(menuIsActive);
            Hotbar.SetActive(!menuIsActive);
            HotbarSelector.SetActive(!menuIsActive);
            HotbarVistasPrevias.SetActive(!menuIsActive);
            Hearts.SetActive(!menuIsActive);

            Time.timeScale = menuIsActive ? 0 : 1;
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (vidaController != null && vidaController.vida <= 0f)
                return;

            bool estadoActual = panelChat.activeSelf;
            panelChat.SetActive(!estadoActual);
        }
    }

    public void BotonReanudar()
    {
        menuIsActive = false;
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        Hotbar.SetActive(!menuIsActive);
        HotbarSelector.SetActive(!menuIsActive);
        HotbarVistasPrevias.SetActive(!menuIsActive);
        Hearts.SetActive(!menuIsActive);

        Time.timeScale = 1;
    }

    public void ToggleMostrarFPS()
    {
        bool nuevoEstado = !fpsCounter.mostrarFPS;
        fpsCounter.mostrarFPS = nuevoEstado;
        fpsCounter.fpsText.gameObject.SetActive(nuevoEstado);

        if (botonFPSLabel != null)
        {
            botonFPSLabel.text = nuevoEstado ? "Ocultar FPS" : "Mostrar FPS";
        }
    }
}
