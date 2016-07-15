using System.Web.Mvc;
using Nop.Core;
using Nop.HomePageNewProductsPlugin.Models;
using Nop.HomePageNewProductsPlugin.Utils;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.HomePageNewProductsPlugin.Controllers
{
    public class HomePageNewProductsController : BasePluginController
    {
        private readonly ILocalizationService _localizationService;
        private readonly IProductsGateway _productsGateway;
        private readonly SettingsGateway _settingsGateway;

        public HomePageNewProductsController(
            IProductsGateway productsGateway,
            IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            _localizationService = localizationService;

            _settingsGateway = new SettingsGateway(
                Server,
                settingService,
                () => GetActiveStoreScopeConfiguration(storeService, workContext));

            _productsGateway = productsGateway;
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