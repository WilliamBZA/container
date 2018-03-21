using System;
using System.Linq;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Factory;
using Unity.Build.Pipeleine;
using Unity.Build.Pipeline;
using Unity.Policy;
using Unity.Registration;

namespace Unity
{
    public partial class UnityContainer
    {
        public static RegisterPipeline BuildAspectFactory(RegisterPipeline next)
        {
            return (IUnityContainer unity, IPolicySet set, object[] args) =>
            {
                Type type;
                var container = (UnityContainer)unity;
                InternalRegistration registration = (InternalRegistration)set;

                switch (args?.Length)
                {
                    case 1:     // Mapped POCO
                        type = (Type)args[0];
                        break;

                    case 2:     // Mapped generic with source
                        type = (Type)args[0];
                        registration = (InternalRegistration)args[1];
                        break;

                    default:    // No mapping    
                        type = registration.Type;
                        break;
                }

                var info = type.GetTypeInfo();

                if (info.IsGenericTypeDefinition)
                {
                    var ctor = container._constructorSelectionPipeline(container, registration);
                    //var methods = container._methodsSelectionPipeline(container, registration).Distinct();
                    //var properties = container._propertiesSelectionPipeline(container, registration).Distinct();


                    // Add factory
                    set.Set(typeof(ResolveMethodFactory<Type>), ctor.ResolveMethodFactory);


                    //buildMethod = (ref ResolutionContext c) =>
                    //{
                    //    ResolveMethod ctorMethod = ctor.ResolveMethodFactory(type);
                    //    //Resolve properties
                    //    //Resolve methods

                    //    buildMethod = (ref ResolutionContext context) =>
                    //    {
                    //        //context.Existing = createMethod(ref context);
                    //        //context.Existing = properties(ref context);
                    //        //context.Existing = methods(ref context);
                    //        return context.Existing;
                    //    };

                    //    // Cleanup

                    //    return buildMethod(ref c);
                    //};
                }
                else if (type.IsConstructedGenericType)
                {
                    var factory = registration.Get<ResolveMethodFactory<Type>>();
                    ResolveMethod buildMethod = factory?.Invoke(type);

                    // Cleanup
                }

                // Build rest of pipeline
                next?.Invoke(unity, set, args);

                // TODO: set.Clear(typeof(SelectConstructor)); // Release injectors and such
                // TODO: Asynchronously create and compile expression

                // Build
                var pipeline = ((InternalRegistration)set).ResolveMethod;
                ((InternalRegistration)set).ResolveMethod = (ref ResolutionContext context) =>
                {
                    context.Existing = pipeline?.Invoke(ref context);// ??
                    //                   buildMethod(ref context);

                    return context.Existing;
                };
            };
        }
    }
}
