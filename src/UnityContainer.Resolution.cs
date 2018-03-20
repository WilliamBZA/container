using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Build.Selected;
using Unity.Builder;
using Unity.Exceptions;
using Unity.Pipeline;
using Unity.Registration;
using Unity.Resolution;

namespace Unity
{
    /// <inheritdoc />
    /// <summary>
    /// A simple, extensible dependency injection container.
    /// </summary>
    public partial class UnityContainer
    {
        #region Getting objects

        /// <summary>
        /// GetOrDefault an instance of the requested type with the given name typeFrom the container.
        /// </summary>
        /// <param name="typeToBuild"><see cref="Type"/> of object to get typeFrom the container.</param>
        /// <param name="nameToBuild">Name of the object to retrieve.</param>
        /// <param name="resolverOverrides">Any overrides for the resolve call.</param>
        /// <returns>The retrieved object.</returns>
        public object Resolve(Type typeToBuild, string nameToBuild, params ResolverOverride[] resolverOverrides)
        {
            // Verify arguments
            var name = string.IsNullOrEmpty(nameToBuild) ? null : nameToBuild;
            var type = typeToBuild ?? throw new ArgumentNullException(nameof(typeToBuild));


            try
            {
                Type ResolvingType = type;
                //VerifyPipeline<Type> Verify = (Type t) => {};
                var registration = GetRegistration(type, name);

                ResolveDependency Resolve = (Type t, string n) =>
                {
                    // Verify if can resolve this type
                    //Verify(t);

                    // Cache old values
                    //VerifyPipeline<Type> parentVerify = Verify;
                    Type parentResolvingType = ResolvingType;

                    try
                    {
                        Func<Type, string, Type, object> getMethod = (Type tt, string na, Type i) => _get(tt, na, i);
                        ResolutionContext context = new ResolutionContext(getMethod, null)
                        {
                            LifetimeContainer = _lifetimeContainer,

                            Registration = GetRegistration(t, n),
                            ImplementationType = t,
                            DeclaringType = ResolvingType,
                        };

                        ResolvingType = t;
                        //Verify = (Type vT) => { if (ResolvingType == vT) throw new InvalidOperationException(); };

                        return ((IResolveMethod)context.Registration).ResolveMethod(ref context);
                    }
                    finally
                    {
                        //Verify = parentVerify;
                        ResolvingType = parentResolvingType;
                    }

                };

                ResolutionContext rootContext = new ResolutionContext
                {
                    LifetimeContainer = _lifetimeContainer,

                    Registration = registration,
                    ImplementationType = type,
                    DeclaringType = null,

                    Resolve = Resolve
                };

                return registration.ResolveMethod?.Invoke(ref rootContext);
            }
            catch (Exception ex)
            {
                throw new ResolutionFailedException(type, name, "// TODO: Bummer!", ex);
            }
        }

        #endregion


        #region BuildUp existing object

        /// <summary>
        /// Run an existing object through the container and perform injection on it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is useful when you don'type control the construction of an
        /// instance (ASP.NET pages or objects created via XAML, for instance)
        /// but you still want properties and other injection performed.
        /// </para></remarks>
        /// <param name="typeToBuild"><see cref="Type"/> of object to perform injection on.</param>
        /// <param name="existing">Instance to build up.</param>
        /// <param name="nameToBuild">name to use when looking up the typemappings and other configurations.</param>
        /// <param name="resolverOverrides">Any overrides for the buildup.</param>
        /// <returns>The resulting object. By default, this will be <paramref name="existing"/>, but
        /// container extensions may add things like automatic proxy creation which would
        /// cause this to return a different object (but still type compatible with <paramref name="typeToBuild"/>).</returns>
        public object BuildUp(Type typeToBuild, object existing, string nameToBuild, params ResolverOverride[] resolverOverrides)
        {
            // Verify arguments
            var name = string.IsNullOrEmpty(nameToBuild) ? null : nameToBuild;
            var type = typeToBuild ?? throw new ArgumentNullException(nameof(typeToBuild));
            if (null != existing) InstanceIsAssignable(type, existing, nameof(existing));

            var context = new BuilderContext(this, (InternalRegistration)GetRegistration(type, name), existing, resolverOverrides);

            return BuilUpPipeline(context);
        }


        #endregion


        #region Resolving Enumerables

        internal static void ResolveArray<T>(IBuilderContext context)
        {
            var container = (UnityContainer)context.Container;
            var list = new List<T>();

            var registrations = (IList<InternalRegistration>)GetNamedRegistrations(container, typeof(T));
            for (var i = 0; i < registrations.Count; i++)
            {
                var registration = registrations[i];

                if (registration.Type.GetTypeInfo().IsGenericTypeDefinition)
                    list.Add((T)((BuilderContext)context).NewBuildUp(typeof(T), registration.Name));
                else
                    list.Add((T)((BuilderContext)context).NewBuildUp(registration));
            }

            context.Existing = list.ToArray();
            context.BuildComplete = true;
        }

        internal static void ResolveEnumerable<T>(IBuilderContext context)
        {
            var container = (UnityContainer)context.Container;
            var list = new List<T>();

            var registrations = (IList<InternalRegistration>)GetNotEmptyRegistrations(container, typeof(T));
            for (var i = 0; i < registrations.Count; i++)
            {
                var registration = registrations[i];

                if (registration.Type.GetTypeInfo().IsGenericTypeDefinition)
                    list.Add((T)((BuilderContext)context).NewBuildUp(typeof(T), registration.Name));
                else
                    list.Add((T)((BuilderContext)context).NewBuildUp(registration));
            }

            context.Existing = list;
            context.BuildComplete = true;
        }

        #endregion
    }
}
