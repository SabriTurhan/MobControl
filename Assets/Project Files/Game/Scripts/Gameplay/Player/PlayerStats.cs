using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquadCommand
{
    [System.Serializable]
    public class PlayerStats
    {
        [Header("Use Local Overrides")]
        [InfoBox("This values override the upgrades system if enabled")]
        [SerializeField] FloatToggle damageOverride;
        [SerializeField] FloatToggle fireRateOverride;
        [SerializeField] FloatToggle healthOverride;

        [Space]
        [SerializeField] float bullerRange;

        [Header("Min stat values")]
        [SerializeField, Min(0f)] float minDamage = 0.1f;
        [SerializeField, Min(0f)] float minFireRate = 0.1f;
        [SerializeField, Min(0f)] float minRange = 0.1f;

        // Dustwater: the upgrade panel is replaced by town buildings (Armory, Workshop, Barracks, Watchtower),
        // so the first upgrade stage is the base value and buildings multiply it
        public float Damage => damageOverride.Handle(damageUpgrade.GetStage(0).Value) * Campaign.DamageMultiplier;
        public float FireRate => fireRateOverride.Handle(fireRateUpgrade.GetStage(0).Value) * Campaign.FireRateMultiplier;
        public float Health => healthOverride.Handle(healthUpgrade.GetStage(0).Value) * Campaign.HealthMultiplier;

        public float BulletRange => bullerRange * Campaign.RangeMultiplier;

        public float MinDamage => minDamage;
        public float MinFireRate => minFireRate;
        public float MinRange => minRange;

        FloatUpgrade damageUpgrade;
        FloatUpgrade fireRateUpgrade;
        FloatUpgrade healthUpgrade;

        public void Init()
        {
            damageUpgrade = UpgradesController.GetUpgrade<FloatUpgrade>(UpgradeType.Damage);
            fireRateUpgrade = UpgradesController.GetUpgrade<FloatUpgrade>(UpgradeType.FireRate);
            healthUpgrade = UpgradesController.GetUpgrade<FloatUpgrade>(UpgradeType.PlayerHealth);
        }
    }
}
