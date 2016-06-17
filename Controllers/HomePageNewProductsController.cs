using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.HomePageNewProductsPlugin.Models;
using Nop.HomePageNewProductsPlugin.Utils;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework.Controllers;

namespace Nop.HomePageNewProductsPlugin.Controllers
{
    public class HomePageNewProductsController : BasePluginController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ProductsGateway _productsGateway;
        private readonly SettingsGateway _settingsGateway;

        public HomePageNewProductsController(IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService,
            IProductService productService,
            IPictureService pictureService,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            ITaxService taxService,
            ICurrencyService currencyService,
            CatalogSettings catalogSettings,
            MediaSettings mediaSettings)
        {
            _localizationService = localizationService;

            _settingsGateway = new SettingsGateway(
                Server,
                settingService,
                () => GetActiveStoreScopeConfiguration(storeService, workContext));

            var pictureModelBuilder = new ProductPictureModelBuilder(pictureService, mediaSettings);

            var priceModelBuilder = new ProductPriceModelBuilder(productService,
                storeContext,
                permissionService,
                priceCalculationService,
                workContext,
                localizationService,
                priceFormatter,
                taxService,
                currencyService,
                catalogSettings);
            
            _productsGateway = new ProductsGateway(
                productService,
                storeContext,
                pictureModelBuilder,
                priceModelBuilder);
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = _settingsGateway.LoadConfiguration();

            return View("~/Plugins/HomePageNewProducts/Views/HomePageNewProducts/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel configuration)
        {
            _settingsGateway.SaveConfiguration(configuration);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        [ChildActionOnly]
        public ActionResult NewProducts()
        {
            var settings = _settingsGateway.LoadSettings();

            var model = _productsGateway
                .LoadRecentProductsOverview(settings.NumberOfRecentlyAddedProducts);

            return View("~/Plugins/HomePageNewProducts/Views/HomePageNewProducts/NewProducts.cshtml", model);
        }
    }
}