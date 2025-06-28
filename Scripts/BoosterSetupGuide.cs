using UnityEngine;

// Script de guía para configurar boosters - NO USAR EN PRODUCCIÓN
public class BoosterSetupGuide : MonoBehaviour
{
    [Header("GUÍA DE CONFIGURACIÓN")]
    [TextArea(5, 10)]
    public string setupInstructions = @"
PASOS PARA CONFIGURAR BOOSTERS:

1. CREAR BOOSTER:
   - Crea un cubo en la escena
   - Nómbralo 'SpeedBooster'
   - Agrega el script SpeedBooster
   - Configura el Collider como Trigger

2. COLOCAR EN CIRCUITO:
   - Posiciónalo en una recta o curva
   - Asegúrate de que esté al nivel del suelo
   - Hazlo visible (color cyan)

3. CONFIGURAR VALORES:
   - Boost Multiplier: 2.0 (duplica velocidad)
   - Boost Duration: 3.0 (3 segundos)
   - Boost Force: 5000 (fuerza adicional)
   - Cooldown Time: 1.0 (1 segundo entre usos)

4. PROBAR:
   - Ejecuta el juego
   - Conduce sobre el booster
   - Deberías acelerar automáticamente
";

    [Header("CONFIGURACIÓN RÁPIDA")]
    public bool createBoosterOnStart = false;
    public Vector3 boosterPosition = new Vector3(0, 0.5f, 10f);
    
    private void Start()
    {
        if (createBoosterOnStart)
        {
            CreateQuickBooster();
        }
    }
    
    private void CreateQuickBooster()
    {
        // Crear objeto booster
        GameObject booster = GameObject.CreatePrimitive(PrimitiveType.Cube);
        booster.name = "SpeedBooster";
        booster.transform.position = boosterPosition;
        booster.transform.localScale = new Vector3(3f, 0.5f, 3f);
        
        // Configurar collider como trigger
        Collider collider = booster.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Configurar material
        Renderer renderer = booster.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.cyan;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.cyan * 0.5f);
            renderer.material = material;
        }
        
        // Agregar script SpeedBooster
        SpeedBooster speedBooster = booster.AddComponent<SpeedBooster>();
        speedBooster.boostMultiplier = 2.0f;
        speedBooster.boostDuration = 3.0f;
        speedBooster.boostForce = 5000f;
        speedBooster.cooldownTime = 1.0f;
        
        Debug.Log("¡Booster creado automáticamente! Conduce sobre él para activar el boost.");
    }
    
    [ContextMenu("Crear Booster Manualmente")]
    public void CreateBoosterManually()
    {
        CreateQuickBooster();
    }
} 