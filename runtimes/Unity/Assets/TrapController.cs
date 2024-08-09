using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Aby
{
    public class TrapController : MonoBehaviour
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="collision"></param>
        void OnCollisionEnter2D(Collision2D collision)
        {
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                var ev = Schedule<PlayerTouchesTrap>();
                ev.player = player;
                ev.trap = this;
            }
        }
    }
}