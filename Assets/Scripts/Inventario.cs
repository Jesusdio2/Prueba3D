using UnityEngine;

[System.Serializable]
public class Inventario // In English: Inventory. // Not Used For Now.
{
    [Tooltip("Cantidad por tipo de bloque (indexado desde 1)")]
    public int[] cantidades;

    // Constructor: inicializa el inventario con N tipos de bloque
    public Inventario(int totalTipos)
    {
        cantidades = new int[totalTipos];
    }

    // Agrega cantidad al tipo de bloque especificado
    public void Agregar(int tipo, int cantidad = 1)
    {
        if (tipo <= 0 || tipo > cantidades.Length) return;
        cantidades[tipo - 1] += cantidad;
    }

    // Intenta consumir cantidad del tipo de bloque especificado
    public bool Consumir(int tipo, int cantidad = 1)
    {
        if (tipo <= 0 || tipo > cantidades.Length) return false;
        if (cantidades[tipo - 1] < cantidad) return false;

        cantidades[tipo - 1] -= cantidad;
        return true;
    }

    // Devuelve la cantidad actual del tipo de bloque especificado
    public int ObtenerCantidad(int tipo)
    {
        if (tipo <= 0 || tipo > cantidades.Length) return 0;
        return cantidades[tipo - 1];
    }
}
