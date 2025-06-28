using UnityEngine;
using UnityEditor;

public class BoosterPrefabCreator : MonoBehaviour
{
    [Header("Configuración del Prefab")]
    public string prefabName = "SpeedBooster";
    public Vector3 boosterSize = new Vector3(3f, 0.5f, 3f);
    public Color boosterColor = Color.cyan;
    public Material boosterMaterial;
    
    [Header("Efectos")]
    public ParticleSystem boosterParticles;
    public AudioClip boosterSound;
    public Light boosterLight;
    
    [ContextMenu("Crear Prefab del Booster")]
    public void CreateBoosterPrefab()
    {
        // Crear el objeto base
        GameObject boosterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boosterObject.name = prefabName;
        
        // Configurar el transform
        boosterObject.transform.localScale = boosterSize;
        
        // Configurar el collider como trigger
        Collider collider = boosterObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Configurar el material
        Renderer renderer = boosterObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (boosterMaterial != null)
            {
                renderer.material = boosterMaterial;
            }
            else
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = boosterColor;
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", boosterColor * 0.5f);
                renderer.material = material;
            }
        }
        
        // Agregar el script SpeedBooster
        SpeedBooster speedBooster = boosterObject.AddComponent<SpeedBooster>();
        
        // Configurar efectos de partículas
        if (boosterParticles != null)
        {
            ParticleSystem particles = Instantiate(boosterParticles, boosterObject.transform);
            particles.transform.localPosition = Vector3.zero;
            speedBooster.boostEffect = particles;
        }
        else
        {
            // Crear partículas por defecto
            CreateDefaultParticles(boosterObject, speedBooster);
        }
        
        // Configurar sonido
        if (boosterSound != null)
        {
            speedBooster.boostSound = boosterSound;
        }
        
        // Configurar luz
        if (boosterLight != null)
        {
            Light light = Instantiate(boosterLight, boosterObject.transform);
            light.transform.localPosition = Vector3.zero;
        }
        else
        {
            // Crear luz por defecto
            CreateDefaultLight(boosterObject);
        }
        
        // Agregar animación de rotación
        AddRotationAnimation(boosterObject);
        
        // Agregar efecto de pulso
        AddPulseEffect(boosterObject);
        
        Debug.Log($"Prefab del booster '{prefabName}' creado exitosamente!");
    }
    
    private void CreateDefaultParticles(GameObject parent, SpeedBooster speedBooster)
    {
        GameObject particlesObject = new GameObject("BoosterParticles");
        particlesObject.transform.SetParent(parent.transform);
        particlesObject.transform.localPosition = Vector3.zero;
        
        ParticleSystem particles = particlesObject.AddComponent<ParticleSystem>();
        
        // Configurar el sistema de partículas
        var main = particles.main;
        main.startLifetime = 1.0f;
        main.startSpeed = 2.0f;
        main.startSize = 0.1f;
        main.startColor = Color.cyan;
        main.maxParticles = 50;
        
        var emission = particles.emission;
        emission.rateOverTime = 20;
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1.5f;
        
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.cyan, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
        
        speedBooster.boostEffect = particles;
    }
    
    private void CreateDefaultLight(GameObject parent)
    {
        GameObject lightObject = new GameObject("BoosterLight");
        lightObject.transform.SetParent(parent.transform);
        lightObject.transform.localPosition = Vector3.zero;
        
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Color.cyan;
        light.intensity = 2.0f;
        light.range = 5.0f;
        light.enabled = false; // Se activará cuando se use el booster
    }
    
    private void AddRotationAnimation(GameObject parent)
    {
        // Agregar un objeto hijo para la rotación
        GameObject rotationObject = new GameObject("RotationVisual");
        rotationObject.transform.SetParent(parent.transform);
        rotationObject.transform.localPosition = Vector3.zero;
        
        // Crear un cilindro para la rotación
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.SetParent(rotationObject.transform);
        cylinder.transform.localPosition = Vector3.zero;
        cylinder.transform.localScale = new Vector3(2.5f, 0.1f, 2.5f);
        
        // Configurar el material del cilindro
        Renderer cylinderRenderer = cylinder.GetComponent<Renderer>();
        if (cylinderRenderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.white;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.cyan * 0.3f);
            cylinderRenderer.material = material;
        }
        
        // Agregar script de rotación
        BoosterRotation rotationScript = rotationObject.AddComponent<BoosterRotation>();
    }
    
    private void AddPulseEffect(GameObject parent)
    {
        BoosterPulse pulseScript = parent.AddComponent<BoosterPulse>();
    }
}

// Script para la rotación del booster
public class BoosterRotation : MonoBehaviour
{
    public float rotationSpeed = 90f; // Grados por segundo
    
    private void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}

// Script para el efecto de pulso del booster
public class BoosterPulse : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;
    
    private Vector3 originalScale;
    private float time;
    
    private void Start()
    {
        originalScale = transform.localScale;
    }
    
    private void Update()
    {
        time += Time.deltaTime * pulseSpeed;
        float scale = 1f + Mathf.Sin(time) * pulseAmount;
        transform.localScale = originalScale * scale;
    }
} 