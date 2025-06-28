using UnityEngine;

public class EngineSound : MonoBehaviour
{

    public Rigidbody carBody;
    private AudioSource engineSound;

    void Start()
    { engineSound = GetComponent<AudioSource>(); }

    void LateUpdate()
    { engineSound.pitch = carBody.linearVelocity.magnitude / 50f; }

}