using UnityEngine;

using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using static Platformer.Core.Simulation;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

using Aby;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when a Player collides with an Enemy.
    /// </summary>
    /// <typeparam name="PlayerSlashEnemy"></typeparam>
    public class PlayerSlashEnemy : Simulation.Event<PlayerSlashEnemy>
    {
        public EnemyController enemy;
        public PlayerController player;

        public override void Execute()
        {
            if (enemy.IsDead) return;
            Schedule<EnemyDeath>().enemy = enemy;
        }
    }

    /// <summary>
    /// Fired when a Player slashes a chest.
    /// </summary>
    /// <typeparam name="PlayerSlashChest"></typeparam>
    public class PlayerSlashChest : Simulation.Event<PlayerSlashChest>
    {
        public Chest chest;
        public PlayerController player;

        public override void Execute()
        {
            if (chest != null)
            {
                chest.OpenChest();
            }
        }
    }

    /// <summary>
    /// Fired when player collides with a Trap
    /// </summary>
    /// <typeparam name="PlayerTouchesTrap"></typeparam>
    public class PlayerTouchesTrap : Simulation.Event<PlayerTouchesTrap>
    {
        public PlayerController player;
        public TrapController trap;

        public override void Execute()
        {
            if (player != null)
            {
                Schedule<PlayerDeath>();
            }
        }
    }
    


    /// <summary>
    /// TODO
    /// </summary>
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
            if (enemy.IsDead) return;

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