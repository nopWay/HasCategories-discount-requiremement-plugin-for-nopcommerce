# nopCommerce "Has categories" discount requirement plugin
With this nopCommerce plugin, you can create discount requirements that checks the total quantity of products in selected categories against a predefined range. For example, you can select 2 categories (let's say Category A and B) and set minimum and maximum quantities to 3 and 5 respectively. In this case, discount will be applied if customer's cart contains any products belonging any of these categories with a total quantity of 3, 4 or 5, so to get this discount a customer can buy 3 pieces of products in Category A or 2 pieces of prodcuts in Category A and 1 piece of product in Category B. By this way, it's really easy to create discounts like

* Buy X of products in these categories and get Y free
* Buy X of products in these categories and get Y% discount

In addition to the discount requirement, you will get a user friendly category selection window. While selecting categories, you don't need to leave selection window anymore. All of your selections will be kept even if you perform a search or go to another page by using pagination. There is also a summary list that you can see all your selected categories, so that you can quickly deselect unwanted ones.

### Installation
* Download latest release and copy "DiscountRules.HasCategories" folder to Presentation/Nop.Web/Plugins directory.
* Go to yourdomain.com/Admin/Plugin/List and click "Reload List of Plugins" button
* Scroll down to "Cart must contain certain amount of products in selected categories" plugin and click "Install" button

### Usage
* After creating a discount open "Reqirements" tab
* Select "Cart must contain certain amount of products in selected categories" as discount requirement type
* Set minimum and maximum quantities. The total quantity of products belonging selected categories in the cart should be between min and max values in order to this discount to be applied.
* Select categories. This field accepts a comma-separated list of valid category ids. Please note that you can't set quantity or range values after category ids.
* Save requirement

### Attributions
* Discount tag icon in the logo by Vectors Market from the Noun Project

### License
MIT Copyright (c) 2018 nopWay
