using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEditor;

namespace Aby.Unity
{
    /// <summary>
    /// TODO
    /// </summary>
    public static class AbyUnitySettings
    {
        /// <summary>
        /// TODO
        /// </summary>
        public static bool ShouldShowBillboard
        {
            get => EditorPrefs.GetBool("ShowBillboard", true);
            set => EditorPrefs.SetBool("ShowBillboard", value);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static bool HasShownBillboard
        {
            get => SessionState.GetBool("HasShownBillboard", false);
            set => SessionState.SetBool("HasShownBillboard", value);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public const string ABY_SETTINGS_LABEL = "Aby";

        //--
        /// <summary>
        /// TODO
        /// </summary>
        [InitializeOnLoadMethod()]
        private static void Billboard()
        {
            if (ShouldShowBillboard && !HasShownBillboard)
            {
                Debug.LogFormat("Aby SDK Assembly Name: {0}", Aby.SDK.Config.AssemblyName);
                HasShownBillboard = true;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns>An instance of SettingsProvider (for Unity Editor)</returns>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Preferences/Aby", SettingsScope.User)
            {
                label = ABY_SETTINGS_LABEL,
                keywords = new HashSet<string>(new[] { "Theta", "Aby" }),
                guiHandler = ctx =>
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Show Billboard", GUILayout.Width(200));

                    var showBillbaordPref = EditorGUILayout.Toggle(ShouldShowBillboard);

                    EditorGUILayout.EndHorizontal();

                    if (GUI.changed)
                    {
                        ShouldShowBillboard = showBillbaordPref;
                    }
                },
            };
        }
    }
}
