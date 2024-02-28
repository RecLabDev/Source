using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEditor;

namespace Theta.Unity.Runtime
{
    /// <summary>
    /// TODO
    /// </summary>
    [CreateAssetMenu(fileName = "New Environment", menuName = "Aby/Environment")]
    public class AbyEnvConfig : ScriptableObject
    {
        public int servicePortNumber = 8080;
    }
}
