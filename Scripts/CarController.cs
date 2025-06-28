using UnityEngine;

public class CarController : MonoBehaviour
{
    public float maxSteer, maxThrottle, steer, throttle;
    public Transform centerOfMass;
    private Rigidbody carBody;
    private Wheel[] wheels;

    // Drift manual
    [Header("Drift Manual")]
    public KeyCode driftKey = KeyCode.LeftShift;
    public string driftButton = "Drift"; // Configurable en Input Manager
    public bool isDrifting = false;
    public float driftSteerMultiplier = 1.5f;
    public float driftGripMultiplier = 0.5f;
    public float driftBoostForce = 1200f;
    public float minDriftTimeForBoost = 0.7f;
    public ParticleSystem[] driftParticles;
    public AudioSource driftAudio;
    private float driftTime = 0f;
    private float[] originalSideFriction;

    // Variables para el sistema de boost
    private float originalMaxThrottle;
    private float originalMaxSteer;
    private bool isBoosted = false;

    private void Start()
    {
        maxThrottle = 2000f;
        maxSteer = 17f;
        originalMaxThrottle = maxThrottle;
        originalMaxSteer = maxSteer;
        wheels = GetComponentsInChildren<Wheel>();
        carBody = GetComponent<Rigidbody>();
        if (centerOfMass != null)
        {
            carBody.centerOfMass = centerOfMass.localPosition;
        }
        // Guardar fricción lateral original de las ruedas
        originalSideFriction = new float[wheels.Length];
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].TryGetComponent<WheelCollider>(out var wc))
                originalSideFriction[i] = wc.sidewaysFriction.stiffness;
            else
                originalSideFriction[i] = 1f;
        }
    }

    private void Update()
    {
        // DRIFT INPUT
        bool driftInput = Input.GetKey(driftKey) || Input.GetButton(driftButton);
        if (driftInput && !isDrifting)
        {
            StartDrift();
        }
        else if (!driftInput && isDrifting)
        {
            EndDrift();
        }

        if (isDrifting)
        {
            driftTime += Time.deltaTime;
        }

        foreach (Wheel wheel in wheels)
        {
            wheel.torque = throttle * maxThrottle;
            wheel.steerAngle = steer * maxSteer;
        }
    }

    public void StartDrift()
    {
        isDrifting = true;
        maxSteer = originalMaxSteer * driftSteerMultiplier;
        // Reducir grip lateral
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].TryGetComponent<WheelCollider>(out var wc))
            {
                var friction = wc.sidewaysFriction;
                friction.stiffness = originalSideFriction[i] * driftGripMultiplier;
                wc.sidewaysFriction = friction;
            }
        }
        // Activar partículas
        foreach (var p in driftParticles) if (p != null) p.Play();
        // Sonido
        if (driftAudio != null) driftAudio.Play();
        driftTime = 0f;
    }

    public void EndDrift()
    {
        isDrifting = false;
        maxSteer = originalMaxSteer;
        // Restaurar grip lateral
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].TryGetComponent<WheelCollider>(out var wc))
            {
                var friction = wc.sidewaysFriction;
                friction.stiffness = originalSideFriction[i];
                wc.sidewaysFriction = friction;
            }
        }
        // Parar partículas
        foreach (var p in driftParticles) if (p != null) p.Stop();
        // Sonido
        if (driftAudio != null) driftAudio.Stop();
        // BOOST AL SALIR DEL DRIFT
        if (driftTime > minDriftTimeForBoost)
        {
            carBody.AddForce(transform.forward * driftBoostForce, ForceMode.Impulse);
        }
        driftTime = 0f;
    }

    // Métodos para el sistema de boost
    public void ApplyBoost(float multiplier, float steerMultiplier = 0.8f)
    {
        if (!isBoosted)
        {
            isBoosted = true;
            maxThrottle = originalMaxThrottle * multiplier;
            maxSteer = originalMaxSteer * steerMultiplier;
            Debug.Log($"Boost aplicado! Throttle: {maxThrottle}, Steer: {maxSteer}");
        }
    }
    public void RemoveBoost()
    {
        if (isBoosted)
        {
            isBoosted = false;
            maxThrottle = originalMaxThrottle;
            maxSteer = originalMaxSteer;
            Debug.Log("Boost removido. Valores restaurados.");
        }
    }
    public bool IsBoosted()
    {
        return isBoosted;
    }
    public float GetOriginalMaxThrottle()
    {
        return originalMaxThrottle;
    }
    public float GetOriginalMaxSteer()
    {
        return originalMaxSteer;
    }

    // Fuerza la invisibilidad de todas las ruedas (truco visual)
    public void ForceWheelsInvisible()
    {
        foreach (Wheel wheel in wheels)
        {
            var meshRenderer = wheel.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.enabled = false;
        }
    }
}