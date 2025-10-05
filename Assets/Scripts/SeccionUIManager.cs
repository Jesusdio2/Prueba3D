using UnityEngine;

public class SeccionUIManager : MonoBehaviour
{
    public GameObject contenidoJuego;
    public GameObject contenidoVideo;
    public GameObject contenidoPerfil;

    public void MostrarSeccion(string seccion)
    {
        contenidoJuego.SetActive(seccion == "Juego");
        contenidoVideo.SetActive(seccion == "Video");
        contenidoPerfil.SetActive(seccion == "Perfil");
    }
}
