using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text;

using UnityEngine;
using static UnityEngine.CullingGroup;

using UnityEditor;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

using Aby.SDK;
using Aby.Unity.Plugin;

namespace Aby.CStuff
{
    // TODO: Move this to the DotNet SDK.
    public class ConcurrentStringInterner : IDisposable
    {
        private ConcurrentDictionary<string, (string, GCHandle)> m_strings = new ConcurrentDictionary<string, (string, GCHandle)>();

        // Interns a string and returns its byte pointer
        public unsafe byte* InternAndReturnPointer(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var interned = m_strings.GetOrAdd(value, key =>
            {
                var bytes = Encoding.UTF8.GetBytes(key + "\0"); // Null-terminate for C compatibility
                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                return (key, handle);
            });

            return (byte*)interned.Item2.AddrOfPinnedObject();
        }

        // Retrieves a byte pointer for an already interned string
        public unsafe byte* GetPointer(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (m_strings.TryGetValue(value, out var tuple))
            {
                return (byte*)tuple.Item2.AddrOfPinnedObject();
            }
            throw new InvalidOperationException("String not interned");
        }

        public void Dispose()
        {
            foreach (var entry in m_strings.Values)
            {
                entry.Item2.Free(); // Free the pinned GCHandle
            }
            m_strings.Clear();
            m_strings = null;
        }
    }

    public static unsafe class CGoodies
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str + '\0');
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static unsafe string PtrToStringUtf8(byte* ptr)
        {
            if (ptr == null)
            {
                return null;
            }

            byte* temp = ptr;
            while (*temp != 0)
            {
                temp++;
            }

            int length = (int)(temp - ptr);

            // Create a string from the byte array
            return Encoding.UTF8.GetString(ptr, length);
        }

        //private unsafe static string GetStringFromBytePtr(byte* ptr)
        //{
        //    if (ptr == null)
        //    {
        //        return null;
        //    }

        //    // Find the null terminator
        //    int length = 0;
        //    while (ptr[length] != 0)
        //    {
        //        length++;
        //    }

        //    // Convert the bytes to a string
        //    return new string((sbyte*)ptr, 0, length, Encoding.UTF8);
        //}
    }
}
