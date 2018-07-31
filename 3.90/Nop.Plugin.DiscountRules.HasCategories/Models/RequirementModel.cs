using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.DiscountRules.HasCategories.Models
{
    public class RequirementModel
    {
        [NopResourceDisplayName("Plugins.DiscountRules.HasCategories.Fields.Categories")]
        public string Categories { get; set; }

        [NopResourceDisplayName("Plugins.DiscountRules.HasCategories.Fields.ExcludedProducts")]
        public string ExcludedProducts { get; set; }

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

        public partial class AddProductModel : BaseNopModel
        {
            public AddProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableStores = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            public string SearchProductName { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public int SearchCategoryId { get; set; }
            public string SearchSelectedCategoryIds { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public int SearchManufacturerId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
            public int SearchStoreId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
            public int SearchVendorId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            //vendor
            public bool IsLoggedInAsVendor { get; set; }
        }

        public partial class ProductModel : BaseNopEntityModel
        {
            public string Name { get; set; }

            public bool Published { get; set; }
        }
        #endregion
    }
}