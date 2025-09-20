using UnityEngine;

[CreateAssetMenu(fileName = "NuevoBloque", menuName = "Bloques/BlockType")]
public class BlockType : ScriptableObject
{
    [Tooltip("ID estilo Minecraft 1.13+ (ej: minecraft:grass_block)")]
    public string id;

    [Tooltip("Material por cara (orden: Front, Back, Left, Right, Top, Bottom)")]
    public Material[] faceMaterials = new Material[6];

    [Tooltip("¿Este bloque emite luz, tiene física, etc.?")]
    public bool tieneFisica = false;

    [Tooltip("Componente opcional (ej: script de comportamiento especial)")]
    public GameObject comportamientoOpcionalPrefab;
    [Tooltip("Sonido Del Bloque")]
    public AudioClip[] sonidosModificar; // Reproduce El Mismo Sonido Al Colocar Y Romper
    [Tooltip("Icono")]
    public Sprite iconoInventario;
}
