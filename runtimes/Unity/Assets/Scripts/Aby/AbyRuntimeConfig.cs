using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEditor;

namespace Aby.Unity.Plugin
{
    /// <summary>
    /// TODO
    /// </summary>
    [CreateAssetMenu(fileName = "Aby Runtime Config", menuName = "Aby/Config")]
    public class AbyRuntimeConfig : ScriptableObject
    {
        public int servicePortNumber = 8080;
    }
}
