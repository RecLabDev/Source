using System;
using System.Collections.Generic;
using System.Text;

namespace Theta.SDK.Utils
{
    /// <summary>
    /// Utilities for indentifying and working with serial streams.
    /// </summary>
    public static class Serial
    {
        /// <summary>
        /// Supported serial formats.
        /// 
        /// Used to identify serial formats for service clients.
        /// </summary>
        public enum Format
        {
            /// <summary>
            /// Useful when sending data over a network or other "slow" buffer.
            /// </summary>
            Flatbuffers = 0,

            /// <summary>
            /// TODO
            /// </summary>
            CBOR = 1,

            /// <summary>
            /// Useful when sending data to third-party APIs, legacy file-based
            /// configs for some tools, and CloudWatch/graffana logs, etc.
            /// </summary>
            JSON = 2,
        }

        /// <summary>
        /// Utility to translate Format enum values to a content type (for file I/O, http, etc)
        /// </summary>
        public static readonly Dictionary<Format, string> ContentType = new Dictionary<Format, string>
        {
            [Format.Flatbuffers] = "application/octet-stream",
            [Format.JSON] = "application/json",
            [Format.CBOR] = "application/cbor",
        };

        /// <summary>
        /// Pack a serial stream for a given format.
        /// </summary>
        public static string Pack(object data, Format format)
        {
            switch (format)
            {
                case Format.Flatbuffers:
                    throw new Exception("Flatbuffers-encoding not (yet) implemented");
                case Format.CBOR:
                    throw new Exception("CBOR-encoding not (yet) implemented");
                case Format.JSON:
                    return JSON.SerializeObject(data);
                default:
                    throw new Exception($"Unknown serial format");
            }
        }

        /// <summary>
        /// Unpack a serial stream of a given format.
        /// </summary>
        public static T Unpack<T>(string data, Format format)
        {
            switch (format)
            {
                case Format.Flatbuffers:
                    throw new Exception("Flatbuffers-decoding not (yet) implemented");
                case Format.CBOR:
                    throw new Exception("CBOR-decoding not (yet) implemented");
                case Format.JSON:
                    return JSON.DeserializeObject<T>(data);
                default:
                    throw new Exception($"Unknown serial format");
            }
        }
    }
}
