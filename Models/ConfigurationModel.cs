using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.HomePageNewProductsPlugin.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("HomePageNewProductsPlugin.NumberOfRecentlyAddedProducts")]
        public int NumberOfRecentlyAddedProducts { get; set; }
        public bool NumberOfRecentlyAddedProducts_OverrideForStore { get; set; }

        [NopResourceDisplayName("HomePageNewProductsPlugin.WidgetZone")]
        public string WidgetZone { get; set; }
        public bool WidgetZone_OverrideForStore { get; set; }
        [NopResourceDisplayName("HomePageNewProductsPlugin.WidgetZoneValues")]
        public SelectList WidgetZoneValues { get; set; }
    }
}
