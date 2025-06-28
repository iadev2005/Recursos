using System.Collections;
using UnityEngine;

public class SpeedBooster : MonoBehaviour
{
    [Header("Configuración del Booster")]
    public float boostMultiplier = 2.0f;        // Multiplicador de velocidad
    public float boostDuration = 3.0f;          // Duración del boost en segundos
    public float boostForce = 5000f;            // Fuerza adicional del boost
    public float respawnTime = 5.0f;            // Tiempo para reaparecer el hongo
    
    [Header("Efectos Visuales y Sonido")]
    public ParticleSystem boostEffect;          // Efecto de partículas
    public AudioClip boostSound;                // Sonido del hongo
    public AudioSource audioSource;             // AudioSource para reproducir el sonido
    public Animator animator;                   // Animator para animación de desaparición/reaparición
    public GameObject visualObject;             // El objeto visual del hongo (para escalar/desactivar)
    
    [Header("Configuración Avanzada")]
    public bool canStackBoosts = false;         // Si se pueden acumular boosts
    public float cooldownTime = 1.0f;           // Tiempo de cooldown entre usos
    public bool enableDebug = false;            // Activar mensajes de debug
    
    [Header("Animación Visual")]
    public float rotationSpeed = 90f;           // grados por segundo
    private bool isActive = true;
    
    private bool isOnCooldown = false;
    private Collider myCollider;
    private Vector3 originalScale;
    
    private void Start()
    {
        myCollider = GetComponent<Collider>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (boostSound == null)
        {
            // Intentar cargar el sonido por defecto si no está asignado
            boostSound = Resources.Load<AudioClip>("items/SE_ITM_BIG_KINOKO_USE");
        }
        if (visualObject == null)
        {
            if (transform.childCount > 0)
                visualObject = transform.GetChild(0).gameObject;
            else
                visualObject = this.gameObject;
        }
        // Guardar la escala original
        originalScale = visualObject.transform.localScale;
        
        if (enableDebug)
        {
            Debug.Log($"SpeedBooster '{gameObject.name}' inicializado en posición {transform.position}");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isOnCooldown) return;
        Player player = FindPlayerInHierarchy(other.gameObject);
        if (player != null)
        {
            ApplySpeedBoost(player);
        }
    }
    
    // Método para buscar el Player en la jerarquía
    private Player FindPlayerInHierarchy(GameObject obj)
    {
        // Buscar en el objeto actual
        Player player = obj.GetComponent<Player>();
        if (player != null)
        {
            return player;
        }
        
        // Buscar en el objeto padre
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            player = parent.GetComponent<Player>();
            if (player != null)
            {
                return player;
            }
            parent = parent.parent;
        }
        
        return null;
    }
    
    private void ApplySpeedBoost(Player player)
    {
        // Obtener el CarController del jugador
        CarController carController = player.GetComponent<CarController>();
        if (carController == null) 
        {
            if (enableDebug)
            {
                Debug.LogError("No se encontró CarController en el player");
            }
            return;
        }
        
        // Verificar si ya está en boost (si no se pueden acumular)
        if (!canStackBoosts && carController.IsBoosted())
        {
            if (enableDebug)
            {
                Debug.Log("Carro ya está en boost y no se permiten acumulaciones");
            }
            return;
        }
        
        if (enableDebug)
        {
            Debug.Log($"Aplicando boost con multiplicador: {boostMultiplier}");
        }
        
        // Aplicar el boost usando el nuevo método
        carController.ApplyBoost(boostMultiplier);
        
        // Forzar invisibilidad de ruedas
        carController.ForceWheelsInvisible();
        
        // Aplicar fuerza adicional al Rigidbody
        Rigidbody carBody = player.GetComponent<Rigidbody>();
        if (carBody != null)
        {
            Vector3 boostDirection = carBody.transform.forward;
            carBody.AddForce(boostDirection * boostForce, ForceMode.Impulse);
            if (enableDebug)
            {
                Debug.Log($"Fuerza aplicada: {boostDirection * boostForce}");
            }
        }
        
        // Activar efectos visuales del carro
        if (boostEffect != null) boostEffect.Play();
        
        // Sonido del hongo
        if (audioSource != null && boostSound != null)
        {
            Debug.Log("Reproduciendo sonido del hongo");
            audioSource.PlayOneShot(boostSound);
        }
        else
        {
            Debug.LogWarning("No se encontró audioSource o boostSound");
        }
        
        // Animación de desaparición y respawn
        StartCoroutine(DisappearAndRespawn());
        
        // Corrutina para remover el boost
        StartCoroutine(RemoveBoostAfterDuration(carController));
        
        // Cooldown para evitar triggers múltiples
        StartCoroutine(CooldownCoroutine());
    }
    
    private void Update()
    {
        // Rotar el visualObject si está activo
        if (isActive && visualObject != null)
        {
            visualObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
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
    
    private IEnumerator RemoveBoostAfterDuration(CarController carController)
    {
        if (enableDebug)
        {
            Debug.Log($"Esperando {boostDuration} segundos para remover boost");
        }
        
        yield return new WaitForSeconds(boostDuration);
        
        // Remover boost
        carController.RemoveBoost();
        
        if (enableDebug)
        {
            Debug.Log("Boost removido después de la duración");
        }
    }
    
    private IEnumerator CooldownCoroutine()
    {
        isOnCooldown = true;
        if (enableDebug)
        {
            Debug.Log($"Cooldown iniciado por {cooldownTime} segundos");
        }
        
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
        
        if (enableDebug)
        {
            Debug.Log("Cooldown terminado, booster listo para usar");
        }
    }
    
    // Método para cambiar configuración desde el editor
    public void SetBoostSettings(float multiplier, float duration, float force)
    {
        boostMultiplier = multiplier;
        boostDuration = duration;
        boostForce = force;
    }
    
    // Método para crear un booster desde código
    public static SpeedBooster CreateSpeedBooster(Vector3 position, float multiplier = 2.0f, float duration = 3.0f)
    {
        GameObject boosterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boosterObject.name = "SpeedBooster";
        boosterObject.transform.position = position;
        
        // Configurar el collider como trigger
        Collider collider = boosterObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Agregar el script SpeedBooster
        SpeedBooster speedBooster = boosterObject.AddComponent<SpeedBooster>();
        speedBooster.boostMultiplier = multiplier;
        speedBooster.boostDuration = duration;
        
        return speedBooster;
    }
    
    // Método para debugging visual
    private void OnDrawGizmos()
    {
        if (enableDebug)
        {
            Gizmos.color = isOnCooldown ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
} 