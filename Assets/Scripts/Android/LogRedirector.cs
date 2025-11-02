using UnityEngine;
using System.IO;

public class LogRedirector : MonoBehaviour
{
    string logPath;

    void Awake()
    {
        // Solo activar en Android
        if (Application.platform == RuntimePlatform.Android)
        {
            logPath = Path.Combine(Application.persistentDataPath, "player.log");
            Application.logMessageReceived += OnLogReceived;

            // Opcional: limpiar log anterior
            File.WriteAllText(logPath, "Inicio de sesión de logs en Android\n");
        }
    }

    void OnLogReceived(string condition, string stackTrace, LogType type)
    {
        if (Application.platform != RuntimePlatform.Android) return;

        string mensaje = $"{System.DateTime.Now:HH:mm:ss} [{type}] {condition}\n";
        if (type == LogType.Exception || type == LogType.Error)
            mensaje += stackTrace + "\n";

        File.AppendAllText(logPath, mensaje);
    }

    void OnDestroy()
    {
        if (Application.platform == RuntimePlatform.Android)
            Application.logMessageReceived -= OnLogReceived;
    }
}
