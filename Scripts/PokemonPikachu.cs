using System.Collections;
using UnityEngine;

public class PokemonPikachu : MonoBehaviour
{
    [Header("Configuración del Pikachu")]
    public float slowMultiplier = 0.4f; // Multiplicador de velocidad (menos de 1 para ralentizar)
    public float effectDuration = 3.0f; // Duración del efecto de lentitud
    public float respawnTime = 5.0f;    // Tiempo para reaparecer Pikachu

    [Header("Efectos Visuales y Sonido")]
    public ParticleSystem effectParticles;
    public AudioClip pikachuSound;
    public AudioSource audioSource;
    public Animator animator;
    public GameObject visualObject;
    public float rotationSpeed = 90f;
    public Color paralysisColor = Color.yellow;

    [Header("Efecto Eléctrico")]
    public GameObject electricityEffectPrefab; // Prefab vfx_Electricity_01
    public Transform electricityParent; // Padre para el efecto eléctrico (opcional)

    [Header("Configuración Avanzada")]
    public float cooldownTime = 1.0f;
    public bool enableDebug = false;

    private bool isOnCooldown = false;
    private Collider myCollider;
    private bool isActive = true;
    private Vector3 originalScale;
    private GameObject currentElectricityEffect; // Referencia al efecto eléctrico actual

    private void Start()
    {
        myCollider = GetComponent<Collider>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (pikachuSound == null)
        {
            pikachuSound = Resources.Load<AudioClip>("pokemon/pikachu5");
        }
        if (visualObject == null)
        {
            if (transform.childCount > 0)
                visualObject = transform.GetChild(0).gameObject;
            else
                visualObject = this.gameObject;
        }
        originalScale = visualObject.transform.localScale;
        
        // Cargar el prefab de electricidad si no está asignado
        if (electricityEffectPrefab == null)
        {
            // Intentar diferentes rutas para cargar el prefab
            electricityEffectPrefab = Resources.Load<GameObject>("GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Electricity_01");
            if (electricityEffectPrefab == null)
            {
                electricityEffectPrefab = Resources.Load<GameObject>("vfx_Electricity_01");
            }
            if (electricityEffectPrefab == null)
            {
                // Buscar en Assets si no está en Resources
                electricityEffectPrefab = FindPrefabInAssets("vfx_Electricity_01");
            }
            if (electricityEffectPrefab == null)
            {
                // Cargar directamente desde la ruta completa
                electricityEffectPrefab = FindPrefabInAssets("vfx_Electricity_01.prefab");
            }
        }
        
        if (electricityEffectPrefab == null)
        {
            Debug.LogError("No se pudo cargar el prefab vfx_Electricity_01. Por favor, asigna manualmente el prefab en el Inspector.");
        }
        else
        {
            Debug.Log("Prefab de electricidad cargado correctamente: " + electricityEffectPrefab.name);
        }
    }

    private void Update()
    {
        if (isActive && visualObject != null)
        {
            visualObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOnCooldown) return;
        Player player = FindPlayerInHierarchy(other.gameObject);
        if (player != null)
        {
            ApplySlowEffect(player);
        }
    }

    private Player FindPlayerInHierarchy(GameObject obj)
    {
        Player player = obj.GetComponent<Player>();
        if (player != null) return player;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            player = parent.GetComponent<Player>();
            if (player != null) return player;
            parent = parent.parent;
        }
        return null;
    }

    private void ApplySlowEffect(Player player)
    {
        CarController carController = player.GetComponent<CarController>();
        if (carController == null) return;
        if (carController.IsBoosted()) return; // No aplicar si ya está bajo otro efecto
        
        // Aplicar efecto de lentitud
        carController.ApplyBoost(slowMultiplier, 1.0f); // No cambiar steer
        
        // Forzar invisibilidad de ruedas
        carController.ForceWheelsInvisible();
        
        // Cambiar color del carro a amarillo
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
                renderers[i].material.color = paralysisColor;
            }
        }
        
        // Aplicar efecto eléctrico al carro
        ApplyElectricityEffect(player.transform);
        
        // Efectos visuales
        if (effectParticles != null) effectParticles.Play();
        
        // Sonido
        if (audioSource != null && pikachuSound != null)
        {
            audioSource.PlayOneShot(pikachuSound);
        }
        
        // Animación de desaparición y respawn
        StartCoroutine(DisappearAndRespawn());
        
        // Corrutina para remover el efecto y restaurar color
        StartCoroutine(RemoveSlowAfterDuration(carController, renderers, originalColors, player.transform));
        
        // Cooldown para evitar triggers múltiples
        StartCoroutine(CooldownCoroutine());
    }

    private void ApplyElectricityEffect(Transform carTransform)
    {
        if (electricityEffectPrefab == null)
        {
            Debug.LogWarning("Prefab de efecto eléctrico no asignado en PokemonPikachu");
            return;
        }

        // Destruir efecto anterior si existe
        if (currentElectricityEffect != null)
        {
            Destroy(currentElectricityEffect);
        }

        // Instanciar el efecto eléctrico en el carro
        Vector3 effectPosition = carTransform.position; // Posición exacta del carro
        currentElectricityEffect = Instantiate(electricityEffectPrefab, effectPosition, Quaternion.identity);
        
        // Hacer que el efecto siga al carro
        currentElectricityEffect.transform.SetParent(carTransform);
        
        // Configurar escala y posición específicas
        currentElectricityEffect.transform.localScale = new Vector3(1f, 1f, 1f);
        currentElectricityEffect.transform.localPosition = Vector3.zero; // Posición exacta en el centro del carro
        
        if (enableDebug)
        {
            Debug.Log("Efecto eléctrico aplicado al carro: " + carTransform.name + " con escala (20,20,20) y posición (0,0,0)");
        }
    }

    private void RemoveElectricityEffect()
    {
        if (currentElectricityEffect != null)
        {
            Destroy(currentElectricityEffect);
            currentElectricityEffect = null;
            
            if (enableDebug)
            {
                Debug.Log("Efecto eléctrico removido del carro");
            }
        }
    }

    private IEnumerator DisappearAndRespawn()
    {
        isOnCooldown = true;
        isActive = false;
        if (myCollider != null) myCollider.enabled = false;
        
        // Animación de desaparición
        if (animator != null)
        {
            animator.SetTrigger("Disappear");
        }
        else if (visualObject != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 3f;
                visualObject.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                yield return null;
            }
            visualObject.transform.localScale = Vector3.zero;
        }
        else
        {
            gameObject.SetActive(false);
        }
        
        // Esperar respawn
        yield return new WaitForSeconds(respawnTime);
        
        // Animación de reaparición
        if (animator != null)
        {
            animator.SetTrigger("Respawn");
        }
        else if (visualObject != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 3f;
                visualObject.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                yield return null;
            }
            visualObject.transform.localScale = originalScale;
        }
        else
        {
            gameObject.SetActive(true);
        }
        
        if (myCollider != null) myCollider.enabled = true;
        isOnCooldown = false;
        isActive = true;
    }

    private IEnumerator RemoveSlowAfterDuration(CarController carController, Renderer[] renderers, Color[] originalColors, Transform carTransform)
    {
        yield return new WaitForSeconds(effectDuration);
        
        carController.RemoveBoost();
        
        // Restaurar color original
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = originalColors[i];
            }
        }
        
        // Remover efecto eléctrico
        RemoveElectricityEffect();
    }

    private IEnumerator CooldownCoroutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }

    // Método para buscar prefab en Assets
    private GameObject FindPrefabInAssets(string prefabName)
    {
        #if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets(prefabName);
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(prefabName))
            {
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    Debug.Log("Prefab encontrado en: " + path);
                    return prefab;
                }
            }
        }
        
        // Buscar específicamente en la carpeta GabrielAguiarProductions
        string specificPath = "Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/" + prefabName;
        GameObject specificPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(specificPath);
        if (specificPrefab != null)
        {
            Debug.Log("Prefab encontrado en ruta específica: " + specificPath);
            return specificPrefab;
        }
        #endif
        return null;
    }
} 