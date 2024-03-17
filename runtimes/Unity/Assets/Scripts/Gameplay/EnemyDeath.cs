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

        //old stuff
        /*public EnemyController enemy;

        public override void Execute()
        {
            //get rid of this line and add triggers for the animation
            enemy._collider.enabled = false;
            enemy.control.enabled = false;
            if (enemy._audio && enemy.ouch)
                enemy._audio.PlayOneShot(enemy.ouch);
        }*/

        //new stuff to change enemy death animation
        public EnemyController enemy;
        public PlayerController player;

        public override void Execute()
        {
            // Call the Die method on the enemy which triggers the death animation and fades out the enemy.
            enemy.Die();
        }
    }
}