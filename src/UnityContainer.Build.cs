using System;
using System.Linq;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Policy;
using Unity.Registration;

namespace Unity
{
    public partial class UnityContainer
    {
        public static RegisterPipeline RegistrationBuildAspect(RegisterPipeline next)
        {
            return (IUnityContainer unity, IPolicySet set, Type type, string name) =>
            {
                // Build rest of pipeline
                next?.Invoke(unity, set, type, name);

                // Prepare
                var registration = (InternalRegistration)set;
                var container = (UnityContainer)unity;

                var ctor = container._constructorPipeline(container, registration);
                var methods = container._methodsPipeline(container, registration).Distinct();
                var properties = container._propertiesPipeline(container, registration).Distinct();


                // Create Build Method
                ResolveMethod buildMethod;

                if (type.GetTypeInfo().IsGenericTypeDefinition)
                {
                    // Create build method during first resolve
                    buildMethod = (ref ResolutionContext c) =>
                    {
                        //ResolveMethod createMethod = ctor.ResolveMethodFactory(type);
                        //Resolve properties
                        //Resolve methods

                        buildMethod = (ref ResolutionContext context) =>
                        {
                            //context.Existing = createMethod(ref context);
                            //context.Existing = properties(ref context);
                            //context.Existing = methods(ref context);
                            return context.Existing;
                        };

                        // Cleanup

                        return buildMethod(ref c);
                    };
                }
                else
                {
                    // Create build method
                    //ResolveMethod create = ctor.ResolveMethodFactory(type);
                    //Resolve properties
                    //Resolve methods

                    buildMethod = (ref ResolutionContext context) =>
                    {
                        //context.Existing = create(ref context);
                        //context.Existing = properties(ref context);
                        //methods(ref context);
                        return context.Existing;
                    };

                    // Cleanup
                    if (!((InternalRegistration)set).EnableOptimization)
                    {
                        ctor = null;
                    }
                }

                // TODO: set.Clear(typeof(SelectConstructor)); // Release injectors and such
                // TODO: Asynchronously create and compile expression

                // Build
                var pipeline = ((InternalRegistration)set).ResolveMethod;
                ((InternalRegistration)set).ResolveMethod = (ref ResolutionContext context) =>
                {
                    context.Existing = pipeline?.Invoke(ref context) ??
                                       buildMethod(ref context);

                    return context.Existing;
                };
            };
        }
    }
}
