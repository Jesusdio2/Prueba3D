using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyTrial : MonoBehaviour {
    public Transform objetivo;
    public VidaController vidaController;
    public float velocidad = 3f;
    public float jumpSpeed = 6f;
    public float gravity = 20f;
    public float detectDistance = 0.6f;     // distancia frente al enemigo
    public float maxStepHeight = 1.05f;     // altura máx. del “bloque”
    public LayerMask groundMask = ~0;       // capa del suelo/bloques
    public float dañoPorGolpe = 0.5f;
    public float tiempoEntreGolpes = 1f;
    private float tiempoUltimoGolpe = -999f;
    CharacterController cc;
    float vy;

    void Awake() {
        cc = GetComponent<CharacterController>();
        cc.slopeLimit = 50f;
    }

    void Update()
    {
        if (transform.position.y < -40f)
        {
            Destroy(gameObject);
        }

        // Dirección horizontal hacia el jugador
        Vector3 dir = (objetivo.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.0001f) dir.Normalize();

        // Movimiento horizontal
        Vector3 move = dir * velocidad;

        // StepOffset solo cuando está en el suelo
        cc.stepOffset = cc.isGrounded ? Mathf.Min(maxStepHeight, cc.height - 0.1f) : 0f;

        // Reset de velocidad vertical al tocar suelo
        if (cc.isGrounded && vy < 0f) vy = -1f;

        // Auto-salto si hay obstáculo tipo “bloque” delante
        if (cc.isGrounded && IsOneBlockAhead(dir))
        {
            vy = jumpSpeed;
        }

        // Gravedad
        vy -= gravity * Time.deltaTime;
        move.y = vy;

        // Aplicar movimiento
        cc.Move(move * Time.deltaTime);

        // Rotación suave hacia el objetivo
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime);
        }
        
        // Daño por proximidad con temporizador
        float distancia = Vector3.Distance(transform.position, objetivo.position);
        if (distancia < 1.2f) { // distancia de contacto
        float tiempoActual = Time.time;
        if (tiempoActual - tiempoUltimoGolpe >= tiempoEntreGolpes) {
        ControlJugador jugador = objetivo.GetComponent<ControlJugador>();
        if (jugador != null) {
            vidaController.vida -= dañoPorGolpe;
            vidaController.vida = Mathf.Clamp(vidaController.vida, 0f, 9f);
            tiempoUltimoGolpe = tiempoActual;
        }
    }
}

    }

    bool IsOneBlockAhead(Vector3 dir) {
        // Ray bajo: ¿hay pared/obstáculo justo delante?
        if (!Physics.Raycast(transform.position + Vector3.up * 0.2f, dir, detectDistance)) return false;

        // Punto por encima del posible escalón
        Vector3 topOrigin = transform.position + Vector3.up * (maxStepHeight + 0.1f) + dir * detectDistance;

        // ¿Hay suelo a altura de bloque delante?
        if (Physics.Raycast(topOrigin, Vector3.down, out RaycastHit stepHit, maxStepHeight + 0.4f, groundMask)) {
            float step = stepHit.point.y - transform.position.y;
            if (step > 0.3f && step <= maxStepHeight + 0.05f) {
                // ¿Hay espacio libre encima para no chocar con techo?
                bool freeHead = !Physics.Raycast(stepHit.point + Vector3.up * 0.1f, Vector3.up, 0.9f);
                return freeHead;
            }
        }
        return false;
    }
}
