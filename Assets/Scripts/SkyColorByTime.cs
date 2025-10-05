using UnityEngine;

public class SkyColorByTime : MonoBehaviour
{
    public Material skyMaterial;

    [Header("Simular hora (0–23). Usa -1 para hora real")]
    public int horaSimulada = -1;

    void Update()
    {
        int hora = (horaSimulada >= 0) ? horaSimulada : System.DateTime.Now.Hour;

        Color colorA, colorB;
        float oscurecer = 0f;
        bool usarGris = false;

        if (hora >= 6 && hora < 17)
        {
            colorA = new Color32(0x19, 0x7B, 0xFF, 255); // Día
            colorB = new Color32(0x47, 0x95, 0xFF, 255);
        }
        else if (hora >= 17 && hora < 20)
        {
            colorA = new Color32(0x19, 0x7B, 0xFF, 255); // Atardecer
            colorB = new Color32(0xFF, 0xB6, 0x58, 255);
            usarGris = true;
            oscurecer = Mathf.InverseLerp(17f, 20f, hora);
        }
        else
        {
            colorA = Color.black; // Noche
            colorB = Color.black;
            oscurecer = 1f;
        }

        skyMaterial.SetColor("_ColorA", colorA);
        skyMaterial.SetColor("_ColorB", colorB);
        skyMaterial.SetFloat("_UseGray", usarGris ? 1f : 0f);
        skyMaterial.SetFloat("_Darkness", oscurecer);
    }
}
