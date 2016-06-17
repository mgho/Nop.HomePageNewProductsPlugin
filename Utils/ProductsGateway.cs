using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Models.Catalog;

namespace Nop.HomePageNewProductsPlugin.Utils
{
    public class ProductsGateway
    {
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly ProductPictureModelBuilder _pictureModelBuilder;
        private readonly ProductPriceModelBuilder _priceModelBuilder;

        public ProductsGateway(
            IProductService productService,
            IStoreContext storeContext,
            ProductPictureModelBuilder pictureModelBuilder,
            ProductPriceModelBuilder priceModelBuilder)
        {
            _productService = productService;
            _storeContext = storeContext;
            _pictureModelBuilder = pictureModelBuilder;
            _priceModelBuilder = priceModelBuilder;
        }

        public IEnumerable<Product> LoadRecentProducts(int count)
        {
            return _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                //markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: count);
        }

        public IEnumerable<ProductOverviewModel> LoadRecentProductsOverview(int count)
        {
            return LoadRecentProducts(count)
                .Select(product => new ProductOverviewModel
                {
                    Id = product.Id,
                    Name = product.GetLocalized(x => x.Name),
                    ShortDescription = product.GetLocalized(x => x.ShortDescription),
                    FullDescription = product.GetLocalized(x => x.FullDescription),
                    SeName = product.GetSeName(),
                    MarkAsNew = true,
                    DefaultPictureModel = _pictureModelBuilder.Build(product),
                    ProductPrice = _priceModelBuilder.Build(product)
                });
        }
    }
}
