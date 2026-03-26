using PhysicsHandlers.Triggers;
using UnityEngine;

public class AlertCollider : MonoBehaviour
{
    [SerializeField] CollisionAlertTrigger collisionAlertTrigger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("FirstPersonPlayer")) collisionAlertTrigger.TriggerAlert();
    }
}
