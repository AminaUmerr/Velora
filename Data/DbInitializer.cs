using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Velora.Models;

namespace Velora.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await context.Database.MigrateAsync();

            // Seed Roles
            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // Seed Admin User
            const string adminEmail = "admin@velora.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Velora Admin",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@123456");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Clothes", Slug = "clothes", Description = "Premium fashion clothing", DisplayOrder = 1, ImageUrl = "/images/categories/clothes.jpg" },
                    new() { Name = "Shoes", Slug = "shoes", Description = "Footwear for every occasion", DisplayOrder = 2, ImageUrl = "/images/categories/shoes.jpg" },
                    new() { Name = "Watches", Slug = "watches", Description = "Luxury timepieces", DisplayOrder = 3, ImageUrl = "/images/categories/watches.jpg" },
                    new() { Name = "Bags", Slug = "bags", Description = "Designer bags and accessories", DisplayOrder = 4, ImageUrl = "/images/categories/bags.jpg" },
                    new() { Name = "Accessories", Slug = "accessories", Description = "Complete your look", DisplayOrder = 5, ImageUrl = "/images/categories/accessories.jpg" },
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Seed Products
            if (!await context.Products.AnyAsync())
            {
                var clothesCatId = (await context.Categories.FirstAsync(c => c.Slug == "clothes")).Id;
                var shoesCatId = (await context.Categories.FirstAsync(c => c.Slug == "shoes")).Id;
                var watchesCatId = (await context.Categories.FirstAsync(c => c.Slug == "watches")).Id;
                var bagsCatId = (await context.Categories.FirstAsync(c => c.Slug == "bags")).Id;
                var accCatId = (await context.Categories.FirstAsync(c => c.Slug == "accessories")).Id;

                var products = new List<Product>
                {
                    // CLOTHES
                    new() { Name = "Premium Slim-Fit Blazer", Description = "Crafted from Italian wool blend, this slim-fit blazer is perfect for formal and semi-formal occasions. Features a notched lapel and two-button closure.", Price = 12999, DiscountPrice = 9999, Stock = 45, Brand = "Velora Studio", Gender = "Men", Sizes = "S,M,L,XL,XXL", Colors = "Black,Navy,Charcoal", CategoryId = clothesCatId, IsFeatured = true, IsBestSeller = true, ImageUrl = "https://images.unsplash.com/photo-1594938298603-c8148c4b4f6a?w=600" },
                    new() { Name = "Luxury Silk Evening Dress", Description = "A stunning floor-length silk dress with delicate embroidery. Perfect for gala events and formal dinners. Breathable, lightweight fabric.", Price = 18500, DiscountPrice = 14900, Stock = 20, Brand = "Velora Couture", Gender = "Women", Sizes = "XS,S,M,L", Colors = "Ivory,Black,Burgundy", CategoryId = clothesCatId, IsFeatured = true, IsNewArrival = true, ImageUrl = "https://images.unsplash.com/photo-1595777457583-95e059d581b8?w=600" },
                    new() { Name = "Classic White Oxford Shirt", Description = "Essential wardrobe staple. 100% Egyptian cotton, wrinkle-resistant, perfect for office or casual wear.", Price = 3499, DiscountPrice = 2799, Stock = 100, Brand = "Velora Basics", Gender = "Men", Sizes = "S,M,L,XL,XXL", Colors = "White,Light Blue,Pink", CategoryId = clothesCatId, IsBestSeller = true, ImageUrl = "https://images.unsplash.com/photo-1603252109303-2751441dd157?w=600" },
                    new() { Name = "Cashmere Turtleneck Sweater", Description = "Pure cashmere turtleneck sweater. Incredibly soft, warm, and elegant. A luxury essential for colder seasons.", Price = 8999, DiscountPrice = 6999, Stock = 35, Brand = "Velora Premium", Gender = "Women", Sizes = "XS,S,M,L,XL", Colors = "Camel,Cream,Black,Grey", CategoryId = clothesCatId, IsFeatured = true, IsTrending = true, ImageUrl = "https://images.unsplash.com/photo-1576566588028-4147f3842f27?w=600" },
                    new() { Name = "Tailored Chino Trousers", Description = "Slim-fit chinos in stretch cotton. Versatile enough for casual Fridays and weekend outings.", Price = 4999, DiscountPrice = 3799, Stock = 70, Brand = "Velora Basics", Gender = "Men", Sizes = "28,30,32,34,36,38", Colors = "Khaki,Navy,Olive,Black", CategoryId = clothesCatId, IsNewArrival = true, ImageUrl = "https://images.unsplash.com/photo-1624378439575-d8705ad7ae80?w=600" },

                    // SHOES
                    new() { Name = "Italian Leather Oxford Shoes", Description = "Hand-crafted genuine Italian leather Oxford shoes. Goodyear-welted construction for durability and comfort. Perfect for formal occasions.", Price = 22000, DiscountPrice = 17500, Stock = 25, Brand = "Velora Footwear", Gender = "Men", Sizes = "40,41,42,43,44,45", Colors = "Black,Dark Brown,Tan", CategoryId = shoesCatId, IsFeatured = true, IsBestSeller = true, ImageUrl = "https://images.unsplash.com/photo-1614253429340-98120bd6d753?w=600" },
                    new() { Name = "Designer Platform Heels", Description = "Sculptural platform heels in premium leather. 4-inch heel with cushioned insole for all-day comfort without sacrificing style.", Price = 14500, DiscountPrice = 11000, Stock = 18, Brand = "Velora Couture", Gender = "Women", Sizes = "36,37,38,39,40,41", Colors = "Black,Nude,Red", CategoryId = shoesCatId, IsFeatured = true, IsNewArrival = true, ImageUrl = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=600" },
                    new() { Name = "Premium Running Sneakers", Description = "Engineered mesh upper with responsive foam cushioning. Breathable, lightweight, and perfect for high-performance running.", Price = 9999, DiscountPrice = 7999, Stock = 60, Brand = "Velora Sport", Gender = "Unisex", Sizes = "38,39,40,41,42,43,44,45", Colors = "White/Grey,Black/Red,Navy/White", CategoryId = shoesCatId, IsBestSeller = true, IsTrending = true, ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=600" },
                    new() { Name = "Suede Chelsea Boots", Description = "Premium suede Chelsea boots with elastic side panels. Versatile style that transitions from day to evening effortlessly.", Price = 13500, DiscountPrice = null, Stock = 30, Brand = "Velora Footwear", Gender = "Men", Sizes = "40,41,42,43,44,45", Colors = "Tan,Dark Brown,Black", CategoryId = shoesCatId, IsNewArrival = true, ImageUrl = "https://images.unsplash.com/photo-1638247025967-b4e38f787b76?w=600" },

                    // WATCHES
                    new() { Name = "Chronograph Sports Watch", Description = "Swiss-movement chronograph with sapphire crystal glass, 100m water resistance, and stainless steel bracelet. Built for the modern adventurer.", Price = 45000, DiscountPrice = 38000, Stock = 15, Brand = "Velora Timepieces", Gender = "Men", Sizes = "One Size", Colors = "Silver/Black,Gold/Brown,Black/Black", CategoryId = watchesCatId, IsFeatured = true, IsBestSeller = true, ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600" },
                    new() { Name = "Rose Gold Minimalist Watch", Description = "Ultra-thin minimalist dial with genuine leather strap. Japanese quartz movement. The epitome of understated luxury.", Price = 28000, DiscountPrice = 22000, Stock = 20, Brand = "Velora Timepieces", Gender = "Women", Sizes = "One Size", Colors = "Rose Gold/White,Silver/Black", CategoryId = watchesCatId, IsFeatured = true, IsNewArrival = true, ImageUrl = "https://images.unsplash.com/photo-1614164185128-e4ec99c436d7?w=600" },
                    new() { Name = "Luxury Automatic Dress Watch", Description = "Self-winding automatic movement visible through display caseback. 18K gold-plated case with alligator leather strap.", Price = 85000, DiscountPrice = 72000, Stock = 8, Brand = "Velora Prestige", Gender = "Men", Sizes = "One Size", Colors = "Gold/Black,Gold/Brown", CategoryId = watchesCatId, IsTrending = true, ImageUrl = "https://images.unsplash.com/photo-1539874754764-5a96559165b0?w=600" },

                    // BAGS
                    new() { Name = "Structured Leather Tote Bag", Description = "Full-grain leather tote with gold hardware. Spacious interior with organizational pockets. Perfect for the modern professional.", Price = 32000, DiscountPrice = 26000, Stock = 22, Brand = "Velora Leather", Gender = "Women", Sizes = "One Size", Colors = "Black,Tan,Burgundy", CategoryId = bagsCatId, IsFeatured = true, IsBestSeller = true, ImageUrl = "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=600" },
                    new() { Name = "Canvas Weekender Duffel Bag", Description = "Waxed canvas and leather trim duffel bag. Perfect for weekend getaways. Fits in aircraft overhead compartment.", Price = 18500, DiscountPrice = 14800, Stock = 35, Brand = "Velora Travel", Gender = "Unisex", Sizes = "One Size", Colors = "Olive,Navy,Black", CategoryId = bagsCatId, IsNewArrival = true, IsTrending = true, ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=600" },
                    new() { Name = "Mini Crossbody Chain Bag", Description = "Compact quilted leather crossbody with chain strap. Classic design, multiple pocket compartments. An iconic everyday essential.", Price = 22000, DiscountPrice = 17500, Stock = 28, Brand = "Velora Couture", Gender = "Women", Sizes = "One Size", Colors = "Black,Beige,Pink", CategoryId = bagsCatId, IsFeatured = true, IsNewArrival = true, ImageUrl = "https://images.unsplash.com/photo-1566150905458-1bf1fc113f0d?w=600" },
                    new() { Name = "Slim Leather Bifold Wallet", Description = "Premium full-grain leather slim wallet. RFID-blocking technology, 8 card slots, 2 currency compartments.", Price = 5500, DiscountPrice = 4200, Stock = 80, Brand = "Velora Leather", Gender = "Men", Sizes = "One Size", Colors = "Black,Brown,Tan", CategoryId = bagsCatId, IsBestSeller = true, ImageUrl = "https://images.unsplash.com/photo-1627123424574-724758594e93?w=600" },

                    // ACCESSORIES
                    new() { Name = "Cashmere Plaid Scarf", Description = "Pure cashmere plaid scarf. Incredibly soft and warm. Classic timeless pattern that complements any outfit.", Price = 6500, DiscountPrice = 5200, Stock = 55, Brand = "Velora Premium", Gender = "Unisex", Sizes = "One Size", Colors = "Camel/Black,Grey/Black,Navy/Red", CategoryId = accCatId, IsBestSeller = true, ImageUrl = "https://images.unsplash.com/photo-1601924994987-69e26d50dc26?w=600" },
                    new() { Name = "Polarized Aviator Sunglasses", Description = "Italian acetate frame with polarized lenses offering 100% UV protection. Timeless aviator silhouette.", Price = 8999, DiscountPrice = 6999, Stock = 40, Brand = "Velora Eyewear", Gender = "Unisex", Sizes = "One Size", Colors = "Gold/Brown,Silver/Grey,Black/Black", CategoryId = accCatId, IsFeatured = true, IsTrending = true, ImageUrl = "https://images.unsplash.com/photo-1572635196237-14b3f281503f?w=600" },
                    new() { Name = "Italian Leather Belt", Description = "Genuine full-grain Italian leather belt with a polished brass buckle. Elegant and durable, fits sizes 28-44.", Price = 4500, DiscountPrice = 3500, Stock = 65, Brand = "Velora Leather", Gender = "Men", Sizes = "S,M,L,XL", Colors = "Black,Brown,Tan", CategoryId = accCatId, IsBestSeller = true, ImageUrl = "https://images.unsplash.com/photo-1624222247344-550fb60583dc?w=600" },
                    new() { Name = "Silk Pocket Square Set", Description = "Set of 3 pure silk pocket squares in complementary patterns. Adds a touch of refinement to any formal ensemble.", Price = 3200, DiscountPrice = 2400, Stock = 90, Brand = "Velora Studio", Gender = "Men", Sizes = "One Size", Colors = "Assorted", CategoryId = accCatId, IsNewArrival = true, ImageUrl = "https://images.unsplash.com/photo-1607346256330-dee7af15f7c5?w=600" },
                    new() { Name = "Pearl Drop Earrings", Description = "Freshwater pearl drop earrings set in sterling silver. Sophisticated yet understated — perfect for any occasion.", Price = 7500, DiscountPrice = 5900, Stock = 30, Brand = "Velora Jewels", Gender = "Women", Sizes = "One Size", Colors = "White Pearl,Black Pearl", CategoryId = accCatId, IsFeatured = true, IsNewArrival = true, ImageUrl = "https://images.unsplash.com/photo-1535632066927-ab7c9ab60908?w=600" },
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }

            // Seed Banners
            if (!await context.Banners.AnyAsync())
            {
                var banners = new List<Banner>
                {
                    new() { Title = "New Collection Arrived", Subtitle = "Discover the latest luxury fashion pieces", ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=1400", LinkUrl = "/shop", ButtonText = "Shop Now", BannerType = "hero", DisplayOrder = 1 },
                    new() { Title = "Up to 40% Off", Subtitle = "Season End Sale on premium brands", ImageUrl = "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=1400", LinkUrl = "/shop?sale=true", ButtonText = "Grab Deals", BannerType = "hero", DisplayOrder = 2 },
                };
                await context.Banners.AddRangeAsync(banners);
                await context.SaveChangesAsync();
            }
        }
    }
}
