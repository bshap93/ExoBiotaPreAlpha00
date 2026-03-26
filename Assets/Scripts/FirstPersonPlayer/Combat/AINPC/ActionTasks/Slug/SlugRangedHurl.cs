using FirstPersonPlayer.Combat.AINPC.ActionTasks.SpittingVine;
using NewScript;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Yarn.Compiler;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks.Slug
{
    [Category("AttackMoves")]
    public class SlugRangedHurl : ActionTask
    {
        public BBParameter<GameObject> MuzzleFlashPrefab;
        [Tooltip("Layer mask for the projectile")]
        public LayerMask ProjectileLayerMask = -1;
        [RequiredField] [Tooltip("The projectile prefab to spawn")]
        public BBParameter<GameObject> ProjectilePrefab;
        [Tooltip("Projectile speed (units per second)")]
        public BBParameter<float> ProjectileSpeed = 15f;
        public BBParameter<Transform> ProjectileOrigin;
        
        [RequiredField] [Tooltip("The target to shoot at")]
        public BBParameter<GameObject> ShootTarget;
        protected override void OnExecute()
        {
            if (ProjectilePrefab.value == null || ShootTarget.value == null)
            {
                EndAction(false);
                return;
            }
            
            var creaturePosition = ProjectileOrigin.value.position;
            
            var shootDirection = (ShootTarget.value.transform.position - creaturePosition).normalized;
            
            if (MuzzleFlashPrefab.value != null)
                Object.Instantiate( MuzzleFlashPrefab.value, creaturePosition,
                    Quaternion.LookRotation(shootDirection));

            var projectile = Object.Instantiate(
                ProjectilePrefab.value, creaturePosition, Quaternion.LookRotation(shootDirection));
            
            ConfigureProjectile(projectile, shootDirection);
            
            // base.OnExecute();
            EndAction(true);
        }
        
        void ConfigureProjectile(GameObject projectile, Vector3 direction)
        {
            var rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = direction * ProjectileSpeed.value;
                Debug.Log(
                    $"Rigidbody configured: linearVelocity = {rb.linearVelocity}, magnitude = {rb.linearVelocity.magnitude}");
#else
                rb.velocity = direction * ProjectileSpeed.value;
                Debug.Log($"Rigidbody configured: velocity = {rb.velocity}, magnitude = {rb.velocity.magnitude}");
#endif

            }
            
            else
            {
                Debug.LogWarning(
                    $"Slug Projectile: Projectile prefab '{ProjectilePrefab.value.name}' has no Rigidbody! Add a Rigidbody component for physics-based movement.");
            }
            
            // Setup projectile script if using a custom component
            var projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null) projectileScript.speed = ProjectileSpeed.value;

            // Set layer if needed
            if (ProjectileLayerMask != -1) projectile.layer = GetLayerFromMask(ProjectileLayerMask);

            Debug.Log(
                $"Projectile fired at {ProjectileSpeed.value} m/s toward {ShootTarget.value.name}, direction: {direction}");
        }
        
        int GetLayerFromMask(LayerMask mask)
        {
            var layerNumber = 0;
            var layer = mask.value;
            while (layer > 1)
            {
                layer = layer >> 1;
                layerNumber++;
            }

            return layerNumber;
        }
    }
    
    
}
