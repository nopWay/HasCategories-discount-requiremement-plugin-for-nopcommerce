using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Catalog;
using System.Collections.Generic;

namespace Nop.Plugin.DiscountRules.HasCategories
{
    public partial class HasCategoriesDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        private readonly ISettingService _settingService;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;

        public HasCategoriesDiscountRequirementRule(ISettingService settingService,
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            ICategoryService categoryService,
            ILocalizationService localizationService)
        {
            this._settingService = settingService;
            this._actionContextAccessor = actionContextAccessor;
            this._urlHelperFactory = urlHelperFactory;
            this._categoryService = categoryService;
            this._localizationService = localizationService;
        }

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            var productQuantityMin = _settingService.GetSettingByKey<int>($"DiscountRequirement.ProductQuantityMin-{request.DiscountRequirementId}");
            var productQuantityMax = _settingService.GetSettingByKey<int>($"DiscountRequirement.ProductQuantityMax-{request.DiscountRequirementId}");
            var restrictedCategoryIds = _settingService.GetSettingByKey<string>($"DiscountRequirement.RestrictedCategoryIds-{request.DiscountRequirementId}");
            var excludedProductIds = _settingService.GetSettingByKey<string>(string.Format("DiscountRequirement.ExcludedProductIds-{0}", request.DiscountRequirementId));

            if (string.IsNullOrWhiteSpace(restrictedCategoryIds))
                return result;

            if (productQuantityMin <= 0 || productQuantityMax <= 0 || productQuantityMin > productQuantityMax)
                return result;

            if (request.Customer == null)
                return result;

            //we support comma-separated list of category identifiers (e.g. 77, 123, 156).
            var restrictedCategories = restrictedCategoryIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();
            if (!restrictedCategories.Any())
                return result;

            var excludedProducts = (excludedProductIds ?? String.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            //group products in the cart by product ID
            //it could be the same product with distinct product attributes
            //that's why we get the total quantity of this product
            var cartQuery = from sci in request.Customer.ShoppingCartItems.LimitPerStore(request.Store.Id)
                            where sci.ShoppingCartType == ShoppingCartType.ShoppingCart
                            group sci by sci.ProductId into g
                            select new { ProductId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) };
            var cart = cartQuery.ToList();
            var totalQuantity = 0;
            var productIds = new List<int>();

            foreach (var sci in cart)
                productIds.Add(sci.ProductId);

            var productCategoryIds = _categoryService.GetProductCategoryIds(productIds.ToArray());

            foreach (var sci in cart)
            {
                if (excludedProducts.Any(id => sci.ProductId.ToString() == id) == false)
                {
                    productCategoryIds.TryGetValue(sci.ProductId, out int[] categories);

                    if (categories != null && categories.Length > 0)
                    {
                        var isProductInRestrictedCategory = false;
                        for (int i = 0; i < categories.Length; i++)
                        {
                            if (isProductInRestrictedCategory == false &&
                                restrictedCategories.Any(id => id == categories[i].ToString()))
                            {
                                totalQuantity += sci.TotalQuantity;
                                isProductInRestrictedCategory = true;
                                if (totalQuantity > productQuantityMax)
                                    return result;
                            }
                        }
                    }
                }
            }

            result.IsValid = totalQuantity >= productQuantityMin && totalQuantity <= productQuantityMax;
            return result;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            return urlHelper.Action("Configure", "DiscountRulesHasCategories",
                new { discountId, discountRequirementId }).TrimStart('/');
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Categories", "Categories");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Categories.Hint", "The comma-separated list of category identifiers (e.g. 77, 123, 156). Quantity and range aren't applicable here. You can find a category ID on its details page.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.ExcludedProducts", "Excluded Products");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.ExcludedProducts.Hint", "The comma-separated list of excluded product identifiers (e.g. 77, 123, 156). Quantity and range aren't applicable here. You can find a product ID on its details page.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Quantity.Min", "Minimum quantity");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Quantity.Min.Hint", "Discount will be applied if cart contains more products in selected categories than the defined value here. Minimum quantity should be greater than zero.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Quantity.Max", "Maximum quantity");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Quantity.Max.Hint", "Discount will be applied if cart contains fewer products in selected categories than the defined value here. Maximum quantity should be greater than zero and minimum quantity.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Multiple.Selected", "{0} categories selected");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Single.Selected", "One category selected");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Not.Selected", "No categories selected");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Multiple.Excluded", "{0} products excluded");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Single.Excluded", "One product excluded");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Not.Excluded", "No products excluded");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.AddCategory", "Add category");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.ExcludeProduct", "Exclude product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.AddNew", "Add category");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Choose", "Choose");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.ViewSelectedCategories", "View Selected Categories");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.ViewExcludedProducts", "View Excluded Products");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasCategories.Error.SelectCategory", "Please select at least one category before excluding products");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Categories");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Categories.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.ExcludedProducts");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.ExcludedProducts.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Quantity.Min");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Quantity.Min.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Quantity.Max");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Fields.Quantity.Max.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Multiple.Selected");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Single.Selected");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Not.Selected");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Multiple.Excluded");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Single.Excluded");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Not.Excluded");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.AddCategory");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.ExcludeProduct");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Choose");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.ViewSelectedCategories");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.ViewExcludedProducts");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasCategories.Error.SelectCategory");

            base.Uninstall();
        }
    }
}