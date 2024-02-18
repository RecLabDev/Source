using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Theta.SDK.Utils.Logging;

namespace Theta.SDK.Utils
{
    /// <summary>
    /// Utility for building and using reflected objects.
    /// </summary>
    public class Reflector : IDisposable
    {
        /// <summary>
        /// The reflected type
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// A constructed instance of the reflected type
        /// </summary>
        public object Instance { get; private set; }

        /// <summary>
        /// Construct a new Reflector from Console arguments
        /// </summary>
        public Reflector(string typeName) => Type = Type.GetType(typeName, true);

        /// <summary>
        /// Create an instance of the reflected type
        /// </summary>
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.activator.createinstance?view=net-6.0#system-activator-createinstance(system-type">System.Activator.CreateInstance Documentation</see>
        /// <param name="args">Optional constructor parameters</param>
        /// <returns>An object representing the requested type</returns>
        public object Construct(params object[] args)
        {
            return Instance = Activator.CreateInstance(Type, args);
        }

        /// <summary>
        /// Get info for a method on the reflected type.
        /// </summary>
        /// <param name="name">The name of the method</param>
        /// <param name="raise">Throw an exception when no method is found</param>
        /// <returns>MethodInfo for the requested method</returns>
        public MethodInfo GetMethod(string name, bool raise = true)
        {
            MethodInfo method = Array.Find(Type.GetMethods(), methodCandidate =>
            {
                return methodCandidate.Name == name && !methodCandidate.IsGenericMethodDefinition;
            });

            if (raise && method == null)
            {
                throw new Exception($"No method named `{method}` on `{Type}`");
            }

            return method;
        }

        /// <summary>
        /// Invoke an async method using the internal instance of the reflected type.
        /// </summary>
        /// <see href="https://stackoverflow.com/questions/39674988/how-to-call-a-generic-async-method-using-reflection">How to call a generic async method using reflection (Stack Overflow)</see>
        /// <param name="method">The method to invoke</param>
        /// <param name="parameters">Optional parameters to pass to the method</param>
        /// <returns>The result of the async method call</returns>
        public async Task<object> Invoke(string methodName, params NameValueCollection[] parameters)
        {
            // Craft objects for method parameters
            MethodInfo method = GetMethod(methodName);
            object[] craftedParameters = CraftMethodParameters(method, parameters);

            // Invoke method against internal instance and await task
            Task methodCaller = (Task)method.Invoke(Instance, craftedParameters);
            await methodCaller.ConfigureAwait(false);

            // Unpack and return the result
            return methodCaller.GetType().GetProperty("Result").GetValue(methodCaller);
        }

        /// <summary>
        /// Build method parameters from a named value collection
        /// </summary>
        /// <param name="method">Method with parameters</param>
        /// <param name="fields">Collection of fields</param>
        /// <returns>Set of parameters</returns>
        public static object[] CraftMethodParameters(MethodInfo method, params NameValueCollection[] parameterValues)
        {
            ParameterInfo[] methodParameters = method.GetParameters();
            if (methodParameters.Length != parameterValues.Length)
            {
                throw new Exception($"Expected {methodParameters.Length} parameters, got {parameterValues.Length}");
            }

            object[] parameters = new object[methodParameters.Length];
            for (int i1 = 0; i1 < parameters.Length; i1++)
            {
                Type parameterType = methodParameters[i1].ParameterType;
                NameValueCollection memberValues = parameterValues[i1];
                parameters[i1] = CraftObject(parameterType, memberValues);
            }

            return parameters;
        }

        /// <summary>
        /// Craft an object of a particular type using a collection of member values.
        /// </summary>
        /// <param name="type">The object type</param>
        /// <param name="values">The collection of member values</param>
        /// <returns>An object of the specified type</returns>
        public static object CraftObject(Type type, NameValueCollection values)
        {
            object craftedObject = FormatterServices.GetSafeUninitializedObject(type);
            string[] memberNames = values.Keys.Cast<string>().ToArray();

            MemberInfo[] members = type.GetMembers().Where(m => memberNames.Contains(m.Name)).ToArray();
            object[] data = members.Select(m => values.Get(m.Name)).ToArray();

            return FormatterServices.PopulateObjectMembers(craftedObject, members, data);
        }

        /// <summary>
        /// Clean reflector artifacts
        /// </summary>
        public void Dispose()
        {
            // TODO
        }
    }
}
