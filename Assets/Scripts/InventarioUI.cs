using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventarioUI : MonoBehaviour
{
    public GameObject prefabSlot; // Prefab con Image + Text
    public Transform contenedorSlots; // Donde se instancian los slots
    public BlockType[] bloquesDisponibles; // Referencia a los bloques
    public Inventario inventario; // Referencia lógica

    void Start()
    {
        GenerarUI();
    }

    public void GenerarUI()
    {
        for (int i = 0; i < bloquesDisponibles.Length; i++)
        {
            GameObject slot = Instantiate(prefabSlot, contenedorSlots);
            Image icono = slot.transform.Find("Icono").GetComponent<Image>();
            TextMeshProUGUI texto = slot.transform.Find("Cantidad").GetComponent<TextMeshProUGUI>();

            icono.sprite = bloquesDisponibles[i].iconoInventario;
            texto.text = inventario.ObtenerCantidad(i + 1).ToString();
        }
    }

    public void ActualizarUI()
    {
        for (int i = 0; i < contenedorSlots.childCount; i++)
        {
            Transform slot = contenedorSlots.GetChild(i);
            TextMeshProUGUI texto = slot.Find("Cantidad").GetComponent<TextMeshProUGUI>();
            texto.text = inventario.ObtenerCantidad(i + 1).ToString();
        }
    }
}
