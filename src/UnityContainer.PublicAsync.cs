using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Container.Registration;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Resolution;

namespace Unity
{
    public partial class UnityContainer : IUnityContainerAsync
    {
        #region Type Registration

        /// <inheritdoc />
        public IUnityContainer RegisterTypeAsync(Type registeredType, string name, Type mappedTo, LifetimeManager lifetimeManager, InjectionMember[] injectionMembers)
        {
            // Validate input
            if (null == registeredType) throw new ArgumentNullException(nameof(registeredType));
            if (null != mappedTo)
            {
                var mappedInfo = mappedTo.GetTypeInfo();
                var registeredInfo = registeredType.GetTypeInfo();
                if (!registeredInfo.IsGenericType && !mappedInfo.IsGenericType && !registeredInfo.IsAssignableFrom(mappedInfo))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                        Constants.TypesAreNotAssignable, registeredType, mappedTo), nameof(registeredType));
                }
            }

            // Create registration
            var registration = new ExplicitRegistration(registeredType, name, mappedTo, lifetimeManager); // ReSharper disable once CoVariantArrayConversion

            // Register type
            //registration.ResolveMethod = _asyncRegistrationPipeline(_lifetimeContainer, registration, injectionMembers);

            // Add to appropriate storage
            StoreRegistration(registration);
/*
            // Define the cancellation token.
            var source = new CancellationTokenSource();

            // Schedule build of pipeline asynchronously
            Task.Factory.StartNew(() =>
            {
                // Add injection members policies to the registration
                if (null != args && 0 < args.Length)
                {
                    foreach (var member in args.OfType<InjectionMember>())
                    {
                        // Validate against ImplementationType with InjectionFactory
                        if (member is InjectionFactory && registration.ImplementationType != registration.Type)  // TODO: Add proper error message
                            throw new InvalidOperationException("Registration where both ImplementationType and InjectionFactory are set is not supported");

                        // Mark as requiring build if any one of the injectors are marked with IRequireBuild
                        if (member is IRequireBuild) registration.BuildRequired = true;

                        // Add policies
                        member.AddPolicies(registration.Type, registration.Name, registration.ImplementationType, registration);
                    }
                }

                return registration.ResolveMethod = next(lifetimeContainer, registration);
            }, source.Token);

            // Return temporary stub in case resolution happened
            // before pipeline has been processed
            return (ref ResolutionContext context) =>
            {
                // Cancel the Async pipeline build 
                source.Cancel();

                // Add injection members policies to the registration
                if (null != args && 0 < args.Length)
                {
                    foreach (var member in args.OfType<InjectionMember>())
                    {
                        // Validate against ImplementationType with InjectionFactory
                        if (member is InjectionFactory && registration.ImplementationType != registration.Type)  // TODO: Add proper error message
                            throw new InvalidOperationException("Registration where both ImplementationType and InjectionFactory are set is not supported");

                        // Mark as requiring build if any one of the injectors are marked with IRequireBuild
                        if (member is IRequireBuild) registration.BuildRequired = true;

                        // Add policies
                        member.AddPolicies(registration.Type, registration.Name, registration.ImplementationType, registration);
                    }
                }

                // Build pipeline in real time
                registration.ResolveMethod = next(lifetimeContainer, registration);

                // Build the object
                return registration.ResolveMethod(ref context);
            };
*/
            return this;
        }

        #endregion


        #region Getting objects

        /// <inheritdoc />
        public Task<object> ResolveAsync(Type type, string nameToBuild, params ResolverOverride[] resolverOverrides)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
