using System.Collections.Generic;
using UnityEngine;

public class BoostManager : MonoBehaviour
{
    [System.Serializable]
    public class BoosterConfig
    {
        public string boosterName = "SpeedBooster";
        public Vector3 position = Vector3.zero;
        public float multiplier = 2.0f;
        public float duration = 3.0f;
        public float force = 5000f;
        public float cooldown = 1.0f;
        public bool canStack = false;
        public Color boosterColor = Color.cyan;
    }
    
    [Header("Configuración de Boosters")]
    public List<BoosterConfig> boosterConfigs = new List<BoosterConfig>();
    public GameObject boosterPrefab; // Prefab personalizado del booster
    
    [Header("Configuración Global")]
    public bool enableBoosters = true;
    public bool randomizeBoosterPositions = false;
    public int maxBoostersPerLevel = 10;
    
    private List<SpeedBooster> activeBoosters = new List<SpeedBooster>();
    
    private void Start()
    {
        if (enableBoosters)
        {
            CreateBoosters();
        }
    }
    
    private void CreateBoosters()
    {
        foreach (BoosterConfig config in boosterConfigs)
        {
            CreateBooster(config);
        }
        
        // Si se quiere randomizar posiciones
        if (randomizeBoosterPositions)
        {
            RandomizeBoosterPositions();
        }
    }
    
    private void CreateBooster(BoosterConfig config)
    {
        GameObject boosterObject;
        
        if (boosterPrefab != null)
        {
            boosterObject = Instantiate(boosterPrefab, config.position, Quaternion.identity);
        }
        else
        {
            boosterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boosterObject.transform.position = config.position;
            
            // Configurar el collider como trigger
            Collider collider = boosterObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }
        
        boosterObject.name = config.boosterName;
        
        // Agregar el script SpeedBooster
        SpeedBooster speedBooster = boosterObject.GetComponent<SpeedBooster>();
        if (speedBooster == null)
        {
            speedBooster = boosterObject.AddComponent<SpeedBooster>();
        }
        
        // Configurar el booster
        speedBooster.boostMultiplier = config.multiplier;
        speedBooster.boostDuration = config.duration;
        speedBooster.boostForce = config.force;
        speedBooster.cooldownTime = config.cooldown;
        speedBooster.canStackBoosts = config.canStack;
        
        // Cambiar color del booster
        Renderer renderer = boosterObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(renderer.material);
            material.color = config.boosterColor;
            renderer.material = material;
        }
        
        activeBoosters.Add(speedBooster);
    }
    
    private void RandomizeBoosterPositions()
    {
        // Obtener el tamaño del nivel (aproximado)
        float levelSize = 100f; // Ajustar según el tamaño de tu nivel
        
        foreach (SpeedBooster booster in activeBoosters)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-levelSize/2, levelSize/2),
                0.5f, // Altura fija sobre el suelo
                Random.Range(-levelSize/2, levelSize/2)
            );
            
            booster.transform.position = randomPosition;
        }
    }
    
    // Método para crear un booster en tiempo de ejecución
    public SpeedBooster CreateBoosterAtPosition(Vector3 position, float multiplier = 2.0f, float duration = 3.0f)
    {
        if (activeBoosters.Count >= maxBoostersPerLevel)
        {
            Debug.LogWarning("Se ha alcanzado el límite máximo de boosters por nivel");
            return null;
        }
        
        BoosterConfig config = new BoosterConfig
        {
            boosterName = "DynamicBooster",
            position = position,
            multiplier = multiplier,
            duration = duration
        };
        
        CreateBooster(config);
        return activeBoosters[activeBoosters.Count - 1];
    }
    
    // Método para desactivar todos los boosters
    public void DisableAllBoosters()
    {
        foreach (SpeedBooster booster in activeBoosters)
        {
            if (booster != null)
            {
                booster.gameObject.SetActive(false);
            }
        }
    }
    
    // Método para activar todos los boosters
    public void EnableAllBoosters()
    {
        foreach (SpeedBooster booster in activeBoosters)
        {
            if (booster != null)
            {
                booster.gameObject.SetActive(true);
            }
        }
    }
    
    // Método para cambiar la configuración de todos los boosters
    public void UpdateAllBoosters(float multiplier, float duration, float force)
    {
        foreach (SpeedBooster booster in activeBoosters)
        {
            if (booster != null)
            {
                booster.SetBoostSettings(multiplier, duration, force);
            }
        }
    }
    
    // Método para obtener estadísticas de boosters
    public int GetActiveBoosterCount()
    {
        return activeBoosters.Count;
    }
    
    // Método para limpiar boosters destruidos
    public void CleanupDestroyedBoosters()
    {
        activeBoosters.RemoveAll(booster => booster == null);
    }
} 