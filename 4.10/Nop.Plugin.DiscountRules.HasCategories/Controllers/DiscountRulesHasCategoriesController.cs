using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.HasCategories.Models;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.DiscountRules.HasCategories.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class DiscountRulesHasCategoriesController : BasePluginController
    {
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IProductService _productService;

        public DiscountRulesHasCategoriesController(IDiscountService discountService,
            ISettingService settingService,
            IPermissionService permissionService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            IProductService productService)
        {
            this._discountService = discountService;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._productService = productService;
        }

        public IActionResult Configure(int discountId, int? discountRequirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            if (discountRequirementId.HasValue)
            {
                var discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId.Value);
                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

            var requirementId = discountRequirementId ?? 0;
            var model = new RequirementModel
            {
                RequirementId = requirementId,
                DiscountId = discountId,
                Categories = _settingService.GetSettingByKey<string>($"DiscountRequirement.RestrictedCategoryIds-{requirementId}"),
                ExcludedProducts = _settingService.GetSettingByKey<string>(string.Format("DiscountRequirement.ExcludedProductIds-{0}", requirementId)),
                ProductQuantityMin = _settingService.GetSettingByKey<int>($"DiscountRequirement.ProductQuantityMin-{requirementId}"),
                ProductQuantityMax = _settingService.GetSettingByKey<int>($"DiscountRequirement.ProductQuantityMax-{requirementId}")
            };

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = $"DiscountRulesHasCategories{requirementId}";

            return View("~/Plugins/DiscountRules.HasCategories/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(int discountId, int? discountRequirementId, int productQuantityMin, int productQuantityMax, string categoryIds, string excludedProductIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Json(new { Result = false, ErrorMessage = "Access denied" });

            if (string.IsNullOrEmpty(categoryIds))
                return Json(new { Result = false, ErrorMessage = "Please select at least one category" });

            if (productQuantityMin <= 0 || productQuantityMax <= 0)
                return Json(new { Result = false, ErrorMessage = "Minimum and maximum quantities should be greater than zero" });

            if (productQuantityMin > productQuantityMax)
                return Json(new { Result = false, ErrorMessage = "Max quantity should be greater than minimum quantity" });

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                return Json(new { Result = false, ErrorMessage = "Discount could not be loaded" });

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
                discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId.Value);

            if (discountRequirement == null)
            {
                discountRequirement = new DiscountRequirement
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirement.HasCategories"
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
            }


            _settingService.SetSetting($"DiscountRequirement.ProductQuantityMin-{discountRequirement.Id}", productQuantityMin);
            _settingService.SetSetting($"DiscountRequirement.ProductQuantityMax-{discountRequirement.Id}", productQuantityMax);
            _settingService.SetSetting($"DiscountRequirement.RestrictedCategoryIds-{discountRequirement.Id}", categoryIds);
            _settingService.SetSetting(string.Format("DiscountRequirement.ExcludedProductIds-{0}", discountRequirement.Id), excludedProductIds);

            return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        }

        public IActionResult CategoryAddPopup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return Content("Access denied");

            var model = new RequirementModel.AddCategoryModel();

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            return View("~/Plugins/DiscountRules.HasCategories/Views/CategoryAddPopup.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult CategoryAddPopupList(DataSourceRequest command, RequirementModel.AddCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return Content("Access denied");

            var categories = _categoryService.GetAllCategories(
                model.SearchCategoryName,
                model.SearchStoreId,
                command.Page - 1,
                command.PageSize,
                true
                );

            return Json(GetCategoryGridModel(categories));
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult CategorySelectedPopupList(string selectedCategoryIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return Content("Access denied");

            var categories = GetCategoriesByIds(selectedCategoryIds);
            return Json(GetCategoryGridModel(categories));
        }

        public ActionResult ProductExcludePopup(string selectedCategoryIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("Access denied");

            if (string.IsNullOrEmpty(selectedCategoryIds))
                return Content("Please select at least one category before excluding products");

            var model = new RequirementModel.AddProductModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = GetCategoriesByIds(selectedCategoryIds);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View("~/Plugins/DiscountRules.HasCategories/Views/ProductExcludePopup.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public ActionResult ProductExcludePopupList(DataSourceRequest command, RequirementModel.AddProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("Access denied");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var selectedCategoryIds = (model.SearchSelectedCategoryIds ?? String.Empty).ToString();
            var products = _productService.SearchProducts(
                categoryIds: model.SearchCategoryId == 0 ? GetIdArr(selectedCategoryIds).Select(int.Parse).ToList() : new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );

            return Json(GetProductGridModel(products, products.TotalCount));
        }

        [HttpPost]
        [AdminAntiForgery]
        public ActionResult ProductExcludedPopupList(string selectedProductIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("Access denied");

            var products = _productService.GetProductsByIds(Array.ConvertAll(selectedProductIds.Split(','), int.Parse));
            return Json(GetProductGridModel(products, products.Count()));
        }

        private DataSourceResult GetProductGridModel(IList<Product> products, int total)
        {
            return new DataSourceResult
            {
                Data = products.Select(x => new RequirementModel.ProductModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Published = x.Published
                }),
                Total = total
            };
        }

        private DataSourceResult GetCategoryGridModel(IPagedList<Category> categories)
        {
            return new DataSourceResult
            {
                Data = categories.Select(x => new RequirementModel.CategoryModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Published = x.Published,
                    Breadcrumb = _categoryService.GetFormattedBreadCrumb(x)
                }),
                Total = categories.TotalCount
            };
        }

        private IPagedList<Category> GetCategoriesByIds(string ids)
        {
            var categoryIds = GetIdArr(ids);
            var allCategories = _categoryService.GetAllCategories(showHidden: true);
            var categories = allCategories.Where(c => categoryIds.Any(id => id.Equals(c.Id.ToString())));
            return new PagedList<Category>(categories, 0, categories.Count(), categories.Count());
        }

        private List<string> GetIdArr(string ids)
        {
            //we support comma-separated list of category identifiers (e.g. 77, 123, 156).
            return ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();
        }
    }
}