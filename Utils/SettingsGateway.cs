using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.HomePageNewProductsPlugin.Models;
using Nop.Services.Configuration;

namespace Nop.HomePageNewProductsPlugin.Utils
{
    public class SettingsGateway
    {
        private readonly HttpServerUtilityBase _server;
        private readonly ISettingService _settingService;
        private readonly Func<int> _storeScopeLoader;

        public SettingsGateway(
            HttpServerUtilityBase server,
            ISettingService settingService,
            Func<int> storeScopeLoader)
        {
            _server = server;
            _settingService = settingService;
            _storeScopeLoader = storeScopeLoader;
        }
        
        public HomePageNewProductsSettings LoadSettings()
        {
            var storeScope = _storeScopeLoader.Invoke();
            return _settingService.LoadSetting<HomePageNewProductsSettings>(storeScope);
        }

        public ConfigurationModel LoadConfiguration()
        {
            var storeScope = _storeScopeLoader.Invoke();
            var settings = _settingService.LoadSetting<HomePageNewProductsSettings>(storeScope);

            var model = new ConfigurationModel();

            model.NumberOfRecentlyAddedProducts = settings.NumberOfRecentlyAddedProducts;
            model.WidgetZone = settings.WidgetZone;
            model.WidgetZoneValues = new SelectList(GetHomePageWidgetList(@"~/Views/Home/Index.cshtml"));

            if (storeScope > 0)
            {
                model.NumberOfRecentlyAddedProducts_OverrideForStore = _settingService
                    .SettingExists(settings, x => x.NumberOfRecentlyAddedProducts, storeScope);

                model.WidgetZone_OverrideForStore = _settingService
                    .SettingExists(settings, x => x.WidgetZone, storeScope);
            }

            return model;
        }


        public void SaveConfiguration(ConfigurationModel configuration)
        {
            var storeScope = _storeScopeLoader.Invoke();
            var newProductsSettings = _settingService.LoadSetting<HomePageNewProductsSettings>(storeScope);

            //save settings
            newProductsSettings.NumberOfRecentlyAddedProducts = configuration.NumberOfRecentlyAddedProducts;
            newProductsSettings.WidgetZone = configuration.WidgetZone;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (configuration.NumberOfRecentlyAddedProducts_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(newProductsSettings, x => x.NumberOfRecentlyAddedProducts, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(newProductsSettings, x => x.NumberOfRecentlyAddedProducts, storeScope);

            if (configuration.WidgetZone_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(newProductsSettings, x => x.WidgetZone, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(newProductsSettings, x => x.WidgetZone, storeScope);


            _settingService.ClearCache();
        }

        
        private IEnumerable<string> GetHomePageWidgetList(string homeIndex)
        {
            var fallBackList = new List<string>
                {
                    "home_page_top",
                    "home_page_before_categories",
                    "home_page_before_products",
                    "home_page_before_best_sellers",
                    "home_page_before_news",
                    "home_page_before_poll",
                    "home_page_bottom"
                };

            IEnumerable<string> widgets;

            try
            {
                var fileContents = System.IO.File.ReadLines(_server.MapPath(homeIndex));

                widgets = fileContents
                    .Where(l => l.Contains("@Html.Widget"))
                    .Select(w => w.Split('"')[1]);
            }
            catch (Exception)
            {
                widgets = fallBackList;
            }

            return widgets;
        }
    }
}
