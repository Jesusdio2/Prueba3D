using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using TMPro;
using System.IO;
using Unity.Netcode;
using System.Threading;

public class MainMenuController : MonoBehaviour
{
    [Header("Menú de carga")]
    public GameObject MenuCarga;
    public GameObject CirculoCarga;
    public Vector3 posicionTerreno = new Vector3(50f, 0f, -30f);
    public Vector3 scale1 = new Vector3(1f, 1f, 1f);
    public Vector3 scale2 = new Vector3(1.2f, 1.2f, 1.2f);
    public float duration = 0.5f;
    public Transform SplashTextTransform;
    public TextMeshProUGUI textoCarga;
    public GameObject OptionsMenu;
    public GameObject LogoPrueba3D;
    public GameObject MainMenu;
    public Slider sliderAntialising;
    public TextMeshProUGUI textoAntialiasing;
    public TMP_InputField campoNombreJugador;
    public TextMeshProUGUI SplashText;
    public TextAsset archivoSplashText;

    [Header("Materiales")]
    public Material[] grassFaceMaterials = new Material[6];
    public Material[] dirtFaceMaterials = new Material[6];
    public Material[] realmiteFaceMaterials = new Material[6];

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
            SolicitarPermisos();

        MenuCarga.SetActive(true);
        CirculoCarga.SetActive(true);
        StartCoroutine(CargarMenu());
        LeerOpcionesGuardadas();
        OptionsMenu.SetActive(false);
        MainMenu.SetActive(true);
        MostrarSplashTextDesdeCSV();
        campoNombreJugador.onEndEdit.AddListener(GuardarNombreDesdeTexto);
        sliderAntialising.onValueChanged.AddListener(CambiarAntialiasing);
        sliderAntialising.value = QualitySettings.antiAliasing;
        string nombreActual = ObtenerNombreJugador();
        campoNombreJugador.text = nombreActual;
    }

    void Update()
    {
        if (CirculoCarga.activeSelf)
            CirculoCarga.transform.Rotate(Vector3.forward * 160f * Time.deltaTime);

        if (SplashTextTransform == null) return;

        float t = Mathf.PingPong(Time.time, duration) / duration;
        SplashTextTransform.localScale = Vector3.Lerp(scale1, scale2, t);
    }

    public void ToggleOpcions()
    {
        bool estado = !OptionsMenu.activeSelf;
        OptionsMenu.SetActive(estado);
        LogoPrueba3D.SetActive(!estado);
        MainMenu.SetActive(!estado);
    }

    public void CambiarAntialiasing(float valor)
    {
        int nivelAA = Mathf.RoundToInt(valor);

        // Solo se permiten valores: 0, 2, 4, 8
        if (nivelAA == 0 || nivelAA == 2 || nivelAA == 4 || nivelAA == 8)
        {
            QualitySettings.antiAliasing = nivelAA;
            Debug.Log("Antialiasing cambiado a: " + nivelAA);
        }

        string rutaArchivo = ObtenerRutaOpciones();
        string nombreJugador = ObtenerNombreJugador();
        string contenido = $"Antialiasing: {nivelAA}\nPlayerName: {nombreJugador}";
        File.WriteAllText(rutaArchivo, contenido);

        if (textoAntialiasing != null)
            textoAntialiasing.text = $"Antialiasing: {nivelAA}";
    }

    void LeerOpcionesGuardadas()
    {
        string rutaArchivo = ObtenerRutaOpciones();

        if (File.Exists(rutaArchivo))
        {
            string linea = File.ReadAllLines(rutaArchivo)[0];

            string[] partes = linea.Split(':');
            if (partes.Length == 2)
            {
                int valorGuardado;
                if (int.TryParse(partes[1].Trim(), out valorGuardado))
                {
                    QualitySettings.antiAliasing = valorGuardado;
                    sliderAntialising.value = valorGuardado;
                    textoAntialiasing.text = $"Antialiasing: {valorGuardado}";
                }
            }
        }
    }

    public static string ObtenerRutaOpciones()
    {
        string rutaArchivo;

        if (Application.platform == RuntimePlatform.Android)
        {
            rutaArchivo = "/storage/emulated/0/games/com/faes/Prueba3D/Options.txt";
            string carpeta = Path.GetDirectoryName(rutaArchivo);
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);
        }
        else
        {
            rutaArchivo = Path.Combine(Application.persistentDataPath, "Options.txt");
        }

        return rutaArchivo;
    }

    void SolicitarPermisos()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
    }

    public static string ObtenerNombreJugador()
    {
        string ruta = ObtenerRutaOpciones();
        if (!File.Exists(ruta))
            return "Player";

        string[] lineas = File.ReadAllLines(ruta);
        foreach (string linea in lineas)
        {
            if (linea.StartsWith("PlayerName:"))
            {
                string[] partes = linea.Split(':');
                if (partes.Length == 2)
                    return partes[1].Trim();
            }
        }
        return "Player";
    }

    public static void GuardarNombreJugador(string nuevoNombre)
    {
        string ruta = ObtenerRutaOpciones();
        List<string> lineas = new List<string>();

        if (File.Exists(ruta))
            lineas.AddRange(File.ReadAllLines(ruta));

        bool reemplazado = false;
        for (int i = 0; i < lineas.Count; i++)
        {
            if (lineas[i].StartsWith("PlayerName:"))
            {
                lineas[i] = $"PlayerName: {nuevoNombre}";
                reemplazado = true;
                break;
            }
        }

        if (!reemplazado)
            lineas.Add($"PlayerName: {nuevoNombre}");

        File.WriteAllLines(ruta, lineas);
    }

    public void GuardarNombreDesdeUI()
    {
        string nuevoNombre = campoNombreJugador.text;
        GuardarNombreJugador(nuevoNombre);
    }

    void GuardarNombreDesdeTexto(string texto)
    {
        GuardarNombreJugador(texto);
    }

    void MostrarSplashTextDesdeCSV()
    {
        if (archivoSplashText == null)
        {
            SplashText.text = "¡Bienvenido!";
            return;
        }

        Dictionary<string, string> textos = new Dictionary<string, string>();
        List<string> idsNumericos = new List<string>();

        string[] lineas = archivoSplashText.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lineas.Length; i++)
        {
            string linea = lineas[i]; // ← aquí defines 'linea'
            string[] partes = linea.Split(',');

            if (partes.Length < 2) continue;

            string id = partes[0].Trim();
            string texto = partes[1].Trim();

            textos[id] = texto;

            if (!id.StartsWith("E"))
                idsNumericos.Add(id);
        }

        string idElegido = "";

        System.DateTime hoy = System.DateTime.Now;
        if (hoy.Month == 9 && hoy.Day == 8 && textos.ContainsKey("E1"))
        {
            idElegido = "E1";
        }
        else
        {
            if (idsNumericos.Count > 0)
            {
                int index = Random.Range(0, idsNumericos.Count);
                idElegido = idsNumericos[index];
            }
        }

        SplashText.text = textos.ContainsKey(idElegido) ? textos[idElegido] : "MissingNo!";
    }

    IEnumerator CargarMenu()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(GenerarTerrenoDeCarga());
        yield return new WaitForSeconds(0.5f);

        MenuCarga.SetActive(false);
        CirculoCarga.SetActive(false);
    }

    IEnumerator GenerarTerrenoDeCarga()
    {
        GameObject chunk = new GameObject("Chunk_MenuCarga");
        chunk.transform.position = posicionTerreno;

        MeshFilter mf = chunk.AddComponent<MeshFilter>();
        MeshRenderer mr = chunk.AddComponent<MeshRenderer>();
        MeshCollider mc = chunk.AddComponent<MeshCollider>();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int>[] submeshTriangles = new List<int>[18];
        for (int i = 0; i < 18; i++)
            submeshTriangles[i] = new List<int>();

        int vertexOffset = 0;
        int sizeX = 16, sizeZ = 16;
        int totalBloques = sizeX * sizeZ;
        int bloquesGenerados = 0;

        ManualResetEvent hilo1Terminado = new ManualResetEvent(false);
        ManualResetEvent hilo2Terminado = new ManualResetEvent(false);

        // Hilo 1: primera mitad
        Thread hilo1 = new Thread(() =>
        {
            for (int x = 0; x < sizeX / 2; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    lock (vertices)
                    {
                        Vector3 blockPos = new Vector3(x, 0, z);
                        int tipo = 1;

                        for (int face = 0; face < 6; face++)
                        {
                            Vector3[] faceVerts = GetFaceVertices(face, blockPos);
                            vertices.AddRange(faceVerts);

                            uvs.Add(new Vector2(0, 0));
                            uvs.Add(new Vector2(1, 0));
                            uvs.Add(new Vector2(1, 1));
                            uvs.Add(new Vector2(0, 1));

                            int submeshIndex = (tipo - 1) * 6 + face;
                            int offset = vertexOffset;

                            submeshTriangles[submeshIndex].Add(offset + 0);
                            submeshTriangles[submeshIndex].Add(offset + 1);
                            submeshTriangles[submeshIndex].Add(offset + 2);
                            submeshTriangles[submeshIndex].Add(offset + 0);
                            submeshTriangles[submeshIndex].Add(offset + 2);
                            submeshTriangles[submeshIndex].Add(offset + 3);

                            vertexOffset += 4;
                        }

                        bloquesGenerados++;
                    }
                }
            }
            hilo1Terminado.Set();
        });

        // Hilo 2: segunda mitad
        Thread hilo2 = new Thread(() =>
        {
            for (int x = sizeX / 2; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    lock (vertices)
                    {
                        Vector3 blockPos = new Vector3(x, 0, z);
                        int tipo = 1;

                        for (int face = 0; face < 6; face++)
                        {
                            Vector3[] faceVerts = GetFaceVertices(face, blockPos);
                            vertices.AddRange(faceVerts);

                            uvs.Add(new Vector2(0, 0));
                            uvs.Add(new Vector2(1, 0));
                            uvs.Add(new Vector2(1, 1));
                            uvs.Add(new Vector2(0, 1));

                            int submeshIndex = (tipo - 1) * 6 + face;
                            int offset = vertexOffset;

                            submeshTriangles[submeshIndex].Add(offset + 0);
                            submeshTriangles[submeshIndex].Add(offset + 1);
                            submeshTriangles[submeshIndex].Add(offset + 2);
                            submeshTriangles[submeshIndex].Add(offset + 0);
                            submeshTriangles[submeshIndex].Add(offset + 2);
                            submeshTriangles[submeshIndex].Add(offset + 3);

                            vertexOffset += 4;
                        }

                        bloquesGenerados++;
                    }
                }
            }
            hilo2Terminado.Set();
        });

        hilo1.Start();
        hilo2.Start();

        // Esperar a que ambos hilos terminen
        while (!hilo1Terminado.WaitOne(0) || !hilo2Terminado.WaitOne(0))
        {
            textoCarga.text = $"{(int)((float)bloquesGenerados / totalBloques * 100)}%";
            yield return null;
        }

        Mesh mesh = new Mesh();
        mesh.subMeshCount = 18;
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        for (int i = 0; i < 18; i++)
            mesh.SetTriangles(submeshTriangles[i], i);
        mesh.RecalculateNormals();

        mf.mesh = mesh;
        mc.sharedMesh = mesh;

        List<Material> allMaterials = new List<Material>();
        allMaterials.AddRange(grassFaceMaterials);
        allMaterials.AddRange(dirtFaceMaterials);
        allMaterials.AddRange(realmiteFaceMaterials);
        mr.materials = allMaterials.ToArray();

        textoCarga.text = "Carga completa";
    }


    Vector3[] GetFaceVertices(int faceIndex, Vector3 offset)
    {
        switch (faceIndex)
        {
            case 0: return new Vector3[] { offset + new Vector3(0, 0, 1), offset + new Vector3(1, 0, 1), offset + new Vector3(1, 1, 1), offset + new Vector3(0, 1, 1) }; // Front
            case 1: return new Vector3[] { offset + new Vector3(1, 0, 0), offset + new Vector3(0, 0, 0), offset + new Vector3(0, 1, 0), offset + new Vector3(1, 1, 0) }; // Back
            case 2: return new Vector3[] { offset + new Vector3(0, 0, 0), offset + new Vector3(0, 0, 1), offset + new Vector3(0, 1, 1), offset + new Vector3(0, 1, 0) }; // Left
            case 3: return new Vector3[] { offset + new Vector3(1, 0, 1), offset + new Vector3(1, 0, 0), offset + new Vector3(1, 1, 0), offset + new Vector3(1, 1, 1) }; // Right
            case 4: return new Vector3[] { offset + new Vector3(0, 1, 1), offset + new Vector3(1, 1, 1), offset + new Vector3(1, 1, 0), offset + new Vector3(0, 1, 0) }; // Top
            case 5: return new Vector3[] { offset + new Vector3(0, 0, 0), offset + new Vector3(1, 0, 0), offset + new Vector3(1, 0, 1), offset + new Vector3(0, 0, 1) }; // Bottom
            default: return new Vector3[4];
        }
    }
}
