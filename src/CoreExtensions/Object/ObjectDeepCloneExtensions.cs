using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using StandardDot.CoreExtensions.Object.DeepClone;

namespace StandardDot.CoreExtensions.Object
{
    /// <summary>
    /// Extensions that allow deep clones of objects.
    /// </summary>
    public static class ObjectDeepCloneExtensions
    {
        /// <summary>
        /// Creates a copy of the object.
        /// </summary>
        /// <param name="instance">The object to be copied.</param>
        /// <param name="overrideSettings">Settings to override the copy logic.</param>
        /// <typeparam name="T">The type of the instance given.</typeparam>
        /// <returns>A deep copy of the object.</returns>
        public static T Copy<T>(this T instance, IList<ICopyOverrideSettings> overrideSettings = null)
        {
            if (instance == null)
                return default(T);
            return Copy(instance, DeduceInstance(instance), overrideSettings);
        }

        /// <summary>
        /// Creates a deep copy of the object using the supplied object as a target for the copy operation.
        /// </summary>
        /// <param name="instance">The object to be copied.</param>
        /// <param name="copy">The object to copy values to. All fields of this object will be overwritten.</param>
        /// <typeparam name="T">The type of the instance given.</typeparam>
        /// <param name="overrideSettings">Settings to override the copy logic.</param>
        /// <returns>A deep copy of the object.</returns>
        public static T Copy<T>(this T instance, T copy, IList<ICopyOverrideSettings> overrideSettings = null)
        {
            if (instance == null)
                return default(T);
            if (copy == null)
                throw new ArgumentNullException("The copy instance cannot be null");
            return Clone(instance, new VisitedGraph(), copy, overrideSettings);
        }

        /// <summary>
        /// A list of instance providers that are available.
        /// </summary>
        private static List<IInstanceProvider> Providers = IntializeInstanceProviders();

        /// <summary>
        /// Updates the list of instance providers with any found in the newly loaded assembly.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="args">The event arguments.</param>
        private static void AssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            UpdateInstanceProviders(args.LoadedAssembly, Providers);
        }

        /// <summary>
        /// Initializes the list of instance providers.
        /// </summary>
        /// <returns>A list of instance providers that are used by the Copyable framework.</returns>
        private static List<IInstanceProvider> IntializeInstanceProviders()
        {
            List<IInstanceProvider> providers = new List<IInstanceProvider>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                UpdateInstanceProviders(assembly, providers);
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(AssemblyLoaded);
            return providers;
        }

        /// <summary>
        /// Updates the list of instance providers with the ones found in the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly with which the list of instance providers will be updated.</param>
        private static void UpdateInstanceProviders(Assembly assembly, List<IInstanceProvider> providerList)
        {
            providerList.AddRange(GetInstanceProviders(assembly));
        }

        /// <summary>
        /// Yields all instance providers defined in the assembly, if and only if they are instantiable
        /// without any arguments. <b>NOTE: Instance providers that cannot be instantiated in this 
        /// way are not used by the Copyable framework!</b>
        /// </summary>
        /// <param name="assembly">The assembly from which instance providers will be retrieved.</param>
        /// <returns>An <see cref="IEnumerable" /> of the instance providers of the assembly.</returns>
        private static IEnumerable<IInstanceProvider> GetInstanceProviders(Assembly assembly)
        {
            foreach (TypeInfo t in assembly.DefinedTypes)
            {
                if (typeof(IInstanceProvider).GetTypeInfo().IsAssignableFrom(t))
                {
                    IInstanceProvider instance = null;
                    try
                    {
                        instance = (IInstanceProvider)Activator.CreateInstance(t.AsType());
                    }
                    catch { } // Ignore provider if it cannot be created
                    if (instance != null)
                        yield return instance;
                }
            }
        }

        /// <summary>
        /// Creates a deep copy of an object using the supplied dictionary of visited objects as 
        /// a source of objects already encountered in the copy traversal. The dictionary of visited 
        /// objects is used for holding objects that have already been copied, to avoid erroneous 
        /// duplication of parts of the object graph.
        /// </summary>
        /// <param name="instance">The object to be copied.</param>
        /// <param name="visited">The graph of objects visited so far.</param>
        /// <param name="overrideSettings">Settings to override the copy logic.</param>
        /// <typeparam name="T">The type of the instance given.</typeparam>
        /// <returns>The copied object.</returns>
        private static T Clone<T>(this T instance, VisitedGraph visited, IList<ICopyOverrideSettings> overrideSettings = null)
        {
            if (instance == null)
                return default(T);

            Type instanceType = instance.GetType();

            if(typeof(Type).IsAssignableFrom(instanceType))
            {
                return instance;
            }

            if (instanceType.IsPointer || instanceType == typeof(Pointer) || instanceType.IsPrimitive || instanceType == typeof(string))
                return instance; // Pointers, primitive types and strings are considered immutable
            
            if (instanceType.IsArray)
            {
                int length = ((Array)(object)instance).Length;
                Array copied = (Array)Activator.CreateInstance(instanceType, length);
                visited.Add(instance, copied);
                for (int i = 0; i < length; ++i)
                    copied.SetValue(((Array)(object)instance).GetValue(i).Clone(visited, overrideSettings), i);
                return (T)(object)copied;
            }
            
            return Clone(instance, visited, DeduceInstance(instance), overrideSettings);
        }

        /// <summary>
        /// Creates a deep copy of an object using the supplied dictionary of visited objects as 
        /// a source of objects already encountered in the copy traversal. The dictionary of visited 
        /// objects is used for holding objects that have already been copied, to avoid erroneous 
        /// duplication of parts of the object graph.
        /// </summary>
        /// <param name="instance">The object to be copied.</param>
        /// <param name="visited">The graph of objects visited so far.</param>
        /// <param name="copy">The object to copy to.</param>
        /// <param name="overrideSettings">Settings to override the copy logic.</param>
        /// <typeparam name="T">The type of the instance given.</typeparam>
        /// <returns>The copied object.</returns>
        private static T Clone<T>(this T instance, VisitedGraph visited, T copy, IList<ICopyOverrideSettings> overrideSettings = null)
        {
            if (visited.ContainsKey(instance))
                return (T)visited[instance];
            else
                visited.Add(instance, copy);

            Type type = instance.GetType();

            while (type != null)
            {
                List<ICopyOverrideSettings> currentSettings = overrideSettings?.Where(s => s.ContainingClassType == type)
                                    .ToList() ?? new List<ICopyOverrideSettings>();

                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    object value = field.GetValue(instance);
                    ICopyOverrideSettings currentOverride = currentSettings.SingleOrDefault(s => s.FieldValueOverrideType == field.FieldType
                                && s.AffectedFieldName == field.Name);
                    if (currentOverride != null) {
                        field.SetValue(copy, currentOverride.FieldValueOverride);
                        if (currentOverride.OnlyOverrideFirst) {
                            overrideSettings.Remove(overrideSettings.Single(s => s.AffectedFieldName == s.AffectedFieldName
                                            && s.ContainingClassType == s.ContainingClassType && s.FieldValueOverride == s.FieldValueOverride
                                            && s.FieldValueOverrideType == s.FieldValueOverrideType && s.OnlyOverrideFirst == s.OnlyOverrideFirst));
                        }
                    }
                    else if (visited.ContainsKey(value))
                    {
                        field.SetValue(copy, visited[value]);
                    }
                    else
                    {
                        field.SetValue(copy, value.Clone(visited, overrideSettings));
                    }
                }         

                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    object value = field.GetValue(instance);
                    ICopyOverrideSettings currentOverride = currentSettings.SingleOrDefault(s => s.FieldValueOverrideType == field.FieldType
                                && s.AffectedFieldName == field.Name);
                    if (currentOverride != null) {
                        field.SetValue(copy, currentOverride.FieldValueOverride);
                        if (currentOverride.OnlyOverrideFirst) {
                            overrideSettings.Remove(overrideSettings.Single(s => s.AffectedFieldName == s.AffectedFieldName
                                            && s.ContainingClassType == s.ContainingClassType && s.FieldValueOverride == s.FieldValueOverride
                                            && s.FieldValueOverrideType == s.FieldValueOverrideType && s.OnlyOverrideFirst == s.OnlyOverrideFirst));
                        }
                    }
                    else if (visited.ContainsKey(value))
                    {
                        field.SetValue(copy, visited[value]);
                    }
                    else
                    {
                        field.SetValue(copy, value.Clone(visited, overrideSettings));
                    }
                }         
                      
                foreach (PropertyInfo field in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (!field.CanRead || field.GetIndexParameters()?.Length > 0)
                    {
                        continue;
                    }
                    object value = field.GetValue(instance);
                    ICopyOverrideSettings currentOverride = currentSettings.SingleOrDefault(s => s.FieldValueOverrideType == field.PropertyType
                                && s.AffectedFieldName == field.Name);
                    if (currentOverride != null) {
                        field.SetValue(copy, currentOverride.FieldValueOverride);
                        if (currentOverride.OnlyOverrideFirst) {
                            overrideSettings.Remove(overrideSettings.Single(s => s.AffectedFieldName == s.AffectedFieldName
                                            && s.ContainingClassType == s.ContainingClassType && s.FieldValueOverride == s.FieldValueOverride
                                            && s.FieldValueOverrideType == s.FieldValueOverrideType && s.OnlyOverrideFirst == s.OnlyOverrideFirst));
                        }
                    }
                    if (!field.CanWrite)
                    {
                        continue;
                    }
                    else if (visited.ContainsKey(value))
                    {
                        field.SetValue(copy, visited[value]);
                    }
                    else
                    {
                        field.SetValue(copy, value.Clone(visited, overrideSettings));
                    }
                }

                type = type.BaseType;
            }
            return copy;
        }

        /// <summary>
        /// Gets a "default" instance of the same type of the instance given.
        /// </summary>
        /// <param name="instance">The object to be copied.</param>
        /// <typeparam name="T">The type of the instance given.</typeparam>
        /// <returns>The "default" instance of the same type of the instance given.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when an unitialized object can not be created.
        /// </exception>
        private static T DeduceInstance<T>(T instance)
        {
            Type instanceType = instance.GetType();

            if (typeof(Copyable).IsAssignableFrom(instanceType))
                return (T)(((Copyable)(object)instance).CreateInstanceForCopy());
            foreach (IInstanceProvider provider in Providers)
            {
                if (provider.Provided == instanceType)
                    return (T)provider.CreateCopy(instance);
            }

            try
            {
                return (T)FormatterServices.GetUninitializedObject(instanceType);
            }
            catch
            {
                throw new ArgumentException(string.Format("Object of type {0} cannot be cloned because an uninitialized object could not be created.", instanceType.FullName));
            }
        }
    }
}