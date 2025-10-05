using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // El texto que muestra los FPS
    public TextMeshProUGUI textoBotonFPS; // El texto dentro del botón
    public bool mostrarFPS = false;

    private float tiempoAcumulado = 0f;
    private int framesContados = 0;

    void Update()
    {
        if (!mostrarFPS) return;

        tiempoAcumulado += Time.unscaledDeltaTime;
        framesContados++;

        if (tiempoAcumulado >= 1f)
        {
            int fps = Mathf.RoundToInt(framesContados / tiempoAcumulado);
            fpsText.text = $"FPS: {fps}";
            tiempoAcumulado = 0f;
            framesContados = 0;
        }
    }

    public void ToggleFPS()
    {
        mostrarFPS = !mostrarFPS;
        fpsText.gameObject.SetActive(mostrarFPS);

        textoBotonFPS.text = mostrarFPS ? "Ocultar FPS" : "Mostrar FPS";
    }
}
