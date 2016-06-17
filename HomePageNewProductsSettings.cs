using Nop.Core.Configuration;

namespace Nop.HomePageNewProductsPlugin
{
    public class HomePageNewProductsSettings : ISettings
    {
        public int NumberOfRecentlyAddedProducts { get; set; }
        public string WidgetZone { get; set; }
    }
}
