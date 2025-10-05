using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HoraSlider : MonoBehaviour // Only Debug
{
    // References
    public SkyColorByTime skyController;
    public Slider horaSlider;
    public TMP_Text horaTexto;

    void Start()
    {
        horaSlider.minValue = 0;
        horaSlider.maxValue = 23;
        horaSlider.wholeNumbers = true;
        horaSlider.onValueChanged.AddListener(CambiarHora);
        CambiarHora(horaSlider.value);
    }

    void CambiarHora(float valor)
    {
        int hora = Mathf.RoundToInt(valor);
        skyController.horaSimulada = hora;
        if (horaTexto != null)
            horaTexto.text = "Hora simulada: " + hora.ToString("00") + ":00";
    }
}
