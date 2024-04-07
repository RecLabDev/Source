using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Theta
{
    public class Trap : MonoBehaviour
    {
        //kill the player if they touch the trap
        public void TrapKill()
        {
            Schedule<PlayerDeath>();
        }
    }

    //got this from the enemy controller to see if i can change it to work with traps.
    //need lorrnes help for fixing it
    /*void OnCollisionEnter2D(Collision2D collision)
    {
        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            var ev = Schedule<PlayerEnemyCollision>();
            ev.player = player;
            ev.trap = this;
        }
    }*/
}