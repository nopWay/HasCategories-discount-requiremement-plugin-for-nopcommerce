using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.DiscountRules.HasCategories.Models
{
    public class RequirementModel
    {
        [NopResourceDisplayName("Plugins.DiscountRules.HasCategories.Fields.Categories")]
        public string Categories { get; set; }

        [NopResourceDisplayName("Plugins.DiscountRules.HasCategories.Fields.Quantity.Min")]
        public int ProductQuantityMin { get; set; }

        [NopResourceDisplayName("Plugins.DiscountRules.HasCategories.Fields.Quantity.Max")]
        public int ProductQuantityMax { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        #region Nested classes

        public partial class AddCategoryModel : BaseNopModel
        {
            public AddCategoryModel()
            {
                AvailableStores = new List<SelectListItem>();
            }

            [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
            public string SearchCategoryName { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchStore")]
            public int SearchStoreId { get; set; }

            public IList<SelectListItem> AvailableStores { get; set; }
        }

        public partial class CategoryModel : BaseNopEntityModel
        {
            public string Name { get; set; }

            public string Breadcrumb { get; set; }

            public bool Published { get; set; }
        }

        #endregion
    }
}