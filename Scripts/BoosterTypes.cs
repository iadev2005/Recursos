using UnityEngine;

[System.Serializable]
public class BoosterType
{
    public string typeName;
    public float multiplier;
    public float duration;
    public float force;
    public float cooldown;
    public Color color;
    public AudioClip sound;
    public ParticleSystem particles;
    public string description;
}

public class BoosterTypes : MonoBehaviour
{
    [Header("Tipos de Boosters Predefinidos")]
    public BoosterType[] boosterTypes;
    
    [Header("Configuración por Defecto")]
    public BoosterType defaultBooster;
    
    private void Awake()
    {
        InitializeDefaultBoosters();
    }
    
    private void InitializeDefaultBoosters()
    {
        if (boosterTypes == null || boosterTypes.Length == 0)
        {
            boosterTypes = new BoosterType[]
            {
                new BoosterType
                {
                    typeName = "Speed Boost",
                    multiplier = 2.0f,
                    duration = 3.0f,
                    force = 5000f,
                    cooldown = 1.0f,
                    color = Color.cyan,
                    description = "Aumenta la velocidad del carro por 3 segundos"
                },
                new BoosterType
                {
                    typeName = "Super Boost",
                    multiplier = 3.0f,
                    duration = 2.0f,
                    force = 8000f,
                    cooldown = 2.0f,
                    color = Color.red,
                    description = "Boost súper potente pero de corta duración"
                },
                new BoosterType
                {
                    typeName = "Endurance Boost",
                    multiplier = 1.5f,
                    duration = 8.0f,
                    force = 3000f,
                    cooldown = 0.5f,
                    color = Color.green,
                    description = "Boost moderado pero de larga duración"
                },
                new BoosterType
                {
                    typeName = "Turbo Boost",
                    multiplier = 2.5f,
                    duration = 1.5f,
                    force = 10000f,
                    cooldown = 3.0f,
                    color = Color.yellow,
                    description = "Boost extremadamente potente pero muy corto"
                },
                new BoosterType
                {
                    typeName = "Stealth Boost",
                    multiplier = 1.8f,
                    duration = 5.0f,
                    force = 4000f,
                    cooldown = 0.3f,
                    color = Color.magenta,
                    description = "Boost discreto pero efectivo"
                }
            };
        }
        
        defaultBooster = boosterTypes[0];
    }
    
    // Método para obtener un tipo de booster por nombre
    public BoosterType GetBoosterType(string typeName)
    {
        foreach (BoosterType type in boosterTypes)
        {
            if (type.typeName == typeName)
            {
                return type;
            }
        }
        
        Debug.LogWarning($"Tipo de booster '{typeName}' no encontrado. Usando booster por defecto.");
        return defaultBooster;
    }
    
    // Método para obtener un tipo de booster aleatorio
    public BoosterType GetRandomBoosterType()
    {
        if (boosterTypes.Length == 0) return defaultBooster;
        
        int randomIndex = Random.Range(0, boosterTypes.Length);
        return boosterTypes[randomIndex];
    }
    
    // Método para aplicar configuración de un tipo a un SpeedBooster
    public void ApplyBoosterType(SpeedBooster speedBooster, BoosterType boosterType)
    {
        if (speedBooster == null || boosterType == null) return;
        
        speedBooster.boostMultiplier = boosterType.multiplier;
        speedBooster.boostDuration = boosterType.duration;
        speedBooster.boostForce = boosterType.force;
        speedBooster.cooldownTime = boosterType.cooldown;
        speedBooster.boostSound = boosterType.sound;
        speedBooster.boostEffect = boosterType.particles;
        
        // Cambiar el color del booster
        Renderer renderer = speedBooster.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(renderer.material);
            material.color = boosterType.color;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", boosterType.color * 0.5f);
            renderer.material = material;
        }
    }
    
    // Método para crear un booster con un tipo específico
    public SpeedBooster CreateBoosterWithType(Vector3 position, string typeName)
    {
        BoosterType type = GetBoosterType(typeName);
        return CreateBoosterWithType(position, type);
    }
    
    public SpeedBooster CreateBoosterWithType(Vector3 position, BoosterType type)
    {
        GameObject boosterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boosterObject.name = $"Booster_{type.typeName}";
        boosterObject.transform.position = position;
        
        // Configurar el collider como trigger
        Collider collider = boosterObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Agregar el script SpeedBooster
        SpeedBooster speedBooster = boosterObject.AddComponent<SpeedBooster>();
        
        // Aplicar la configuración del tipo
        ApplyBoosterType(speedBooster, type);
        
        return speedBooster;
    }
    
    // Método para obtener información de todos los tipos
    public string[] GetAllBoosterTypeNames()
    {
        string[] names = new string[boosterTypes.Length];
        for (int i = 0; i < boosterTypes.Length; i++)
        {
            names[i] = boosterTypes[i].typeName;
        }
        return names;
    }
    
    // Método para obtener descripción de un tipo
    public string GetBoosterTypeDescription(string typeName)
    {
        BoosterType type = GetBoosterType(typeName);
        return type != null ? type.description : "Descripción no disponible";
    }
} 