using System.Collections.Generic;
using UnityEngine;

namespace SquadCommand
{
    public class BulletBehavior : MonoBehaviour
    {
        private const string HIT_PARTICLE_NAME = "Hit";
        private const string EXPLOSION_PARTICLE_NAME = "Grenade Explosion";

        [Tooltip("Optional child that flies in an arc (grenades)")]
        [SerializeField] Transform arcVisual;
        [SerializeField] float arcHeight = 0f;

        private int pierceLeft;
        private ITargetable lastPiercedTarget;

        [SerializeField] TrailRenderer trailRenderer;

        protected float damage;
        protected float speed;

        protected float selfDestroyDistance;
        protected float distanceTraveled = 0;

        protected bool isPlayerShooting;
        protected bool applyMagnet;

        private float bulletStartX;
        private float bulletFinalX;

        public GunBehavior Gun { get; private set; }

        public void Init(GunBehavior gun, float damage, float speed, float selfDestroyDistance, bool isPlayerShooting, bool applyMagnet)
        {
            Gun = gun;
            this.damage = damage;
            this.speed = speed;

            this.isPlayerShooting = isPlayerShooting;
            this.applyMagnet = applyMagnet;

            this.selfDestroyDistance = selfDestroyDistance;

            if (selfDestroyDistance == 0)
                selfDestroyDistance = -1f;

            if (applyMagnet)
            {
                bulletStartX = gameObject.transform.position.x;
                bulletFinalX = PlayerBehavior.Instance.CurrentSideShift + (gameObject.transform.position.x - PlayerBehavior.Instance.CurrentSideShift) * 0.1f;
            }

            distanceTraveled = 0;

            pierceLeft = isPlayerShooting && gun != null && gun.GunData != null ? gun.GunData.Pierce : 0;
            lastPiercedTarget = null;

            trailRenderer.gameObject.SetActive(true);
        }

        protected virtual void Update()
        {
            // moving forward
            transform.position += transform.forward * speed * Time.deltaTime;

            if (selfDestroyDistance != -1)
            {
                distanceTraveled += speed * Time.fixedDeltaTime;

                if (applyMagnet)
                {
                    float currentPosX = Mathf.Lerp(bulletStartX, bulletFinalX, distanceTraveled / selfDestroyDistance);

                    // moving sideways - bullet magnet effect
                    transform.position = transform.position.SetX(currentPosX);
                }

                if (arcVisual != null)
                {
                    float t = Mathf.Clamp01(distanceTraveled / selfDestroyDistance);
                    arcVisual.localPosition = Vector3.up * (arcHeight * 4f * t * (1f - t));
                }

                if (distanceTraveled >= selfDestroyDistance)
                {
                    if (IsSplash) Explode(null);

                    SelfDestroy();
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            ITargetable target = other.GetComponent<ITargetable>();

            if (target != null)
            {
                if (lastPiercedTarget != null && ReferenceEquals(target, lastPiercedTarget))
                    return;

                if (isPlayerShooting)
                {
                    target.GetHit(damage, transform.position, Gun.GunData.DamageMultiplier);

                    if (IsSplash)
                    {
                        Explode(target);
                    }
                    else if (pierceLeft > 0 && target is EnemyBehavior)
                    {
                        pierceLeft--;
                        lastPiercedTarget = target;
                        return;
                    }
                }

                SelfDestroy();
            }
            else
            {
                if (!isPlayerShooting)
                {
                    CharacterBehavior character = other.GetComponent<CharacterBehavior>();

                    if (character != null)
                    {
                        character.TakeDamage(damage, transform.position);
                        SelfDestroy();
                    }
                }
            }

            if (gameObject.activeSelf)
            {
                BulletBehavior otherBullet = other.GetComponent<BulletBehavior>();

                // aimed sniper/boss shots go through the squad's fire
                bool unblockable = (Gun != null && Gun.GunData != null && Gun.GunData.Unblockable) || (otherBullet != null && otherBullet.Gun != null && otherBullet.Gun.GunData != null && otherBullet.Gun.GunData.Unblockable);
                if (otherBullet != null && otherBullet.isPlayerShooting != isPlayerShooting && !unblockable)
                {
                    ParticleCase particleCase = ParticlesController.PlayParticle(HIT_PARTICLE_NAME);

                    particleCase.SetPosition(transform.position);

                    SelfDestroy();
                }

                if (other.gameObject.layer == PhysicsHelper.LAYER_OBSTACLE)
                {
                    SelfDestroy();
                }
            }
        }

        private bool IsSplash => isPlayerShooting && Gun != null && Gun.GunData != null && Gun.GunData.SplashRadius > 0;

        private static readonly Collider[] splashHits = new Collider[32];

        private void Explode(ITargetable directTarget)
        {
            float radius = Gun.GunData.SplashRadius;
            Vector3 center = transform.position;

            int count = Physics.OverlapSphereNonAlloc(center, radius, splashHits, ~0, QueryTriggerInteraction.Collide);
            List<ITargetable> damaged = new List<ITargetable>();
            for (int i = 0; i < count; i++)
            {
                ITargetable splashTarget = splashHits[i].GetComponent<ITargetable>();
                if (splashTarget == null || ReferenceEquals(splashTarget, directTarget) || damaged.Contains(splashTarget)) continue;

                // Gates are only changed by direct hits
                if (!(splashTarget is EnemyBehavior) && !(splashTarget is ObstacleBehavior)) continue;

                damaged.Add(splashTarget);
                splashTarget.GetHit(damage * 0.6f, splashHits[i].transform.position, Gun.GunData.DamageMultiplier);
            }

            if (ParticlesController.HasParticle(EXPLOSION_PARTICLE_NAME))
                ParticlesController.PlayParticle(EXPLOSION_PARTICLE_NAME).SetPosition(center);
        }

        public void SelfDestroy()
        {
            // Disable bullet
            trailRenderer.Clear();
            gameObject.SetActive(false);
            trailRenderer.gameObject.SetActive(false);
        }
    }
}