using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{

    // Referencias
    public GameObject pauseMenu;
    public GameObject optionsMenu;

    public void CambiaScena(string sceneName) // Cambia La Escena De Una A Otra
    {
        SceneManager.LoadScene(sceneName);
    }

    public void AbrirMenuOpciones() // Abre El Menu De Opciones
    {
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void CerrarMenuOpciones() // Cierra El Menu De Opciones
    {
        pauseMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }

    public void CerrarJuego() // Cierra El Juego
    {
        Debug.Log("Closing the game...");
        Application.Quit();
    }
}
