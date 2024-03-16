using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{

    /// <summary>
    /// Fired when a Player collides with an Enemy.
    /// </summary>
    /// <typeparam name="EnemyCollision"></typeparam>
    /// 

    public class PlayerSlashEnemy : Simulation.Event<PlayerSlashEnemy>
    {
        public EnemyController enemy;
        public PlayerController player;

        public override void Execute()
        {
            // Schedule an EnemyDeath event directly, bypassing health decrement.
            Schedule<EnemyDeath>().enemy = enemy;
        }
    }




    public class PlayerEnemyCollision : Simulation.Event<PlayerEnemyCollision>
    {
        public EnemyController enemy;
        public PlayerController player;

        //PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        /*public override void Execute()
        {
            var willHurtEnemy = player.Bounds.center.y >= enemy.Bounds.max.y;//possibly change to x

            if (willHurtEnemy)
            {
                var enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.Decrement();
                    if (!enemyHealth.IsAlive)
                    {
                        Schedule<EnemyDeath>().enemy = enemy;
                        player.Bounce(2);
                    }
                    else
                    {
                        player.Bounce(7);
                    }
                }
                else
                {
                    Schedule<EnemyDeath>().enemy = enemy;
                    player.Bounce(2);
                }
            }
            else
            {
                Schedule<PlayerDeath>();
            }
        }*/
        //what it should be
        public override void Execute()
        {
            if (!player.animator.GetBool("Attacking"))
            {
                Schedule<PlayerDeath>();
            }
        }
    }
}