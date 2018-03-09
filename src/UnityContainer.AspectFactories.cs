using System;
using Unity.Lifetime;
using Unity.Registration;

namespace Unity
{
    public partial class UnityContainer
    {




        #region Build

        private static ResolveDelegate BuildFactoryRegistration(StaticRegistration registration, Func<IUnityContainer, Type, string, object> factory, ResolveDelegate next)
        {
            return (ref ResolutionContext context) => factory(context.Container, registration.RegisteredType, registration.Name);
        }


        private static ResolveDelegate BuildObjectAspectFactory(InternalRegistration registration, ResolveDelegate next)
        {
            if (null != registration.Factory)
                return (ref ResolutionContext context) => registration.Factory(context.Container, registration.Type, registration.Name);

            return (ref ResolutionContext context) => new object();

        }

        #endregion
    }
}
