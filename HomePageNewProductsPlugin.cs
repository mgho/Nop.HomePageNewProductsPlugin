using System.Collections.Generic;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.HomePageNewProductsPlugin
{
    public class HomePageNewProductsPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly ISettingService _settingService;

        public HomePageNewProductsPlugin(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public IList<string> GetWidgetZones()
        {
            var newProductsSettings = _settingService.LoadSetting<HomePageNewProductsSettings>();

            return new List<string>
            {
                newProductsSettings.WidgetZone
            };
        }

        public void GetDisplayWidgetRoute(
            string widgetZone,
            out string actionName,
            out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "NewProducts";
            controllerName = "HomePageNewProducts";
            routeValues = new RouteValueDictionary()
            {
                { "Namespaces", "Nop.HomePageNewProductsPlugin.Controllers" },
                { "area", null }
            };
        }

        public void GetConfigurationRoute(
            out string actionName,
            out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "HomePageNewProducts";
            routeValues = new RouteValueDictionary()
            {
                { "Namespaces", "Nop.HomePageNewProductsPlugin.Controllers" },
                { "area", null }
            };
        }

        public override void Install()
        {
            //settings
            var settings = new HomePageNewProductsSettings
            {
                NumberOfRecentlyAddedProducts = 3,
                WidgetZone = "home_page_top"
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("HomePageNewProductsPlugin.NumberOfRecentlyAddedProducts", "Number Of Recently Added Products");
            this.AddOrUpdatePluginLocaleResource("HomePageNewProductsPlugin.NumberOfRecentlyAddedProducts.Hint", "How many recently added products should be shown.");
            this.AddOrUpdatePluginLocaleResource("HomePageNewProductsPlugin.WidgetZoneValues", "Widget Zones");
            this.AddOrUpdatePluginLocaleResource("HomePageNewProductsPlugin.WidgetZoneValues.Hint", "Widget zone on the home page to show recently added products.");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<HomePageNewProductsSettings>();

            //locales
            this.DeletePluginLocaleResource("HomePageNewProductsPlugin.NumberOfRecentlyAddedProducts");
            this.DeletePluginLocaleResource("HomePageNewProductsPlugin.NumberOfRecentlyAddedProducts.Hint");
            this.DeletePluginLocaleResource("HomePageNewProductsPlugin.WidgetZoneValues");
            this.DeletePluginLocaleResource("HomePageNewProductsPlugin.WidgetZoneValues.Hint");

            base.Uninstall();
        }
    }
}
