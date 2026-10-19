using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquadCommand
{
    public class GunBehavior : MonoBehaviour
    {
        [SerializeField] Transform shootPointTransform;
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] bool magnetBullets;

        [Space]
        [SerializeField] ParticleSystem muzzleParticle;

        public GunData GunData { get; private set; }

        private PoolGeneric<BulletBehavior> bulletPool;
        private float BulletRange { get; set; }
        public float RangeMultiplier { get; set; } = 1f;

        private void Awake()
        {
            string booletPoolName = "PlayerBullet_" + bulletPrefab.name;
            if (!PoolManager.PoolExists(booletPoolName))
            {
                bulletPool = new PoolGeneric<BulletBehavior>(bulletPrefab, booletPoolName);
            }
            else
            {
                bulletPool = (PoolGeneric<BulletBehavior>)PoolManager.GetPoolByName(booletPoolName);
            }
        }

        private void OnDestroy()
        {
            bulletPool?.Destroy();
        }

        public void RecalculateBulletRange(List<StatModifier> modifiers, float minRange)
        {
            BulletRange = GunData.BulletRangeMultiplier;

            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier statModifier = modifiers[i];

                BulletRange = statModifier.ApplyValue(BulletRange);
            }

            BulletRange *= RangeMultiplier;

            if (BulletRange < minRange) BulletRange = minRange;
        }

        public void Shoot(float damage, bool isPlayerShooting)
        {
            for (int i = 0; i < GunData.BulletsCount; i++)
            {
                BulletBehavior bullet = bulletPool.GetPooledComponent();

                bullet.transform.position = shootPointTransform.position;
                bullet.transform.rotation = Quaternion.identity;
                bullet.transform.localScale = Vector3.one;

                float bulletDamage = damage;

                Vector3 direction = isPlayerShooting ? Vector3.forward : Vector3.back;

                if (!isPlayerShooting && GunData.AimAtSquad && PlayerBehavior.Instance != null)
                {
                    Vector3 toSquad = SquadCenter() - shootPointTransform.position;
                    toSquad.y = 0f;
                    if (toSquad.z < -0.5f) direction = toSquad.normalized;
                }

                float spread = GunData.Spread;
                float halfSpread = GunData.Spread / 2f;

                if (!Mathf.Approximately(spread, 0))
                {
                    float angle = Random.Range(-halfSpread, halfSpread);

                    if(i == 0)
                    {
                        angle *= 0.1f;
                    }

                    Quaternion rotation = Quaternion.Euler(0, angle, 0);
                    direction = rotation * direction;
                }

                bullet.transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction);

                bool applyMagnet = magnetBullets;

                // disable magnet for enemies
                if (!isPlayerShooting)
                    applyMagnet = false;

                bullet.Init(this, bulletDamage, GunData.BulletSpeed, BulletRange, isPlayerShooting, applyMagnet);

                if (muzzleParticle != null) muzzleParticle.Play();
            }
        }

        private static Vector3 SquadCenter()
        {
            PlayerBehavior player = PlayerBehavior.Instance;
            var characters = player.Characters;
            if (characters.Count == 0) return player.transform.position;

            Vector3 sum = Vector3.zero;
            for (int i = 0; i < characters.Count; i++) sum += characters[i].transform.position;
            return sum / characters.Count;
        }

        public void SetData(GunData data)
        {
            GunData = data;
        }
    }
}