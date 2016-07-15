using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.HomePageNewProductsPlugin.Utils;

namespace Nop.HomePageNewProductsPlugin
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ProductPictureModelBuilder>().As<IProductPictureModelBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<ProductPriceModelBuilder>().As<IProductPriceModelBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<ProductsGateway>().As<IProductsGateway>().InstancePerLifetimeScope();
        }

        public int Order => 2;
    }
}
