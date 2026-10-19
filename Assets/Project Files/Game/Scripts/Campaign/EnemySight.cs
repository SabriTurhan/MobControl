using UnityEngine;

namespace SquadCommand
{
    /// <summary>
    /// Long trigger in front of an enemy (bandit snipers, bosses): starts shooting as soon as a squad member
    /// enters it, before the squad's own target detector reaches the enemy. Lives on the Pickable layer,
    /// which only collides with characters, so it never stops bullets.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EnemySight : MonoBehaviour
    {
        [SerializeField] EnemyBehavior enemy;

        private int charactersInside;

        public void SetEnemy(EnemyBehavior enemy)
        {
            this.enemy = enemy;
        }

        private void OnEnable()
        {
            charactersInside = 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (enemy == null || other.GetComponent<CharacterBehavior>() == null) return;

            charactersInside++;
            if (charactersInside == 1 && enemy.Health != null && !enemy.Health.IsDepleted)
                enemy.StartShooting();
        }

        private void OnTriggerExit(Collider other)
        {
            if (enemy == null || other.GetComponent<CharacterBehavior>() == null) return;

            charactersInside = Mathf.Max(0, charactersInside - 1);
        }
    }
}
