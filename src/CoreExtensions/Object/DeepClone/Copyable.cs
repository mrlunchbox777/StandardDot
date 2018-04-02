﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace shoellibraries.CoreExtensions.Object.DeepClone
{
    /// <summary>
    /// This class is an abstract base class that can be used as a really simple way of making an object
    /// copyable.
    /// 
    /// To make an object copyable, simply inherit from this class, and call the base constructor from
    /// your constructor, with the same arguments as your constructor.
    /// </summary>
    /// <example>
    ///     <code>
    ///     public class ACopyable : Copyable
    ///     {
    ///       private ACopyable _friend;
    ///       
    ///       public ACopyable(ACopyable friend)
    ///         : base(friend)
    ///       {
    ///         this._friend = friend;
    ///       }
    ///     }
    ///     </code>
    /// </example>
    public abstract class Copyable
    {
        private ConstructorInfo constructor;

        private object[] constructorArgs;

        /// <summary>
        /// The generic constructor that should be called from every inheriting class to let copyable work properly.
        /// </summary>
        /// <param name="args">All of the arguments for the constructor</param>
        protected Copyable(params object[] args)
        {
            StackFrame frame = new StackFrame(1, true);

            MethodBase method = frame.GetMethod();

            if (!method.IsConstructor)
            {
                throw new InvalidOperationException("Copyable cannot be instantiated directly; use a subclass.");
            }

            ParameterInfo[] parameters = method.GetParameters();

            if (args.Length > parameters.Length)
            {
                throw new InvalidOperationException("Copyable constructed with more arguments than the constructor of its subclass.");
            }

            List<Type> constructorTypeArgs = new List<Type>();
            int i = 0;

            for (; i < args.Length; ++i)
            {
                if (!parameters[i].ParameterType.GetTypeInfo().IsAssignableFrom(args[i].GetType().GetTypeInfo()))
                {
                    throw new InvalidOperationException(string.Format("Copyable constructed with invalid type {0} for argument #{2} (should be {1})",
                        args[i].GetType(), parameters[i].ParameterType, i));
                }
                constructorTypeArgs.Add(parameters[i].ParameterType);
            }
            for (; i < parameters.Length; ++i)
            {
                if (!parameters[i].IsOptional)
                {
                    throw new InvalidOperationException("Copyable constructed with too few arguments.");
                }
                constructorTypeArgs.Add(parameters[i].ParameterType);
            }

            constructor = GetType().GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters()
                .Select(p => p.ParameterType).Except(constructorTypeArgs).Count() == 0);
            constructorArgs = args;
        }

        /// <summary>
        /// Creates a "default" instance for an object to be used by the <see cref="ObjectDeepCloneExtensions" />.
        /// </summary>
        /// <returns>The "default" instance of this Type.</returns>
        public object CreateInstanceForCopy()
        {
            return constructor.Invoke(constructorArgs);
        }
    }
}
