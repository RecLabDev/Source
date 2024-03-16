using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using static Platformer.Core.Simulation;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Platformer.Gameplay
{

    /// <summary>
    /// Fired when a Player collides with an Enemy.
    /// </summary>
    /// <typeparam name="EnemyCollision"></typeparam>
    public class PlayerEnemyCollision : Simulation.Event<PlayerEnemyCollision>
    {
        /// <summary>
        /// TODO
        /// </summary>
        public PlayerController player;

        /// <summary>
        /// TODO
        /// </summary>
        public EnemyController enemy;

        /// <summary>
        /// TODO
        /// </summary>
        private Vector3 relativeDirection => (enemy.transform.position - player.transform.position).normalized;

        /// <summary>
        /// TODO
        /// </summary>
        private PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        /// <summary>
        /// Player kills enemy if the player is facing the enemy and 
        /// </summary>
        public override void Execute()
        {
            var isPlayerFacingEnemy = Vector3.Dot(player.Direction, relativeDirection) < 0;
            var isEnemyFacingPlayer = Vector3.Dot(enemy.Direction, relativeDirection) > 0;

            if (isPlayerFacingEnemy && player.IsAttacking)
            {
                Schedule<EnemyDeath>().enemy = enemy;
            }
            else if (isEnemyFacingPlayer && enemy.IsAttacking)
            {
                Schedule<PlayerDeath>();
            }
        }
    }
}