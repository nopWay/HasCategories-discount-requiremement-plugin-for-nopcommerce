using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.DiscountRules.HasCategories.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.DiscountRules.HasCategories.Configure",
                "Plugins/DiscountRulesHasCategories/Configure",
                new { controller = "DiscountRulesHasCategories", action = "Configure" });

            routeBuilder.MapRoute("Plugin.DiscountRules.HasCategories.CategoryAddPopup",
                "Plugins/DiscountRulesHasCategories/CategoryAddPopup",
                new { controller = "DiscountRulesHasCategories", action = "CategoryAddPopup" });

            routeBuilder.MapRoute("Plugin.DiscountRules.HasCategories.CategoryAddPopupList",
                "Plugins/DiscountRulesHasCategories/CategoryAddPopupList",
                new { controller = "DiscountRulesHasCategories", action = "CategoryAddPopupList" });

            routeBuilder.MapRoute("Plugin.DiscountRules.HasCategories.CategorySelectedPopupList",
                "Plugins/DiscountRulesHasCategories/CategorySelectedPopupList",
                new { controller = "DiscountRulesHasCategories", action = "CategorySelectedPopupList" });

            routeBuilder.MapRoute("Plugin.DiscountRules.HasCategories.ProductExcludePopup",
                "Plugins/DiscountRulesHasCategories/ProductExcludePopup",
                new { controller = "DiscountRulesHasCategories", action = "ProductExcludePopup" });

            routeBuilder.MapRoute("Plugin.DiscountRules.HasCategories.ProductExcludePopupList",
                "Plugins/DiscountRulesHasCategories/ProductExcludePopupList",
                new { controller = "DiscountRulesHasCategories", action = "ProductExcludePopupList" });

            routeBuilder.MapRoute("Plugin.DiscountRules.HasCategories.ProductExcludedPopupList",
                "Plugins/DiscountRulesHasCategories/ProductExcludedPopupList",
                new { controller = "DiscountRulesHasCategories", action = "ProductExcludedPopupList" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}
