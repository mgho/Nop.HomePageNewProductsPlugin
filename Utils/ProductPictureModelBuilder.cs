using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Media;
using Nop.Web.Models.Media;

namespace Nop.HomePageNewProductsPlugin.Utils
{
    public class ProductPictureModelBuilder : IProductPictureModelBuilder
    {
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;

        public ProductPictureModelBuilder(IPictureService pictureService, MediaSettings mediaSettings)
        {
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
        }

        public PictureModel Build(Product product)
        {
            var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();

            var pictureModel = new PictureModel
            {
                ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductThumbPictureSize),
                FullSizeImageUrl = _pictureService.GetPictureUrl(picture)
            };

            return pictureModel;
        }
    }
}
