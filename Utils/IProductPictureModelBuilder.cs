using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Media;

namespace Nop.HomePageNewProductsPlugin.Utils
{
    public interface IProductPictureModelBuilder
    {
        PictureModel Build(Product product);
    }
}
