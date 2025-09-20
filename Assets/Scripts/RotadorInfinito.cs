using UnityEngine;

public class RotadorInfinito : MonoBehaviour
{
    public Vector3 velocidadRotacion = new Vector3(0f, 20f, 0f); // grados por segundo

    void Update()
    {
        transform.Rotate(velocidadRotacion * Time.deltaTime);
    }
}
