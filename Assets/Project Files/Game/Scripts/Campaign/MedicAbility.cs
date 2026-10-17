using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Medic card: restores health to every squad member over time.
    /// </summary>
    public class MedicAbility : MonoBehaviour
    {
        private const float TICK = 0.5f;

        private CharacterBehavior character;
        private float healPerSecond;
        private float timer;

        public void Init(CharacterBehavior character, float healPerSecond)
        {
            this.character = character;
            this.healPerSecond = healPerSecond;
            timer = 0;
        }

        private void Update()
        {
            if (character == null || character.IsDead || character.Player == null || !GameController.IsGameplayActive)
                return;

            timer += Time.deltaTime;
            if (timer < TICK) return;
            timer -= TICK;

            var squad = character.Player.Characters;
            for (int i = 0; i < squad.Count; i++)
            {
                CharacterBehavior member = squad[i];
                if (member == null || member.IsDead) continue;
                if (member.Health.CurrentHealth >= member.Health.MaxHealth) continue;

                member.AddHealth(Mathf.Min(healPerSecond * TICK, member.Health.MaxHealth - member.Health.CurrentHealth));
            }
        }
    }
}
