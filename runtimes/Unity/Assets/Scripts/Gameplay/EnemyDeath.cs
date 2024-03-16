using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the health component on an enemy has a hitpoint value of  0.
    /// </summary>
    /// <typeparam name="EnemyDeath"></typeparam>
    public class EnemyDeath : Simulation.Event<EnemyDeath>
    {
        public EnemyController enemy;

        public override void Execute()
        {
            enemy.control.enabled = false;

            // TODO: Should set enemy to "dead" state until re-enabled.
            enemy._collider.enabled = false;

            if (enemy._audio && enemy.ouch)
            {
                enemy._audio.PlayOneShot(enemy.ouch);
            }
        }
    }
}