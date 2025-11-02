using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class ChunksController : MonoBehaviour
{
    [Header("Chunk settings")]
    public int chunkSizeX = 16;
    public int[] distanciasLOD = new int[] { 4, 8, 16 };
    public int maxLOD = 2;
    public int chunkSizeY = 64;
    public int chunkSizeZ = 16;
    public int distanciaRender = 8;      // en chunks
    public int chunksPorFrame = 2;
    private bool juegoActivo = true;

    [Header("Gameplay")]
    public int tipoDeBloqueActivo = 1;   // 1 = grass, 2 = dirt, 3 = realmite ...
    public LayerMask capaBloques;
    public GameObject prefabContornoCara;
    public GameObject prefabParticulaBloque;
    public GameObject prefabEnemigo;
    public Collider jugadorCollider;
    public GameObject OpcionsMenu;
    public GameObject PauseMenu;
    private Transform jugadorTransform;
    private VidaController vidaController;
    private InputAction placeBlockAction;
    private InputAction removeBlockAction;
    public PlayerInput playerInput;
    // Solo infinito
    private readonly Dictionary<Vector2Int, Chunk> chunksInfinitos = new Dictionary<Vector2Int, Chunk>();
    private readonly Queue<Vector2Int> colaChunksPendientes = new Queue<Vector2Int>();
    private readonly HashSet<Vector2Int> enProceso = new HashSet<Vector2Int>();
    private GameObject contornoActual;
    private int semilla;
    public static int debugLod = 0;
    public BlockType[] bloquesDisponibles;
    public AudioSource audioSource;

    void Start()
    {
        placeBlockAction = playerInput.actions["PlaceBlock"];
        removeBlockAction = playerInput.actions["RemoveBlock"];
        vidaController = Object.FindFirstObjectByType<VidaController>();
        jugadorTransform = GameObject.FindWithTag("Player")?.transform;

        if (jugadorTransform == null)
        {
            Debug.LogError("No se encontró el Player. Asegúrate de tener un objeto con tag 'Player'.");
            enabled = false;
            return;
        }

        semilla = Random.Range(0, 1000000);
        Debug.Log("Semilla del mundo: " + semilla);

        // Inicial: encolar los chunks alrededor del jugador
        Vector2Int jugadorChunk = GetChunkCoord(jugadorTransform.position);
        EnqueueAround(jugadorChunk);
    }

    void Update()
    {
        // Pausas/estado
        if (vidaController != null && vidaController.vida <= 0f) return;
        if ((OpcionsMenu != null && OpcionsMenu.activeSelf) || (PauseMenu != null && PauseMenu.activeSelf)) return;
        if (SceneGUIController.instance != null && SceneGUIController.instance.ChatActivo) return;

        // Coordenada de chunk del jugador y encolado incremental
        Vector2Int jugadorChunk = GetChunkCoord(jugadorTransform.position);
        EnqueueAround(jugadorChunk);

        // Procesar N chunks por frame
        int procesados = 0;
        while (colaChunksPendientes.Count > 0 && procesados < chunksPorFrame)
        {
            Vector2Int coord = colaChunksPendientes.Dequeue();
            if (enProceso.Contains(coord) || chunksInfinitos.ContainsKey(coord)) continue;
            enProceso.Add(coord);
            EncolarChunkConLOD(coord);
            procesados++;
        }

        // Descargar chunks lejanos
        List<Vector2Int> paraEliminar = new List<Vector2Int>();
        foreach (var kv in chunksInfinitos)
        {
            float distancia = Vector2Int.Distance(kv.Key, jugadorChunk);
            if (distancia > distanciaRender + 2)
            {
                paraEliminar.Add(kv.Key); // acumula claves para eliminar después
            }
        }
        foreach (var c in paraEliminar)
        {
            if (chunksInfinitos.TryGetValue(c, out var chunk) && chunk.chunkObject != null)
                Destroy(chunk.chunkObject);
            chunksInfinitos.Remove(c);
        }
        foreach (var c in paraEliminar) chunksInfinitos.Remove(c);

        // Selección de tipo de bloque
        for (int i = 1; i <= 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                tipoDeBloqueActivo = i;

        // Interacción por raycast
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, capaBloques))
        {
            Vector3Int blockPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.5f);
            Vector3Int adjacentPos = Vector3Int.FloorToInt(hit.point + hit.normal * 0.5f);
            bool quitar = removeBlockAction != null && removeBlockAction.triggered;
            bool colocar = placeBlockAction != null && placeBlockAction.triggered;

            if (contornoActual != null) Destroy(contornoActual);
            contornoActual = MostrarContorno(blockPos);

            if (quitar)
            {
                RemoveBlock(blockPos);
            }
            else if (colocar)
            {
                if (tipoDeBloqueActivo == 4) GenerarEnemigo(adjacentPos);
                else AddBlock(adjacentPos, tipoDeBloqueActivo);
            }
        }
        else
        {
            if (contornoActual != null)
            {
                Destroy(contornoActual);
                contornoActual = null;
            }
        }
    }

    void OnDestroy()
    {
        juegoActivo = false;
    }

    // ==============================
    // Chunks infinitos
    // ==============================
    void EnqueueAround(Vector2Int centro)
    {
        for (int dx = -distanciaRender; dx <= distanciaRender; dx++)
        {
            for (int dz = -distanciaRender; dz <= distanciaRender; dz++)
            {
                Vector2Int coord = new Vector2Int(centro.x + dx, centro.y + dz);
                if (!chunksInfinitos.ContainsKey(coord) && !enProceso.Contains(coord))
                    colaChunksPendientes.Enqueue(coord);
            }
        }
    }

    private void EncolarChunkConLOD(Vector2Int coord)
    {
        Vector2Int jugadorChunkCoord = GetChunkCoord(jugadorTransform.position);
        int distanciaX = Mathf.Abs(jugadorChunkCoord.x - coord.x);
        int distanciaZ = Mathf.Abs(jugadorChunkCoord.y - coord.y);
        int maxDistancia = Mathf.Max(distanciaX, distanciaZ);

        int lod = 0;
        for (int i = 0; i < distanciasLOD.Length; i++)
            if (maxDistancia > distanciasLOD[i]) lod = i + 1;
        lod = Mathf.Min(lod, maxLOD);

        GenerarChunkConLOD(coord, lod); // ✅ esta es la llamada correcta
    }

    async void GenerarChunkConLOD(Vector2Int coord, int lod)
    {
        if (!juegoActivo) return;

        Vector3Int origin = new Vector3Int(coord.x * chunkSizeX, 0, coord.y * chunkSizeZ);
        Chunk chunk = new Chunk(chunkSizeX, chunkSizeY, chunkSizeZ, origin);

        await Task.Run(() =>
        {
            for (int lx = 0; lx < chunkSizeX; lx++)
            {
                for (int lz = 0; lz < chunkSizeZ; lz++)
                {
                    int worldX = origin.x + lx;
                    int worldZ = origin.z + lz;

                    float ruido = 0f;
                    float[] escalas = { 0.005f, 0.02f, 0.1f, 0.5f };
                    foreach (float s in escalas)
                    {
                        float nx = (worldX + semilla) * s;
                        float nz = (worldZ + semilla) * s;
                        ruido += Mathf.PerlinNoise(nx, nz) * chunkSizeY;
                    }

                    ruido *= Mathf.Sin(worldX * 0.0001f) * Mathf.Cos(worldZ * 0.0001f);

                    int altura = CalcularAltura(worldX, worldZ, chunkSizeY, semilla);

                    for (int y = 0; y <= altura; y++)
                    {
                        int tipo = (y == altura) ? 1 : (y >= altura - 3 ? 2 : 4);
                        if (tipo > 0 && tipo <= bloquesDisponibles.Length)
                            chunk.blocks[lx, y, lz] = tipo;
                    }
                }
            }
        });

        if (!Application.isPlaying || this == null || gameObject == null) return;

        chunk.chunkObject = new GameObject($"Chunk_{coord.x}_{coord.y}");
        chunk.chunkObject.transform.parent = transform;
        chunk.chunkObject.transform.position = origin;
        chunk.chunkObject.layer = LayerMask.NameToLayer("Bloques");

        chunk.meshFilter = chunk.chunkObject.AddComponent<MeshFilter>();
        chunk.meshRenderer = chunk.chunkObject.AddComponent<MeshRenderer>();
        chunk.meshCollider = chunk.chunkObject.AddComponent<MeshCollider>();

        chunksInfinitos[coord] = chunk;
        enProceso.Remove(coord);

        chunk.RequestMeshGeneration(bloquesDisponibles, GetBlockAtWorldPos, lod);
    }

    // ==============================
    // Bloques
    // ==============================
    void RemoveBlock(Vector3Int pos)
    {
        if (!EstaDentroDeRango(pos)) return;

        (Chunk chunk, Vector3Int localPos, Vector2Int chunkCoord) = GetChunkAndLocalFromWorld(pos);
        if (chunk == null) return;

        int tipo = chunk.blocks[localPos.x, localPos.y, localPos.z];
        if (tipo == 0) return;

        GenerarParticulas(pos, tipo);

        chunk.blocks[localPos.x, localPos.y, localPos.z] = 0;
        chunk.RegenerateMesh(bloquesDisponibles, GetBlockAtWorldPos, 0); // LOD base

        // Si es borde, intentar regenerar vecinos
        if (EsBordeDelChunk(localPos))
        {
            Vector3Int[] vecinos = {
                pos + Vector3Int.right,
                pos + Vector3Int.left,
                pos + Vector3Int.forward,
                pos + Vector3Int.back
            };

            foreach (var v in vecinos)
            {
                (Chunk chV, _, _) = GetChunkAndLocalFromWorld(v);
                if (chV != null) chV.RegenerateMesh(bloquesDisponibles, GetBlockAtWorldPos, 0); // LOD base

            }
        }
        ReproducirSonidoModificar(pos, tipo);
    }

    void AddBlock(Vector3Int pos, int tipo)
    {
        if (!EstaDentroDeRango(pos)) return;

        // Evitar colocar dentro del jugador
        Vector3 centroBloque = new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f);
        Collider[] colisiones = Physics.OverlapBox(centroBloque, Vector3.one * 0.5f);
        foreach (Collider col in colisiones) if (col == jugadorCollider) return;

        (Chunk chunk, Vector3Int localPos, Vector2Int chunkCoord) = GetChunkAndLocalFromWorld(pos);

        // Si el chunk aún no existe pero está dentro del radio, se puede encolar
        if (chunk == null)
        {
            Vector2Int c2 = WorldToChunkCoord(pos);
            if (!enProceso.Contains(c2) && !chunksInfinitos.ContainsKey(c2))
                colaChunksPendientes.Enqueue(c2);
            return;
        }

        if (tipo <= 0 || tipo > bloquesDisponibles.Length) return;

        chunk.blocks[localPos.x, localPos.y, localPos.z] = tipo;
        chunk.RegenerateMesh(bloquesDisponibles, GetBlockAtWorldPos, 0); // LOD base

        if (EsBordeDelChunk(localPos))
        {
            Vector3Int[] vecinos = {
                pos + Vector3Int.right,
                pos + Vector3Int.left,
                pos + Vector3Int.forward,
                pos + Vector3Int.back
            };
            foreach (var v in vecinos)
            {
                (Chunk chV, _, _) = GetChunkAndLocalFromWorld(v);
                if (chV != null) chV.RegenerateMesh(bloquesDisponibles, GetBlockAtWorldPos, 0); // LOD base

            }
        }
        ReproducirSonidoModificar(pos, tipo);
    }

    // ==============================
    // Utilidades
    // ==============================
    // Distancia corta para interacción (no limita mundo)
    bool EstaDentroDeRango(Vector3Int bloque)
    {
        if (jugadorTransform == null) return false;
        Vector3 centroBloque = bloque + Vector3.one * 0.5f;
        float distancia = Vector3.Distance(jugadorTransform.position, centroBloque);
        return distancia <= 4.5f;
    }

    // Coord de chunk (maneja negativos)
    Vector2Int GetChunkCoord(Vector3 pos)
    {
        int cx = FloorDiv((int)Mathf.Floor(pos.x), chunkSizeX);
        int cz = FloorDiv((int)Mathf.Floor(pos.z), chunkSizeZ);
        return new Vector2Int(cx, cz);
    }

    // Para posiciones enteras
    Vector2Int WorldToChunkCoord(Vector3Int wp)
    {
        int cx = FloorDiv(wp.x, chunkSizeX);
        int cz = FloorDiv(wp.z, chunkSizeZ);
        return new Vector2Int(cx, cz);
    }

    Vector3Int WorldToLocal(Vector3Int wp)
    {
        int lx = FloorMod(wp.x, chunkSizeX);
        int ly = wp.y; // vertical simple
        int lz = FloorMod(wp.z, chunkSizeZ);
        return new Vector3Int(lx, ly, lz);
    }

    int FloorDiv(int a, int b)
    {
        int q = a / b;
        int r = a % b;
        if ((r != 0) && ((r > 0) != (b > 0))) q--;
        return q;
    }

    int FloorMod(int a, int b)
    {
        int r = a % b;
        if ((r != 0) && ((r > 0) != (b > 0))) r += b;
        return r;
    }

    // Devuelve chunk + local + coord desde worldPos
    (Chunk, Vector3Int, Vector2Int) GetChunkAndLocalFromWorld(Vector3Int worldPos)
    {
        // Limitar vertical para evitar IndexOutOfRange
        if (worldPos.y < 0 || worldPos.y >= chunkSizeY) return (null, Vector3Int.zero, default);

        Vector2Int coord = WorldToChunkCoord(worldPos);
        if (!chunksInfinitos.TryGetValue(coord, out var chunk)) return (null, Vector3Int.zero, coord);

        Vector3Int local = WorldToLocal(worldPos);
        return (chunk, local, coord);
    }

    public int GetBlockAtWorldPos(Vector3Int pos)
    {
        if (pos.y < 0 || pos.y >= chunkSizeY) return 0;

        Vector2Int coord = WorldToChunkCoord(pos);
        if (!chunksInfinitos.TryGetValue(coord, out var chunk)) return -1; // ← corregido

        Vector3Int local = WorldToLocal(pos);

        if (local.x < 0 || local.x >= chunkSizeX ||
            local.y < 0 || local.y >= chunkSizeY ||
            local.z < 0 || local.z >= chunkSizeZ) return 0;

        return chunk.blocks[local.x, local.y, local.z];
    }

    bool EsBordeDelChunk(Vector3Int localPos)
    {
        return localPos.x == 0 || localPos.x == chunkSizeX - 1 ||
               localPos.z == 0 || localPos.z == chunkSizeZ - 1;
    }

    // ==============================
    // Extras
    // ==============================
    void GenerarParticulas(Vector3Int pos, int tipo)
    {
        if (prefabParticulaBloque == null) return;

        Vector3 centroBloque = pos + Vector3.one * 0.5f;
        Material caraSuperior = null;

        if (tipo > 0 && tipo <= bloquesDisponibles.Length)
            caraSuperior = bloquesDisponibles[tipo - 1].faceMaterials[4]; // cara superior

        for (int i = 0; i < 6; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 0.2f;
            GameObject particula = Instantiate(prefabParticulaBloque, centroBloque + offset, Quaternion.identity);
            particula.transform.localScale = Vector3.one * 0.3f;

            MeshRenderer mr = particula.GetComponent<MeshRenderer>();
            if (mr != null && caraSuperior != null) mr.material = caraSuperior;

            Rigidbody rb = particula.GetComponent<Rigidbody>();
            if (rb != null) rb.AddForce(Random.onUnitSphere * 2f, ForceMode.Impulse);
        }
    }

    void GenerarEnemigo(Vector3Int pos)
    {
        if (prefabEnemigo == null) return;

        Vector3 spawn = pos + new Vector3(0.5f, 0, 0.5f);
        GameObject enemigo = Instantiate(prefabEnemigo, spawn, Quaternion.identity);

        EnemyTrial scriptEnemigo = enemigo.GetComponent<EnemyTrial>();
        if (scriptEnemigo != null)
        {
            scriptEnemigo.objetivo = GameObject.FindWithTag("Player")?.transform;
            scriptEnemigo.vidaController = Object.FindFirstObjectByType<VidaController>();
        }
    }

    GameObject MostrarContorno(Vector3Int blockPos)
    {
        if (prefabContornoCara == null) return null;
        if (!EstaDentroDeRango(blockPos)) return null;

        Vector3 centro = blockPos + Vector3.one * 0.5f;
        GameObject contorno = Instantiate(prefabContornoCara, centro, Quaternion.identity);
        contorno.transform.localScale = Vector3.one * 1.01f;
        return contorno;
    }

    int CalcularAltura(int worldX, int worldZ, int chunkSizeY, int semilla)
    {
        // Semillas desplazadas para evitar patrones repetitivos
        float ox = semilla * 0.1234f + 137.2f;
        float oz = semilla * 0.5678f + 911.7f;

        // Parámetros base
        float frecuencia = 0.003f;
        float amplitud = 1f;
        float suma = 0f;
        float norma = 0f;

        // fBm: fractal Brownian motion con 5 octavas
        for (int o = 0; o < 5; o++)
        {
            float nx = (worldX + ox) * frecuencia;
            float nz = (worldZ + oz) * frecuencia;
            float n = Mathf.PerlinNoise(nx, nz); // rango [0,1]
            suma += n * amplitud;
            norma += amplitud;
            amplitud *= 0.5f;
            frecuencia *= 2f;
        }

        float ruido = (norma > 0f) ? (suma / norma) : 0f; // normalizado a [0,1]

        // Continentalidad suave (sin sesgo direccional)
        float warp = Mathf.PerlinNoise((worldX + ox) * 0.0008f, (worldZ + oz) * 0.0008f);
        float factor = Mathf.Lerp(0.85f, 1.15f, warp);
        ruido *= factor;

        // Escalar a altura útil
        int alturaMax = Mathf.FloorToInt(chunkSizeY * 0.6f);
        int altura = Mathf.Clamp(Mathf.FloorToInt(ruido * alturaMax), 0, chunkSizeY - 1);

        return altura;
    }

    void ReproducirSonidoModificar(Vector3Int pos, int tipo)
    {
        if (tipo <= 0 || tipo > bloquesDisponibles.Length) return;

        BlockType bloque = bloquesDisponibles[tipo - 1];
        AudioClip[] clips = bloque.sonidosModificar;
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        audioSource.PlayOneShot(clip);
    }
}
