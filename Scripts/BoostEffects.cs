using System.Collections;
using UnityEngine;

public class BoostEffects : MonoBehaviour
{
    [Header("Efectos de Partículas")]
    public ParticleSystem speedLines;           // Líneas de velocidad
    public ParticleSystem boostTrail;           // Estela del boost
    public ParticleSystem engineGlow;           // Brillo del motor
    
    [Header("Efectos de Material")]
    public Material boostCarMaterial;           // Material del carro durante boost
    public Material normalCarMaterial;          // Material normal del carro
    public Renderer[] carRenderers;             // Renderers del carro para cambiar material
    
    [Header("Efectos de Luz")]
    public Light[] boostLights;                 // Luces que se activan durante boost
    public Color boostLightColor = Color.cyan;  // Color de las luces de boost
    
    [Header("Efectos de Cámara")]
    public CameraFollow cameraFollow;           // Referencia a la cámara
    public float boostFOV = 80f;                // FOV durante boost
    public float normalFOV = 60f;               // FOV normal
    
    private bool isBoosted = false;
    private Color[] originalLightColors;
    private float originalFOV;
    
    private void Start()
    {
        // Guardar colores originales de las luces
        if (boostLights != null && boostLights.Length > 0)
        {
            originalLightColors = new Color[boostLights.Length];
            for (int i = 0; i < boostLights.Length; i++)
            {
                if (boostLights[i] != null)
                {
                    originalLightColors[i] = boostLights[i].color;
                }
            }
        }
        
        // Guardar FOV original
        if (cameraFollow != null)
        {
            Camera cam = cameraFollow.GetComponent<Camera>();
            if (cam != null)
            {
                originalFOV = cam.fieldOfView;
            }
        }
        
        // Desactivar efectos inicialmente
        StopAllEffects();
    }
    
    public void StartBoostEffects()
    {
        if (isBoosted) return;
        
        isBoosted = true;
        
        // Activar partículas
        if (speedLines != null) speedLines.Play();
        if (boostTrail != null) boostTrail.Play();
        if (engineGlow != null) engineGlow.Play();
        
        // Cambiar materiales
        ChangeCarMaterials(true);
        
        // Activar luces
        ActivateBoostLights(true);
        
        // Cambiar FOV de la cámara
        ChangeCameraFOV(true);
    }
    
    public void StopBoostEffects()
    {
        if (!isBoosted) return;
        
        isBoosted = false;
        
        // Detener partículas
        StopAllEffects();
        
        // Restaurar materiales
        ChangeCarMaterials(false);
        
        // Desactivar luces
        ActivateBoostLights(false);
        
        // Restaurar FOV de la cámara
        ChangeCameraFOV(false);
    }
    
    private void StopAllEffects()
    {
        if (speedLines != null) speedLines.Stop();
        if (boostTrail != null) boostTrail.Stop();
        if (engineGlow != null) engineGlow.Stop();
    }
    
    private void ChangeCarMaterials(bool useBoostMaterial)
    {
        if (carRenderers == null) return;
        
        Material targetMaterial = useBoostMaterial ? boostCarMaterial : normalCarMaterial;
        if (targetMaterial == null) return;
        
        foreach (Renderer renderer in carRenderers)
        {
            if (renderer != null)
            {
                renderer.material = targetMaterial;
            }
        }
    }
    
    private void ActivateBoostLights(bool activate)
    {
        if (boostLights == null) return;
        
        for (int i = 0; i < boostLights.Length; i++)
        {
            if (boostLights[i] != null)
            {
                boostLights[i].color = activate ? boostLightColor : originalLightColors[i];
                boostLights[i].intensity = activate ? 2f : 1f;
            }
        }
    }
    
    private void ChangeCameraFOV(bool useBoostFOV)
    {
        if (cameraFollow == null) return;
        
        Camera cam = cameraFollow.GetComponent<Camera>();
        if (cam != null)
        {
            float targetFOV = useBoostFOV ? boostFOV : originalFOV;
            StartCoroutine(LerpCameraFOV(cam, targetFOV, 0.5f));
        }
    }
    
    private IEnumerator LerpCameraFOV(Camera camera, float targetFOV, float duration)
    {
        float startFOV = camera.fieldOfView;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            camera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }
        
        camera.fieldOfView = targetFOV;
    }
    
    // Método público para verificar si está en boost
    public bool IsBoosted()
    {
        return isBoosted;
    }
} 