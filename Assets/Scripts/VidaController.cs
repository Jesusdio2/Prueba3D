using UnityEngine;
using UnityEngine.UI;

public class VidaController : MonoBehaviour
{
    // Referencias Y Variables
    public float vida = 9;
    public Image[] corazones;
    public Sprite spriteLleno;
    public Sprite spriteMitad;
    public Sprite spriteVacio;
    public GameObject pantallaMuerte;
    public float limiteVacioY = -20f; // Altura mínima antes de empezar a morir
    public Transform PlayerTransform;
    public Vector3 posicionSpawn = new Vector3(14f, 13f, 12f); // Ajusta según tu mundo

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        ActualizarCorazones(vida);
        if (vida <= 0 && !pantallaMuerte.activeSelf)
        {
            pantallaMuerte.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Object.FindFirstObjectByType<PlayerCameraController>().bloquearCamara = true;
        }
        if (PlayerTransform.position.y < limiteVacioY && vida > 0f)
        {
            vida -= Time.deltaTime * 5f; // daño progresivo
            vida = Mathf.Clamp(vida, 0f, 9f);
        }
    }

    void ActualizarCorazones(float vidaActual)
    {
        for (int i = 0; i < corazones.Length; i++)
        {
            if (vidaActual >= i + 1)
            {
                corazones[i].sprite = spriteLleno;
            }
            else if (vidaActual >= i + 0.5f)
            {
                corazones[i].sprite = spriteMitad;
            }
            else
            {
                corazones[i].sprite = spriteVacio;
            }
        }
    }

    public void Revivir()
    {
        vida = 9f;

        CharacterController cc = PlayerTransform.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            PlayerTransform.position = posicionSpawn;
            cc.enabled = true;
        }
        else
        {
            PlayerTransform.position = posicionSpawn;
        }

        pantallaMuerte.SetActive(false);
        Time.timeScale = 1f;
        Object.FindFirstObjectByType<PlayerCameraController>().bloquearCamara = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
