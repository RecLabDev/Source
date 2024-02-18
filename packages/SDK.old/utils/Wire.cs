using System;

using Newtonsoft.Json;

namespace Theta.SDK.Utils
{
    /// <summary>
    /// What kind of message is this?
    /// </summary>
    public enum WireMessageKind
    {
        Ghost = 0,
    }

    /// <summary>
    /// Classification utility for Theta SDK wire messages.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class WireMessageAttribute : Attribute
    {
        /// <summary>
        /// Friendly name for the object.
        /// </summary>
        public readonly WireMessageKind Kind;

        /// <summary>
        /// Default CTOR.
        /// </summary>
        /// <param name="name">The name of the decorated object</param>
        /// <param name="namespace">The name of the decorated object</param>
        /// <param name="project">The name of the decorated object</param>
        public WireMessageAttribute(WireMessageKind kind)
        {
            Kind = kind;
        }
    }
}
