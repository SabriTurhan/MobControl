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

            if (controllingMovement)
            {
                controllingMovement = false;
                engaged = false;
                MovementManager.AdvanceMultiplier = 1f;
            }
        }

        [Tooltip("The squad stops this far in front of the boss and the fight plays out at a standoff")]
        [SerializeField] float engageDistance = 12f;

        private bool engaged;
        private bool controllingMovement;

        private void Update()
        {
            if (enemy == null || enemy.Health == null || !GameController.IsGameplayActive) return;

            PlayerBehavior player = PlayerBehavior.Instance;
            if (player == null) return;

            bool alive = !enemy.Health.IsDepleted;
            float distance = transform.position.z - player.transform.position.z;

            if (!engaged && alive && distance > 0f && distance < engageDistance)
            {
                engaged = true;
                controllingMovement = true;
                player.SetIsMovingForward(false);
                enemy.StartShooting();
            }
            else if (engaged && !alive)
            {
                engaged = false;
                if (LevelController.LevelData != null && LevelController.LevelData.MovementMode == MovementModeType.Classic)
                    player.SetIsMovingForward(true);
            }

            if (controllingMovement)
            {
                float target = engaged ? 0f : 1f;
                MovementManager.AdvanceMultiplier = Mathf.MoveTowards(MovementManager.AdvanceMultiplier, target, Time.deltaTime * 2.5f);
                if (!engaged && MovementManager.AdvanceMultiplier >= 1f) controllingMovement = false;
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
