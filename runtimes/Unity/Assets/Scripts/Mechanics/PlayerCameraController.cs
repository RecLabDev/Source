using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aby.Unity
{
    /// <summary>
    /// TODO
    /// </summary>
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField]
        public Transform focusPoint;

        /// <summary>
        /// TODO
        /// </summary>
        void Awake()
        {
            //..
        }

        /// <summary>
        /// TODO
        /// </summary>
        void Start()
        {
            //..
        }

        /// <summary>
        /// TODO
        /// </summary>
        void Update()
        {
            SyncPosition();
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void SyncPosition()
        {
            if (focusPoint == null) return;
            transform.position = focusPoint.position;
            transform.rotation = focusPoint.rotation;
        }
    }
}
