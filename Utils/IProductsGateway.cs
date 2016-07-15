using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;

namespace Nop.HomePageNewProductsPlugin.Utils
{
    public interface IProductsGateway
    {
        IEnumerable<Product> LoadRecentProducts(int count);
        IEnumerable<ProductOverviewModel> LoadRecentProductsOverview(int count);
    }
}
