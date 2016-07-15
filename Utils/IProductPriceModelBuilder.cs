using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;

namespace Nop.HomePageNewProductsPlugin.Utils
{
    public interface IProductPriceModelBuilder
    {
        ProductOverviewModel.ProductPriceModel Build(Product product);
    }
}
