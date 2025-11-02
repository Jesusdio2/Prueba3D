using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseInitializer : MonoBehaviour
{
    void Start()
    {
        // Comprobación de plataforma
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    Debug.Log("✅ Firebase inicializado correctamente en " + Application.platform);
                }
                else
                {
                    Debug.LogError("❌ Error al inicializar Firebase: " + task.Result);
                }
            });
        }
        else
        {
            Debug.Log("⚠️ Plataforma actual (" + Application.platform + ") no requiere inicialización de Firebase.");
        }
    }
}
