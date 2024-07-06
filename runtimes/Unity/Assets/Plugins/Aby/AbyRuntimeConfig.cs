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
    public class AbyServiceConfig : ScriptableObject
    {
        /// <summary>
        /// TODO
        /// </summary>
        public string RootDirectory = ".";

        /// <summary>
        /// TODO
        /// </summary>
        public string MainModuleSpecifier = "./Assets/Scripts/Main.js";

        /// <summary>
        /// TODO
        /// </summary>
        public string DataDirectory = "./Data";

        /// <summary>
        /// TODO
        /// </summary>
        public string LogDirectory = "./Logs";

        /// <summary>
        /// TODO
        /// </summary>
        public string InspectorAddress = "127.0.0.1:9222";
    }
}
