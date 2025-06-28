using UnityEngine;

public class BoosterTest : MonoBehaviour
{
    [Header("Configuración de Prueba")]
    public bool createTestBooster = true;
    public Vector3 boosterPosition = new Vector3(0, 0.5f, 10f);
    public KeyCode testBoostKey = KeyCode.B;
    
    private SpeedBooster testBooster;
    private Player testPlayer;
    
    private void Start()
    {
        if (createTestBooster)
        {
            CreateTestBooster();
        }
        
        // Buscar el player en la escena
        testPlayer = FindObjectOfType<Player>();
        if (testPlayer != null)
        {
            Debug.Log($"Player encontrado: {testPlayer.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("No se encontró ningún Player en la escena");
        }
    }
    
    private void Update()
    {
        // Tecla de prueba para activar boost manualmente
        if (Input.GetKeyDown(testBoostKey) && testPlayer != null)
        {
            TestBoostManually();
        }
    }
    
    private void CreateTestBooster()
    {
        // Crear objeto booster
        GameObject boosterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boosterObject.name = "TestBooster";
        boosterObject.transform.position = boosterPosition;
        boosterObject.transform.localScale = new Vector3(3f, 0.5f, 3f);
        
        // Configurar collider como trigger
        Collider collider = boosterObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Configurar material
        Renderer renderer = boosterObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.cyan;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.cyan * 0.5f);
            renderer.material = material;
        }
        
        // Agregar script SpeedBooster
        testBooster = boosterObject.AddComponent<SpeedBooster>();
        testBooster.boostMultiplier = 2.0f;
        testBooster.boostDuration = 3.0f;
        testBooster.boostForce = 5000f;
        testBooster.cooldownTime = 1.0f;
        testBooster.enableDebug = true;
        
        Debug.Log($"TestBooster creado en posición {boosterPosition}");
    }
    
    private void TestBoostManually()
    {
        if (testPlayer == null) return;
        
        CarController carController = testPlayer.GetComponent<CarController>();
        if (carController != null)
        {
            Debug.Log("Aplicando boost manualmente...");
            carController.ApplyBoost(2.0f);
            
            // Remover boost después de 3 segundos
            StartCoroutine(RemoveBoostAfterDelay(carController));
        }
        else
        {
            Debug.LogError("No se encontró CarController en el player");
        }
    }
    
    private System.Collections.IEnumerator RemoveBoostAfterDelay(CarController carController)
    {
        yield return new WaitForSeconds(3.0f);
        carController.RemoveBoost();
        Debug.Log("Boost manual removido");
    }
    
    [ContextMenu("Crear Booster de Prueba")]
    public void CreateTestBoosterManually()
    {
        CreateTestBooster();
    }
    
    [ContextMenu("Probar Boost Manualmente")]
    public void TestBoostManuallyFromMenu()
    {
        TestBoostManually();
    }
} 