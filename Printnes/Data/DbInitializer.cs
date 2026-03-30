/*
 * الملف: Data/DbInitializer.cs
 * كلاس تهيئة قاعدة البيانات (Seeder)
 * تم بناء قاعدة بيانات عملاقة ومكتملة للمتجر تغطي كافة الجداول: 
 * أقسام، منتجات، خيارات طباعة، إضافات، شرائح خصم، مصفوفة أسعار، تقييمات للعملاء، وبانرات.
 */

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Printnes.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Printnes.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // تطبيق التحديثات على الداتابيز
            await context.Database.MigrateAsync();

            // 1. تهيئة الصلاحيات
            string[] roles = { "SuperAdmin", "Admin", "Accountant", "Designer", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. إنشاء المستخدمين الافتراضيين
            string adminEmail = "admin@printnes.co";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "مدير النظام الأساسي",
                    PhoneNumber = "0500000001",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var res = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (res.Succeeded) await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }

            string customerEmail = "customer@gmail.com";
            var fakeCustomer = await userManager.FindByEmailAsync(customerEmail);
            if (fakeCustomer == null)
            {
                fakeCustomer = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FullName = "عميل تجريبي",
                    PhoneNumber = "0555555555",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var res = await userManager.CreateAsync(fakeCustomer, "User@123456");
                if (res.Succeeded) await userManager.AddToRoleAsync(fakeCustomer, "Customer");
            }

            // 3. تهيئة الأقسام الرئيسية والفرعية
            if (!context.Categories.Any())
            {
                var categories = new[]
                {
                    new Category { NameAr = "مطبوعات ورقية", NameEn = "Paper Prints", Slug = "paperprints", IconClass = "fa-print", SortOrder = 1 },
                    new Category { NameAr = "بوكسات وتغليف", NameEn = "Boxes & Packaging", Slug = "boxes", IconClass = "fa-box-open", SortOrder = 2 },
                    new Category { NameAr = "ملصقات واستيكرات", NameEn = "Stickers", Slug = "stickers", IconClass = "fa-sticky-note", SortOrder = 3 },

                    new Category { NameAr = "كروت شخصية", NameEn = "Business Cards", Slug = "business-cards", ParentMenu = "paperprints", SortOrder = 10 },
                    new Category { NameAr = "فلايرات تسويقية", NameEn = "Flyers", Slug = "flyers", ParentMenu = "paperprints", SortOrder = 11 },
                    new Category { NameAr = "بوكسات شحن", NameEn = "Shipping Boxes", Slug = "shipping-boxes", ParentMenu = "boxes", SortOrder = 20 },
                    new Category { NameAr = "أكياس ورقية", NameEn = "Paper Bags", Slug = "paper-bags", ParentMenu = "boxes", SortOrder = 21 },
                    new Category { NameAr = "استيكر مقصوص", NameEn = "Die-Cut Stickers", Slug = "stickers-cut", ParentMenu = "stickers", SortOrder = 30 },
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // 4. تهيئة المنتجات
            if (!context.Products.Any())
            {
                int bCardCatId = context.Categories.FirstOrDefault(c => c.Slug == "business-cards")?.Id ?? 1;
                int boxCatId = context.Categories.FirstOrDefault(c => c.Slug == "shipping-boxes")?.Id ?? 1;
                int flyerCatId = context.Categories.FirstOrDefault(c => c.Slug == "flyers")?.Id ?? 1;
                int bagCatId = context.Categories.FirstOrDefault(c => c.Slug == "paper-bags")?.Id ?? 1;
                int stickerCatId = context.Categories.FirstOrDefault(c => c.Slug == "stickers-cut")?.Id ?? 1;

                var products = new[]
                {
                    new Product { CategoryId = bCardCatId, NameAr = "كروت شخصية فاخرة", NameEn = "Premium Business Cards", Slug = "premium-business-cards", DescriptionAr = "كروت شخصية بلمسات جمالية وخيارات تغليف متعددة تعكس هوية علامتك التجارية.", CoverImageUrl = "https://cdn.salla.sa/rDvVQ/OQxmT3F9y8bS0SrRQOIZVusIvSyRHalGpw0j7rJX.png", MinQuantity = 100, MaxQuantity = 50000, AllowCustomSize = false },
                    new Product { CategoryId = bCardCatId, NameAr = "كروت شفافة بلاستيك", NameEn = "Transparent Cards", Slug = "transparent-cards", DescriptionAr = "كروت بلاستيكية شفافة فخمة جداً ومقاومة للماء.", CoverImageUrl = "https://cdn.salla.sa/form-builder/PD53rr1LUNb19Hj3je9ULKSfhYUGg0o3TOBdz1T7.png", MinQuantity = 50, MaxQuantity = 10000, AllowCustomSize = false },
                    new Product { CategoryId = boxCatId, NameAr = "بوكس شحن مضلع", NameEn = "Corrugated Shipping Box", Slug = "shipping-box", DescriptionAr = "بوكس قوي يتحمل الشحن مع إمكانية طباعة الشعار.", CoverImageUrl = "https://cdn.salla.sa/rDvVQ/1pKR8dtK4Hd9UoYFf1hqwqPQJMn5YFEqyOyJngiZ.jpg", MinQuantity = 50, MaxQuantity = 5000, AllowCustomSize = true },
                    new Product { CategoryId = flyerCatId, NameAr = "فلاير إعلاني A5", NameEn = "A5 Flyer", Slug = "a5-flyer", DescriptionAr = "مطبوعات تسويقية سريعة بأسعار تنافسية للتوزيع.", CoverImageUrl = "https://cdn.salla.sa/form-builder/wzsdRwVsUbNJVFVyQS6oTkganoSdKqnn1KADKYWk.png", MinQuantity = 1000, MaxQuantity = 100000, AllowCustomSize = false },
                    new Product { CategoryId = bagCatId, NameAr = "كيس ورقي مطبوع", NameEn = "Printed Paper Bag", Slug = "paper-bag", DescriptionAr = "أكياس ورقية أنيقة للمتاجر مع حبال قطنية متينة.", CoverImageUrl = "https://cdn.salla.sa/form-builder/PD53rr1LUNb19Hj3je9ULKSfhYUGg0o3TOBdz1T7.png", MinQuantity = 100, MaxQuantity = 10000, AllowCustomSize = false },
                    new Product { CategoryId = stickerCatId, NameAr = "استيكر دائري مقصوص", NameEn = "Circle Die-Cut Sticker", Slug = "circle-sticker", DescriptionAr = "استيكرات لاصقة قوية تستخدم لختم العلب والمنتجات.", CoverImageUrl = "https://cdn.salla.sa/rDvVQ/OQxmT3F9y8bS0SrRQOIZVusIvSyRHalGpw0j7rJX.png", MinQuantity = 500, MaxQuantity = 50000, AllowCustomSize = true }
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }

            // 5. تهيئة البانرات الإعلانية
            if (!context.Banners.Any())
            {
                var banners = new[]
                {
                    new Banner { Title = "عروض المطبوعات الورقية", ImageUrl = "https://cdn.salla.sa/form-builder/wzsdRwVsUbNJVFVyQS6oTkganoSdKqnn1KADKYWk.png", LinkUrl = "/Category/paperprints", SortOrder = 1 },
                    new Banner { Title = "بوكسات الشحن المخصصة", ImageUrl = "https://cdn.salla.sa/form-builder/PD53rr1LUNb19Hj3je9ULKSfhYUGg0o3TOBdz1T7.png", LinkUrl = "/Category/boxes", SortOrder = 2 }
                };
                context.Banners.AddRange(banners);
                await context.SaveChangesAsync();
            }

            // 6. تهيئة كافة تفاصيل منتج "الكروت الشخصية" (خيارات، أسعار، شرائح، مراجعات)
            var targetProduct = await context.Products.FirstOrDefaultAsync(p => p.Slug == "premium-business-cards");
            if (targetProduct != null)
            {
                // أ. خيارات الطباعة
                if (!context.ProductOptions.Any(o => o.ProductId == targetProduct.Id))
                {
                    var options = new[]
                    {
                        new ProductOption { ProductId = targetProduct.Id, OptionType = 1, NameAr = "كوشيه 300 جرام", NameEn = "Coated 300g" },
                        new ProductOption { ProductId = targetProduct.Id, OptionType = 1, NameAr = "كوشيه 350 جرام", NameEn = "Coated 350g" },
                        new ProductOption { ProductId = targetProduct.Id, OptionType = 2, NameAr = "9x5.5 سم", NameEn = "9x5.5 cm", ExtraData = "{\"w\":9,\"h\":5.5}" },
                        new ProductOption { ProductId = targetProduct.Id, OptionType = 3, NameAr = "وجه واحد", NameEn = "1 Side" },
                        new ProductOption { ProductId = targetProduct.Id, OptionType = 3, NameAr = "وجهين", NameEn = "2 Sides" },
                        new ProductOption { ProductId = targetProduct.Id, OptionType = 4, NameAr = "سلوفان مطفي", NameEn = "Matte Lam" },
                        new ProductOption { ProductId = targetProduct.Id, OptionType = 4, NameAr = "بدون سلوفان", NameEn = "No Lam" }
                    };
                    context.ProductOptions.AddRange(options);
                    await context.SaveChangesAsync();
                }

                // ب. الإضافات الاختيارية
                if (!context.ProductExtras.Any(e => e.ProductId == targetProduct.Id))
                {
                    var extras = new[]
                    {
                        new ProductExtra { ProductId = targetProduct.Id, NameAr = "طلب تصميم للكرت", NameEn = "Design Request", Price = 100.0m },
                        new ProductExtra { ProductId = targetProduct.Id, NameAr = "قص زوايا دائرية", NameEn = "Rounded Corners", Price = 25.0m }
                    };
                    context.ProductExtras.AddRange(extras);
                    await context.SaveChangesAsync();
                }

                // ج. شرائح الخصم
                if (!context.QuantityTiers.Any(q => q.ProductId == targetProduct.Id))
                {
                    var tiers = new[]
                    {
                        new QuantityTier { ProductId = targetProduct.Id, MinQuantity = 500, MaxQuantity = 999, DiscountPercent = 5.0m },
                        new QuantityTier { ProductId = targetProduct.Id, MinQuantity = 1000, MaxQuantity = 4999, DiscountPercent = 12.5m },
                        new QuantityTier { ProductId = targetProduct.Id, MinQuantity = 5000, MaxQuantity = 100000, DiscountPercent = 25.0m }
                    };
                    context.QuantityTiers.AddRange(tiers);
                    await context.SaveChangesAsync();
                }

                // د. مصفوفة الأسعار للتقاطعات
                if (!context.PricingMatrices.Any(m => m.ProductId == targetProduct.Id))
                {
                    var paper300 = await context.ProductOptions.FirstOrDefaultAsync(o => o.ProductId == targetProduct.Id && o.OptionType == 1 && o.NameAr.Contains("300"));
                    var paper350 = await context.ProductOptions.FirstOrDefaultAsync(o => o.ProductId == targetProduct.Id && o.OptionType == 1 && o.NameAr.Contains("350"));
                    var size = await context.ProductOptions.FirstOrDefaultAsync(o => o.ProductId == targetProduct.Id && o.OptionType == 2);
                    var side1 = await context.ProductOptions.FirstOrDefaultAsync(o => o.ProductId == targetProduct.Id && o.OptionType == 3 && o.NameAr.Contains("واحد"));
                    var side2 = await context.ProductOptions.FirstOrDefaultAsync(o => o.ProductId == targetProduct.Id && o.OptionType == 3 && o.NameAr.Contains("وجهين"));

                    if (paper300 != null && size != null && side1 != null && side2 != null && paper350 != null)
                    {
                        var matrices = new[]
                        {
                            new PricingMatrix { ProductId = targetProduct.Id, PaperOptionId = paper300.Id, SizeOptionId = size.Id, SidesOptionId = side1.Id, BasePrice = 20m, UnitPrice = 0.15m },
                            new PricingMatrix { ProductId = targetProduct.Id, PaperOptionId = paper300.Id, SizeOptionId = size.Id, SidesOptionId = side2.Id, BasePrice = 30m, UnitPrice = 0.20m },
                            new PricingMatrix { ProductId = targetProduct.Id, PaperOptionId = paper350.Id, SizeOptionId = size.Id, SidesOptionId = side1.Id, BasePrice = 25m, UnitPrice = 0.18m },
                            new PricingMatrix { ProductId = targetProduct.Id, PaperOptionId = paper350.Id, SizeOptionId = size.Id, SidesOptionId = side2.Id, BasePrice = 35m, UnitPrice = 0.25m }
                        };
                        context.PricingMatrices.AddRange(matrices);
                        await context.SaveChangesAsync();
                    }
                }

                // هـ. التقييمات الوهمية لإثراء واجهة الإدارة والعميل
                if (!context.ProductReviews.Any(r => r.ProductId == targetProduct.Id))
                {
                    var reviews = new[]
                    {
                        new ProductReview { ProductId = targetProduct.Id, UserId = fakeCustomer?.Id, ReviewerName = "أحمد محمد", ReviewerPhone = "0501234567", Rating = 5, ReviewText = "جودة الطباعة ممتازة جداً والتغليف راقي، شكراً برنتس!", IsApproved = true, IsFeatured = true, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                        new ProductReview { ProductId = targetProduct.Id, UserId = fakeCustomer?.Id, ReviewerName = "مؤسسة الأفق", ReviewerPhone = "0598765432", Rating = 4, ReviewText = "تعامل احترافي وسرعة في التوصيل، لكن أتمنى توفير خيارات ورق أكثر.", IsApproved = true, AdminReply = "شكراً لتقييمك، نعمل حالياً على توفير خيارات ورق فاخرة قريباً.", AdminReplyDate = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow.AddDays(-2) },
                        new ProductReview { ProductId = targetProduct.Id, ReviewerName = "زائر", ReviewerPhone = "0511111111", Rating = 5, ReviewText = "أفضل مطبعة تعاملت معها.", IsApproved = false, CreatedAt = DateTime.UtcNow }
                    };
                    context.ProductReviews.AddRange(reviews);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}