using UnityEngine;

public class ParticulaBloque : MonoBehaviour
{
    private Transform jugador;
    private float tiempoVida = 0.7f;

    void Start()
    {
        jugador = GameObject.FindWithTag("Player")?.transform;
        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        if (jugador != null)
        {
            Vector3 direccion = transform.position - jugador.position;
            transform.rotation = Quaternion.LookRotation(direccion);
        }
    }
}
