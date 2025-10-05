using UnityEngine;

public class SeccionSettingsGUI : MonoBehaviour
{
    public GameObject contenidoPerfil;
    public GameObject contenidoVideo;

    public void MostrarSeccion(string seccion)
    {
        contenidoVideo.SetActive(seccion == "Video");
        contenidoPerfil.SetActive(seccion == "Perfil");
    }
}
