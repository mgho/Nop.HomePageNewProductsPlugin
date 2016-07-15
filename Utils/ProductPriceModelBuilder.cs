using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Models.Catalog;

namespace Nop.HomePageNewProductsPlugin.Utils
{
    public class ProductPriceModelBuilder : IProductPriceModelBuilder
    {
        private bool _forceRedirectionAfterAddingToCart = false;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly IPermissionService _permissionService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;

        public ProductPriceModelBuilder(
            IProductService productService,
            IStoreContext storeContext,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter,
            ITaxService taxService,
            ICurrencyService currencyService,
            CatalogSettings catalogSettings)
        {
            _productService = productService;
            _storeContext = storeContext;
            _catalogSettings = catalogSettings;
            _priceCalculationService = priceCalculationService;
            _workContext = workContext;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _taxService = taxService;
            _currencyService = currencyService;
            _permissionService = permissionService;
        }

        public ProductOverviewModel.ProductPriceModel Build(Product product)
        {
            #region Prepare product price

            var priceModel = new ProductOverviewModel.ProductPriceModel
            {
                ForceRedirectionAfterAddingToCart = _forceRedirectionAfterAddingToCart
            };

            switch (product.ProductType)
            {
                case ProductType.GroupedProduct:
                    {
                        #region Grouped product

                        var associatedProducts = _productService.GetAssociatedProducts(product.Id, _storeContext.CurrentStore.Id);

                        switch (associatedProducts.Count)
                        {
                            case 0:
                                {
                                    //no associated products
                                    //priceModel.DisableBuyButton = true;
                                    //priceModel.DisableWishlistButton = true;
                                    //compare products
                                    priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
                                    //priceModel.AvailableForPreOrder = false;
                                }
                                break;
                            default:
                                {
                                    //we have at least one associated product
                                    //priceModel.DisableBuyButton = true;
                                    //priceModel.DisableWishlistButton = true;
                                    //compare products
                                    priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
                                    //priceModel.AvailableForPreOrder = false;

                                    if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                                    {
                                        //find a minimum possible price
                                        decimal? minPossiblePrice = null;
                                        Product minPriceProduct = null;
                                        foreach (var associatedProduct in associatedProducts)
                                        {
                                            //calculate for the maximum quantity (in case if we have tier prices)
                                            var tmpPrice = _priceCalculationService.GetFinalPrice(associatedProduct,
                                                _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);
                                            if (!minPossiblePrice.HasValue || tmpPrice < minPossiblePrice.Value)
                                            {
                                                minPriceProduct = associatedProduct;
                                                minPossiblePrice = tmpPrice;
                                            }
                                        }
                                        if (minPriceProduct != null && !minPriceProduct.CustomerEntersPrice)
                                        {
                                            if (minPriceProduct.CallForPrice)
                                            {
                                                priceModel.OldPrice = null;
                                                priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
                                            }
                                            else if (minPossiblePrice.HasValue)
                                            {
                                                //calculate prices
                                                decimal taxRate;
                                                decimal finalPriceBase = _taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, out taxRate);
                                                decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                                priceModel.OldPrice = null;
                                                priceModel.Price = String.Format(_localizationService.GetResource("Products.PriceRangeFrom"), _priceFormatter.FormatPrice(finalPrice));
                                                priceModel.PriceValue = finalPrice;
                                            }
                                            else
                                            {
                                                //Actually it's not possible (we presume that minimalPrice always has a value)
                                                //We never should get here
                                                Debug.WriteLine("Cannot calculate minPrice for product #{0}", product.Id);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //hide prices
                                        priceModel.OldPrice = null;
                                        priceModel.Price = null;
                                    }
                                }
                                break;
                        }

                        #endregion
                    }
                    break;
                case ProductType.SimpleProduct:
                default:
                    {
                        #region Simple product

                        //add to cart button
                        priceModel.DisableBuyButton = product.DisableBuyButton ||
                            !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
                            !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                        //add to wishlist button
                        priceModel.DisableWishlistButton = product.DisableWishlistButton ||
                            !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
                            !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
                        //compare products
                        priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

                        //rental
                        priceModel.IsRental = product.IsRental;

                        //pre-order
                        if (product.AvailableForPreOrder)
                        {
                            priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                            priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
                        }

                        //prices
                        if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            if (!product.CustomerEntersPrice)
                            {
                                if (product.CallForPrice)
                                {
                                    //call for price
                                    priceModel.OldPrice = null;
                                    priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
                                }
                                else
                                {
                                    //prices

                                    //calculate for the maximum quantity (in case if we have tier prices)
                                    decimal minPossiblePrice = _priceCalculationService.GetFinalPrice(product,
                                        _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);

                                    decimal taxRate;
                                    decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);
                                    decimal finalPriceBase = _taxService.GetProductPrice(product, minPossiblePrice, out taxRate);

                                    decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                                    decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                    //do we have tier prices configured?
                                    var tierPrices = new List<TierPrice>();
                                    if (product.HasTierPrices)
                                    {
                                        tierPrices.AddRange(product.TierPrices
                                            .OrderBy(tp => tp.Quantity)
                                            .ToList()
                                            .FilterByStore(_storeContext.CurrentStore.Id)
                                            .FilterForCustomer(_workContext.CurrentCustomer)
                                            .RemoveDuplicatedQuantities());
                                    }
                                    //When there is just one tier (with  qty 1), 
                                    //there are no actual savings in the list.
                                    bool displayFromMessage = tierPrices.Count > 0 &&
                                        !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                                    if (displayFromMessage)
                                    {
                                        priceModel.OldPrice = null;
                                        priceModel.Price = String.Format(_localizationService.GetResource("Products.PriceRangeFrom"), _priceFormatter.FormatPrice(finalPrice));
                                        priceModel.PriceValue = finalPrice;
                                    }
                                    else
                                    {
                                        if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
                                        {
                                            priceModel.OldPrice = _priceFormatter.FormatPrice(oldPrice);
                                            priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
                                            priceModel.PriceValue = finalPrice;
                                        }
                                        else
                                        {
                                            priceModel.OldPrice = null;
                                            priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
                                            priceModel.PriceValue = finalPrice;
                                        }
                                    }
                                    if (product.IsRental)
                                    {
                                        //rental product
                                        priceModel.OldPrice = _priceFormatter.FormatRentalProductPeriod(product, priceModel.OldPrice);
                                        priceModel.Price = _priceFormatter.FormatRentalProductPeriod(product, priceModel.Price);
                                    }


                                    //property for German market
                                    //we display tax/shipping info only with "shipping enabled" for this product
                                    //we also ensure this it's not free shipping
                                    priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes
                                        && product.IsShipEnabled &&
                                        !product.IsFreeShipping;
                                }
                            }
                        }
                        else
                        {
                            //hide prices
                            priceModel.OldPrice = null;
                            priceModel.Price = null;
                        }

                        #endregion
                    }
                    break;
            }
            
            #endregion

            return priceModel;
        }
    }
}