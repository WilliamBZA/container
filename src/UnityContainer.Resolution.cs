using System.Collections.Generic;
using System.Reflection;
using Unity.Builder;
using Unity.Registration;

namespace Unity
{
    // Resolving Engine Implementation 
    public partial class UnityContainer
    {
        #region Resolving enumerable types

        internal static void ResolveArray<T>(IBuilderContext context)
        {
            var container = (UnityContainer)context.Container;
            var list = new List<T>();

            var registrations = (IList<ImplicitRegistration>)GetNamedRegistrations(container, typeof(T));
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

            var registrations = (IList<ImplicitRegistration>)GetNotEmptyRegistrations(container, typeof(T));
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
