using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace Theta
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CBootstrapOptions
    {
        public int intValue;
        public string stringValue;
        public IntPtr arrayValue;
        public CJsRuntimeConfig nestedValue;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CJsRuntimeConfig
    {
        public long longValue;
    }
}
