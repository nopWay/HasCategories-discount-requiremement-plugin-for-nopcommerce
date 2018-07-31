using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.DiscountRules.HasCategories
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.HasCategories.Configure",
                 "Plugins/DiscountRulesHasCategories/Configure",
                 new { controller = "DiscountRulesHasCategories", action = "Configure" },
                 new[] { "Nop.Plugin.DiscountRules.HasCategories.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasCategories.CategoryAddPopup",
                 "Plugins/DiscountRulesHasCategories/CategoryAddPopup",
                 new { controller = "DiscountRulesHasCategories", action = "CategoryAddPopup" },
                 new[] { "Nop.Plugin.DiscountRules.HasCategories.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasCategories.CategoryAddPopupList",
                 "Plugins/DiscountRulesHasCategories/CategoryAddPopupList",
                 new { controller = "DiscountRulesHasCategories", action = "CategoryAddPopupList" },
                 new[] { "Nop.Plugin.DiscountRules.HasCategories.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasCategories.CategorySelectedPopupList",
                "Plugins/DiscountRulesHasCategories/CategorySelectedPopupList",
                new { controller = "DiscountRulesHasCategories", action = "CategorySelectedPopupList" },
                new[] { "Nop.Plugin.DiscountRules.HasCategories.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasCategories.ProductExcludePopup",
                 "Plugins/DiscountRulesHasCategories/ProductExcludePopup",
                 new { controller = "DiscountRulesHasCategories", action = "ProductExcludePopup" },
                 new[] { "Nop.Plugin.DiscountRules.HasCategories.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasCategories.ProductExcludePopupList",
                 "Plugins/DiscountRulesHasCategories/ProductExcludePopupList",
                 new { controller = "DiscountRulesHasCategories", action = "ProductExcludePopupList" },
                 new[] { "Nop.Plugin.DiscountRules.HasCategories.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasCategories.ProductExcludedPopupList",
                "Plugins/DiscountRulesHasCategories/ProductExcludedPopupList",
                new { controller = "DiscountRulesHasCategories", action = "ProductExcludedPopupList" },
                new[] { "Nop.Plugin.DiscountRules.HasCategories.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
