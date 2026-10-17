using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Marks a chapter boss: shows the big health bar while the boss is alive in the level.
    /// </summary>
    public class BossBehavior : MonoBehaviour
    {
        [SerializeField] string bossName = "BOSS";
        public string BossName => bossName;

        [SerializeField] EnemyBehavior enemy;
        public EnemyBehavior Enemy => enemy;

        public static BossBehavior Active { get; private set; }

        public static event SimpleCallback OnBossChanged;

        public void SetData(string bossName, EnemyBehavior enemy)
        {
            this.bossName = bossName;
            this.enemy = enemy;
        }

        private void OnEnable()
        {
            Active = this;
            OnBossChanged?.Invoke();
        }

        private void OnDisable()
        {
            if (Active == this)
            {
                Active = null;
                OnBossChanged?.Invoke();
            }
        }

        public float HealthFraction
        {
            get
            {
                if (enemy == null || enemy.Health == null || enemy.Health.MaxHealth <= 0) return 0f;

                return Mathf.Clamp01(enemy.Health.CurrentHealth / enemy.Health.MaxHealth);
            }
        }
    }
}
