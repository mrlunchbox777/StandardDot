using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
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
        /// <param name="expectedType">The type that should be found.</param>
        /// <param name="overrideSettings">Settings to override the copy logic.</param>
        /// <typeparam name="T">The type of the instance given.</typeparam>
        /// <returns>A deep copy of the object.</returns>
        public static T Copy<T>(this T instance, Type expectedType, ICopyOverrideSettings overrideSettings)
        {
            return instance.Copy(expectedType, new[] { overrideSettings });
        }

        /// <summary>
        /// Creates a deep copy of the object using the supplied object as a target for the copy operation.
        /// </summary>
        /// <param name="instance">The object to be copied.</param>
        /// <param name="copy">The object to copy values to. All fields of this object will be overwritten.</param>
        /// <param name="expectedType">The type that should be found.</param>
        /// <param name="overrideSettings">Settings to override the copy logic.</param>
        /// <typeparam name="T">The type of the instance given.</typeparam>
        /// <returns>A deep copy of the object.</returns>
        public static T Copy<T>(this T instance, T copy, Type expectedType, ICopyOverrideSettings overrideSettings)
        {
            return instance.Copy(copy, expectedType, new[] { overrideSettings });
        }

        /// <summary>
        /// Creates a copy of the object.
        /// </summary>
        /// <param name="instance">The object to be copied.</param>
        /// <param name="expectedType">The type that should be found.</param>
        /// <param name="overrideSettings">Settings to override the copy logic.</param>
        /// <typeparam name="T">The type of the instance given.</typeparam>
        /// <returns>A deep copy of the object.</returns>
        public static T Copy<T>(this T instance, Type expectedType, IList<ICopyOverrideSettings> overrideSettings = null)
        {
            if (instance == null)
                return default(T);
            return Clone(instance, new VisitedGraph(), expectedType, overrideSettings);
        }

        /// <summary>
        /// Creates a deep copy of the object using the supplied object as a target for the copy operation.
        /// </summary>
        /// <param name="instance">The object to be copied.</param>
        /// <param name="copy">The object to copy values to. All fields of this object will be overwritten.</param>
        /// <param name="expectedType">The type that should be found.</param>
        /// <param name="overrideSettings">Settings to override the copy logic.</param>
        /// <typeparam name="T">The type of the instance given.</typeparam>
        /// <returns>A deep copy of the object.</returns>
        public static T Copy<T>(this T instance, T copy, Type expectedType, IList<ICopyOverrideSettings> overrideSettings = null)
        {
            if (instance == null)
                return default(T);
            if (copy == null)
                throw new ArgumentNullException("The copy instance cannot be null");
            return Clone(instance, new VisitedGraph(), copy, expectedType, overrideSettings);
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
        /// <param name="expectedType">The type that should be found.</param>
        /// <param name="overrideSettings">Settings to override the copy logic.</param>
        /// <typeparam name="T">The type of the instance given.</typeparam>
        /// <returns>The copied object.</returns>
        private static T Clone<T>(this T instance, VisitedGraph visited, Type expectedType, IList<ICopyOverrideSettings> overrideSettings = null)
        {
            if (instance == null)
            {
                return default(T);
            }

            Type instanceType = instance.GetType();
            string instanceTypeNameString = instanceType.FullName;
            bool shouldSkip = false;
            shouldSkip = CheckIfShouldSkip(overrideSettings, instanceType.FullName, false);

            if (shouldSkip)
            {
                return default(T);
            }


            if (typeof(Type).IsAssignableFrom(instanceType))
            {
                return instance;
            }

            if (instanceType.IsPointer || instanceType == typeof(Pointer) || instanceType.IsPrimitive || instanceType == typeof(string) || instanceType == typeof(decimal) || instanceType.IsValueType)
            {
                return instance; // Pointers, primitive types and strings are considered immutable
            }

            if (instanceType.IsArray)
            {
                int length = ((Array)(object)instance).Length;
                Array copied = (Array)Activator.CreateInstance(instanceType, length);
                if (ShouldUseVisitedGraph(overrideSettings))
                {
                    visited.Add(instance, copied);
                }
                Type elementType = instance.GetType().GetElementType();
                bool gotARealType = elementType != null;
                if (!gotARealType)
                {
                    for (int i = 0; i < length || gotARealType; i++)
                    {
                        object current = ((Array)(object)instance).GetValue(i);
                        if (current != null)
                        {
                            elementType = current.GetType();
                            gotARealType = true;
                        }
                    }
                }
                for (int i = 0; i < length; ++i)
                {
                    object current = ((Array)(object)instance).GetValue(i);
                    copied.SetValue(current.Clone(visited, gotARealType ? elementType : current?.GetType(), overrideSettings), i);
                }
                return (T)(object)copied;
            }
            else if (typeof(IList).IsAssignableFrom(instanceType))
            {
                IList copied = (IList)Activator.CreateInstance(instanceType);
                Type elementType = null;
                bool gotAType = false;
                if (instanceType.IsGenericType)
                {
                    elementType = instanceType.GetGenericArguments().Single();
                    gotAType = true;
                }

                if (!gotAType)
                {
                    elementType = instance.GetType().GetElementType();
                    gotAType = true;
                }


                if (ShouldUseVisitedGraph(overrideSettings))
                {
                    visited.Add(instance, copied);
                }
                foreach (object item in (IList)instance)
                {
                    object clonedItem = item.Clone(visited, elementType, overrideSettings);
                    copied.Add(clonedItem);
                }
                return (T)(object)copied;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(instanceType))
            {
                // we need to find a way for this to handle ICollection and IEnumerable
                // I'm thinking that we go with an iterator approach find the length, make a new instance with that size
                // then iterate again and do a clone for each item
                Type[] arguments = instanceType.GenericTypeArguments;
                IEnumerable copied = (IEnumerable)DeduceInstance(instance);
                if (ShouldUseVisitedGraph(overrideSettings))
                {
                    visited.Add(instance, copied);
                }
                MethodInfo castMethod = typeof(Enumerable).GetMethod("Cast", BindingFlags.Static | BindingFlags.Public);
                MethodInfo castGenericMethod = castMethod.MakeGenericMethod(arguments);
                IEnumerable<dynamic> casted = (dynamic)castGenericMethod.Invoke(null, new object[] { ((IEnumerable)instance) });
                copied = ((IEnumerable)instance).Cast<dynamic>().ToList().AsEnumerable();
                return (T)(object)copied;
            }
            else if (typeof(DateTime).IsAssignableFrom(instanceType))
            {
                if (((DateTime)(object)instance) == DateTime.MinValue)
                {
                    return (T)(object)(new DateTime(1753, 1, 1));
                }
            }

            return Clone(instance, visited, DeduceInstance(instance), expectedType, overrideSettings);
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
        /// <param name="expectedType">The type that should be found.</param>
        /// <param name="overrideSettings">Settings to override the copy logic.</param>
        /// <typeparam name="T">The type of the instance given.</typeparam>
        /// <returns>The copied object.</returns>
        private static T Clone<T>(this T instance, VisitedGraph visited, T copy, Type expectedType, IList<ICopyOverrideSettings> overrideSettings = null)
        {
            if (ShouldUseVisitedGraph(overrideSettings))
            {
                if (visited.ContainsKey(instance))
                    return (T)visited[instance];
                else
                    visited.Add(instance, copy);
            }

            Type type = instance.GetType();
            string instanceTypeNameString = type.FullName;
            bool shouldSkip = CheckIfShouldSkip(overrideSettings, type.FullName, false);

            if (shouldSkip)
            {
                return default(T);
            }

            BindingFlags flags = overrideSettings?.Any(s => s.IncludeNonPublic) ?? false
                ? BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                : BindingFlags.Public | BindingFlags.Instance;

            while (type != null)
            {
                List<ICopyOverrideSettings> currentSettings = overrideSettings?.Where(s => s.ContainingClassType.IsAssignableFrom(type))
                                    .ToList() ?? new List<ICopyOverrideSettings>();

                foreach (FieldInfo field in type.GetFields(flags))
                {
                    shouldSkip = false;
                    shouldSkip = CheckIfShouldSkip(overrideSettings, field?.FieldType?.FullName, shouldSkip);

                    if (shouldSkip || ShouldSkipDotNet(type, field.FieldType, field.Name))
                    {
                        continue;
                    }

                    object value = field.GetValue(instance);
                    ICopyOverrideSettings currentOverride = currentSettings.SingleOrDefault(s => s.FieldValueOverrideType.IsAssignableFrom(field.FieldType)
                                && s.ContainingClassType.IsAssignableFrom(type)
                                && s.AffectedFieldName == field.Name);
                    try
                    {
                        if (currentOverride != null)
                        {
                            if (!currentOverride.ShouldSkipOverrideInsteadOfSet)
                            {
                                var settableValue = currentOverride.UseFieldValueOverrideFunction
                                    ? currentOverride.FieldValueOverrideFunction(instance)
                                    : currentOverride.FieldValueOverride;
                                field.SetValue(copy, settableValue);
                                if (currentOverride.OnlyOverrideFirst)
                                {
                                    overrideSettings.Remove(overrideSettings.Single(s => s.AffectedFieldName == s.AffectedFieldName
                                                    && s.ContainingClassType == s.ContainingClassType && s.FieldValueOverride == s.FieldValueOverride
                                                    && s.FieldValueOverrideType == s.FieldValueOverrideType && s.OnlyOverrideFirst == s.OnlyOverrideFirst));
                                }
                            }
                        }
                        else if (ShouldUseVisitedGraph(overrideSettings) && visited.ContainsKey(value))
                        {
                            var settableValue = visited[value];
                            field.SetValue(copy, settableValue);
                        }
                        else
                        {
                            var settableValue = value.Clone(visited, field.FieldType, overrideSettings);
                            field.SetValue(copy, settableValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ShouldThrow(overrideSettings, field?.FieldType?.FullName, true, ex))
                        {
                            throw;
                        }
                    }
                }

                foreach (PropertyInfo propertyInfo in type.GetProperties(flags))
                {
                    if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
                    {
                        continue;
                    }
                    shouldSkip = false;
                    shouldSkip = CheckIfShouldSkip(overrideSettings, propertyInfo?.PropertyType?.FullName, shouldSkip);

                    ParameterInfo[] propParameters = propertyInfo.GetIndexParameters();
                    if (!shouldSkip && propParameters?.Where(pp => !pp.IsOptional).Count() > 0)
                    {
                        shouldSkip = true;
                    }

                    if (shouldSkip || ShouldSkipDotNet(type, propertyInfo.PropertyType, propertyInfo.Name))
                    {
                        continue;
                    }

                    object value = propertyInfo.GetValue(instance);
                    ICopyOverrideSettings currentOverride = currentSettings.SingleOrDefault(s => s.FieldValueOverrideType.IsAssignableFrom(propertyInfo.PropertyType)
                                && s.ContainingClassType.IsAssignableFrom(type)
                                && s.AffectedFieldName == propertyInfo.Name);
                    try
                    {
                        if (currentOverride != null)
                        {
                            if (!currentOverride.ShouldSkipOverrideInsteadOfSet)
                            {
                                var settableValue = currentOverride.UseFieldValueOverrideFunction
                                    ? currentOverride.FieldValueOverrideFunction(instance)
                                    : currentOverride.FieldValueOverride;
                                propertyInfo.SetValue(copy, settableValue);
                                if (currentOverride.OnlyOverrideFirst)
                                {
                                    overrideSettings.Remove(overrideSettings.Single(s => s.AffectedFieldName == s.AffectedFieldName
                                                    && s.ContainingClassType == s.ContainingClassType && s.FieldValueOverride == s.FieldValueOverride
                                                    && s.FieldValueOverrideType == s.FieldValueOverrideType && s.OnlyOverrideFirst == s.OnlyOverrideFirst));
                                }
                            }
                        }
                        else if (ShouldUseVisitedGraph(overrideSettings) && visited.ContainsKey(value))
                        {
                            var settableValue = visited[value];
                            propertyInfo.SetValue(copy, settableValue);
                        }
                        else
                        {
                            var settableValue = value.Clone(visited, propertyInfo.PropertyType, overrideSettings);
                            propertyInfo.SetValue(copy, settableValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ShouldThrow(overrideSettings, propertyInfo?.PropertyType?.FullName, true, ex))
                        {
                            throw;
                        }
                    }
                }

                type = type.BaseType;
            }

            // call the post copy actions
            if (overrideSettings?.Any(s => s?.PostCopyActions != null) ?? false)
            {
                foreach (ICopyOverrideSettings setting in overrideSettings)
                {
                    if (!(setting?.PostCopyActions?.Any() ?? false))
                    {
                        continue;
                    }

                    // try the exact post copy action
                    if (setting.PostCopyActions.ContainsKey(copy.GetType()))
                    {
                        setting.PostCopyActions[copy.GetType()](instance, copy, expectedType);
                    }
                    // try the default post copy action
                    else if (setting.PostCopyActions.ContainsKey(setting.DefaultPostActionType))
                    {
                        setting.PostCopyActions[setting.DefaultPostActionType](instance, copy, expectedType);
                    }
                }
            }
            return copy;
        }

        private static Dictionary<int, List<string>> cachedFullNamesToSkip = new Dictionary<int, List<string>>();

        private static Dictionary<int, List<string>> cachedFullNamesToInclude = new Dictionary<int, List<string>>();

        private static Dictionary<int, List<string>> cachedFullNamesToAttempt = new Dictionary<int, List<string>>();

        /// <summary>
        /// Checks if a property should be skipped because it is a dotnet property that can't be cloned
        /// </summary>
        /// <param name="containingClassType">The type of containing classe</param>
        /// <param name="propertyType">The type of the property</param>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>If the property should be skipped</returns>
        private static bool ShouldSkipDotNet(Type containingClassType, Type propertyType, string propertyName)
        {
            if (typeof(IEnumerable).IsAssignableFrom(containingClassType))
            {
                if (propertyType == typeof(int) && propertyName == "Capacity")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a property should be skipped in the clone
        /// </summary>
        /// <param name="overrideSettings">The settings to check</param>
        /// <param name="fullName">The full name of the property</param>
        /// <param name="shouldThrow">If the property should be skipped by default</param>
        /// <returns>If the property should be skipped</returns>
        private static bool CheckIfShouldSkip(IList<ICopyOverrideSettings> overrideSettings, string fullName, bool shouldSkip)
        {
            List<string> fullNamesToSkip;
            if (overrideSettings != null && cachedFullNamesToSkip.ContainsKey(overrideSettings.GetHashCode()))
            {
                fullNamesToSkip = cachedFullNamesToSkip[overrideSettings.GetHashCode()];
            }
            else
            {
                fullNamesToSkip = overrideSettings?.Where(s => s?.FullNamesToSkip?.Any() ?? false).SelectMany(s => s.FullNamesToSkip)
                    .Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
                if (fullNamesToSkip?.Any() ?? false)
                {
                    cachedFullNamesToSkip.Add(overrideSettings.GetHashCode(), fullNamesToSkip);
                }
            }
            List<string> fullNamesToInclude;
            if (overrideSettings != null && cachedFullNamesToInclude.ContainsKey(overrideSettings.GetHashCode()))
            {
                fullNamesToInclude = cachedFullNamesToInclude[overrideSettings.GetHashCode()];
            }
            else
            {
                fullNamesToInclude = overrideSettings?.Where(s => s?.FullNamesToInclude?.Any() ?? false).SelectMany(s => s.FullNamesToInclude)
                    .Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
                if (fullNamesToInclude?.Any() ?? false)
                {
                    cachedFullNamesToInclude.Add(overrideSettings.GetHashCode(), fullNamesToInclude);
                }
            }

            if (fullNamesToSkip?.Any() ?? false)
            {
                foreach (string fullNameToSkip in fullNamesToSkip)
                {
                    if (string.IsNullOrWhiteSpace(fullNameToSkip))
                    {
                        continue;
                    }
                    if (Regex.Match(fullName ?? "", fullNameToSkip).Success)
                    {
                        shouldSkip = true;
                        break;
                    }
                }
            }
            if (fullNamesToInclude?.Any() ?? false)
            {
                foreach (string fullNameToInclude in fullNamesToInclude)
                {
                    if (string.IsNullOrWhiteSpace(fullNameToInclude))
                    {
                        continue;
                    }
                    if (Regex.Match(fullName ?? "", fullNameToInclude).Success)
                    {
                        shouldSkip = false;
                        break;
                    }
                }
            }

            return shouldSkip;
        }

        /// <summary>
        /// Checks if an exception should be thrown
        /// </summary>
        /// <param name="overrideSettings">The settings to check</param>
        /// <param name="fullName">The full name of the property</param>
        /// <param name="shouldThrow">If the exception should be thrown by default</param>
        /// <param name="Exception">The exception in question</param>
        /// <returns>If the exception should be thrown</returns>
        private static bool ShouldThrow(IList<ICopyOverrideSettings> overrideSettings, string fullName, bool shouldThrow, Exception ex)
        {
            shouldThrow = true;
            List<string> fullNamesToAttempt;
            if (overrideSettings != null && cachedFullNamesToAttempt.ContainsKey(overrideSettings.GetHashCode()))
            {
                fullNamesToAttempt = cachedFullNamesToAttempt[overrideSettings.GetHashCode()];
            }
            else
            {
                fullNamesToAttempt = overrideSettings?.Where(s => s?.FullNamesToAttempt?.Any() ?? false).SelectMany(s => s.FullNamesToAttempt)
                    .Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
                if (fullNamesToAttempt?.Any() ?? false)
                {
                    cachedFullNamesToAttempt.Add(overrideSettings.GetHashCode(), fullNamesToAttempt);
                }
            }

            if (fullNamesToAttempt?.Any() ?? false)
            {
                foreach (string fullNameToAttempt in fullNamesToAttempt)
                {
                    if (string.IsNullOrWhiteSpace(fullNameToAttempt))
                    {
                        continue;
                    }
                    if (Regex.Match(fullName ?? "", fullNameToAttempt).Success)
                    {
                        shouldThrow = false;
                    }
                }
            }
            return shouldThrow;
        }

        /// <summary>
        /// Checks the setting to see if the visited graph should be used
        /// </summary>
        /// <param name="overrideSettings">The settings to check</param>
        /// <returns>If the visted graph should be used</returns>
        private static bool ShouldUseVisitedGraph(IList<ICopyOverrideSettings> overrideSettings)
        {
            return overrideSettings?.Any(o => o?.UseVisitedGraph ?? true) ?? true;
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
        public static T DeduceInstanceForType<T>(this T instance)
        {
            Type instanceType = instance.GetType();
            if (typeof(Type).IsAssignableFrom(instanceType))
            {
                return instance;
            }

            if (instanceType.IsPointer || instanceType == typeof(Pointer) || instanceType.IsPrimitive || instanceType == typeof(string) || instanceType == typeof(decimal) || instanceType.IsValueType)
                return instance; // Pointers, primitive types and strings are considered immutable

            if (instanceType.IsArray)
            {
                int length = ((Array)(object)instance).Length;
                Array copied = (Array)Activator.CreateInstance(instanceType, length);
                return (T)(object)copied;
            }
            return DeduceInstance(instance);
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