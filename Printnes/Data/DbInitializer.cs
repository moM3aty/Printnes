/*
 * الملف: Data/DbInitializer.cs
 * ملف التهيئة العملاق (Massive Seeder) - إصدار الداتا الضخمة
 * يقوم بزرع آلاف السجلات لتجهيز المطبعة بشكل احترافي ومتكامل.
 */

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Printnes.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

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

            // تطبيق التحديثات
            await context.Database.MigrateAsync();

            // ==========================================
            // 1. تهيئة الصلاحيات
            // ==========================================
            string[] roles = { "SuperAdmin", "Admin", "Accountant", "Designer", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ==========================================
            // 2. إنشاء المستخدمين (المدير + عملاء وهميين للتقييمات)
            // ==========================================
            string adminEmail = "admin@printnes.co";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "مدير النظام", PhoneNumber = "0500000001", EmailConfirmed = true, IsActive = true };
                var res = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (res.Succeeded) await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }

            // قائمة عملاء وهميين
            var fakeCustomers = new List<ApplicationUser>();
            string[] customerNames = { "أحمد الدوسري", "سارة خالد", "مؤسسة الأفق", "شركة الرؤية", "محمد العتيبي" };
            for (int i = 0; i < customerNames.Length; i++)
            {
                string email = $"customer{i + 1}@gmail.com";
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser { UserName = email, Email = email, FullName = customerNames[i], PhoneNumber = $"055555555{i}", EmailConfirmed = true, IsActive = true };
                    var res = await userManager.CreateAsync(user, "User@123456");
                    if (res.Succeeded) await userManager.AddToRoleAsync(user, "Customer");
                }
                fakeCustomers.Add(user);
            }

            // ==========================================
            // 3. الأقسام الرئيسية والفرعية (شاملة وكبيرة جداً)
            // ==========================================
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    // أقسام رئيسية
                    new Category { NameAr = "المطبوعات الورقية", NameEn = "Paper Prints", Slug = "paperprints", IconClass = "fa-print", SortOrder = 1, IsActive = true },
                    new Category { NameAr = "التغليف والبوكسات", NameEn = "Packaging", Slug = "packaging", IconClass = "fa-box-open", SortOrder = 2, IsActive = true },
                    new Category { NameAr = "الملصقات والاستيكرات", NameEn = "Stickers", Slug = "stickers", IconClass = "fa-sticky-note", SortOrder = 3, IsActive = true },
                    new Category { NameAr = "الهدايا الدعائية", NameEn = "Promotional Gifts", Slug = "promo", IconClass = "fa-gift", SortOrder = 4, IsActive = true },
                    new Category { NameAr = "اللوحات والستاندات", NameEn = "Large Format", Slug = "large-format", IconClass = "fa-image", SortOrder = 5, IsActive = true },
                    new Category { NameAr = "الأختام والدروع", NameEn = "Stamps & Shields", Slug = "stamps-shields", IconClass = "fa-award", SortOrder = 6, IsActive = true },
                    new Category { NameAr = "مستلزمات المعارض", NameEn = "Exhibitions", Slug = "exhibitions", IconClass = "fa-store", SortOrder = 7, IsActive = true },
                    new Category { NameAr = "تصميم الجرافيك", NameEn = "Design Services", Slug = "design-services", IconClass = "fa-palette", SortOrder = 8, IsActive = true },

                    // فرعية - المطبوعات الورقية
                    new Category { NameAr = "كروت شخصية", NameEn = "Business Cards", Slug = "business-cards", ParentMenu = "paperprints", SortOrder = 10, IsActive = true },
                    new Category { NameAr = "فلايرات ومطويات", NameEn = "Flyers & Brochures", Slug = "flyers", ParentMenu = "paperprints", SortOrder = 11, IsActive = true },
                    new Category { NameAr = "فولدرات (ملفات)", NameEn = "Folders", Slug = "folders", ParentMenu = "paperprints", SortOrder = 12, IsActive = true },
                    new Category { NameAr = "أوراق رسمية", NameEn = "Letterheads", Slug = "letterheads", ParentMenu = "paperprints", SortOrder = 13, IsActive = true },
                    new Category { NameAr = "أظرف مراسلات", NameEn = "Envelopes", Slug = "envelopes", ParentMenu = "paperprints", SortOrder = 14, IsActive = true },
                    new Category { NameAr = "دفاتر ومفكرات", NameEn = "Notebooks", Slug = "notebooks", ParentMenu = "paperprints", SortOrder = 15, IsActive = true },
                    new Category { NameAr = "سندات وفواتير", NameEn = "Invoices", Slug = "invoices", ParentMenu = "paperprints", SortOrder = 16, IsActive = true },

                    // فرعية - البوكسات والتغليف
                    new Category { NameAr = "بوكسات شحن", NameEn = "Shipping Boxes", Slug = "shipping-boxes", ParentMenu = "packaging", SortOrder = 20, IsActive = true },
                    new Category { NameAr = "بوكسات هدايا", NameEn = "Gift Boxes", Slug = "gift-boxes", ParentMenu = "packaging", SortOrder = 21, IsActive = true },
                    new Category { NameAr = "بوكسات طعام", NameEn = "Food Boxes", Slug = "food-boxes", ParentMenu = "packaging", SortOrder = 22, IsActive = true },
                    new Category { NameAr = "أكياس ورقية", NameEn = "Paper Bags", Slug = "paper-bags", ParentMenu = "packaging", SortOrder = 23, IsActive = true },
                    new Category { NameAr = "أكياس قماشية", NameEn = "Canvas Bags", Slug = "canvas-bags", ParentMenu = "packaging", SortOrder = 24, IsActive = true },
                    new Category { NameAr = "ورق تغليف", NameEn = "Wrapping Paper", Slug = "wrapping-paper", ParentMenu = "packaging", SortOrder = 25, IsActive = true },
                    new Category { NameAr = "شريط لاصق مطبوع", NameEn = "Printed Tape", Slug = "printed-tape", ParentMenu = "packaging", SortOrder = 26, IsActive = true },
                    new Category { NameAr = "أكواب ورقية", NameEn = "Paper Cups", Slug = "paper-cups", ParentMenu = "packaging", SortOrder = 27, IsActive = true },

                    // فرعية - الملصقات
                    new Category { NameAr = "استيكر مقصوص", NameEn = "Die-Cut Stickers", Slug = "stickers-cut", ParentMenu = "stickers", SortOrder = 30, IsActive = true },
                    new Category { NameAr = "استيكر رول", NameEn = "Roll Stickers", Slug = "stickers-roll", ParentMenu = "stickers", SortOrder = 31, IsActive = true },
                    new Category { NameAr = "استيكر شفاف", NameEn = "Clear Stickers", Slug = "stickers-clear", ParentMenu = "stickers", SortOrder = 32, IsActive = true },

                    // فرعية - الهدايا
                    new Category { NameAr = "أكواب ومطارات", NameEn = "Mugs & Flasks", Slug = "mugs", ParentMenu = "promo", SortOrder = 40, IsActive = true },
                    new Category { NameAr = "تيشرتات ويونيفورم", NameEn = "T-Shirts", Slug = "tshirts", ParentMenu = "promo", SortOrder = 41, IsActive = true },
                    new Category { NameAr = "أقلام دعائية", NameEn = "Pens", Slug = "pens", ParentMenu = "promo", SortOrder = 42, IsActive = true },
                    new Category { NameAr = "فلاش ميموري (USB)", NameEn = "USBs", Slug = "usbs", ParentMenu = "promo", SortOrder = 43, IsActive = true },
                    new Category { NameAr = "تعليقات بطاقات", NameEn = "Lanyards", Slug = "lanyards", ParentMenu = "promo", SortOrder = 44, IsActive = true },

                    // فرعية - لوحات وأختام
                    new Category { NameAr = "رول أب وبوب أب", NameEn = "Rollups & Popups", Slug = "rollups", ParentMenu = "large-format", SortOrder = 50, IsActive = true },
                    new Category { NameAr = "لوحات كانفاس", NameEn = "Canvas Prints", Slug = "canvas", ParentMenu = "large-format", SortOrder = 51, IsActive = true },
                    new Category { NameAr = "أختام حبرية", NameEn = "Rubber Stamps", Slug = "rubber-stamps", ParentMenu = "stamps-shields", SortOrder = 60, IsActive = true },
                    new Category { NameAr = "أختام شمعية", NameEn = "Wax Seals", Slug = "wax-seals", ParentMenu = "stamps-shields", SortOrder = 61, IsActive = true },
                    new Category { NameAr = "دروع كريستال", NameEn = "Crystal Shields", Slug = "crystal-shields", ParentMenu = "stamps-shields", SortOrder = 62, IsActive = true }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 4. البانرات (Banners)
            // ==========================================
            if (!context.Banners.Any())
            {
                var banners = new List<Banner>
                {
                    new Banner { Title = "عروض التغليف الفاخر", ImageUrl = "https://cdn.salla.sa/form-builder/PD53rr1LUNb19Hj3je9ULKSfhYUGg0o3TOBdz1T7.png", LinkUrl = "/Category/packaging", SortOrder = 1, IsActive = true },
                    new Banner { Title = "المطبوعات الورقية", ImageUrl = "https://cdn.salla.sa/form-builder/wzsdRwVsUbNJVFVyQS6oTkganoSdKqnn1KADKYWk.png", LinkUrl = "/Category/paperprints", SortOrder = 2, IsActive = true },
                    new Banner { Title = "استيكرات المصانع", ImageUrl = "https://cdn.salla.sa/rDvVQ/1pKR8dtK4Hd9UoYFf1hqwqPQJMn5YFEqyOyJngiZ.jpg", LinkUrl = "/Category/stickers", SortOrder = 3, IsActive = true },
                    new Banner { Title = "الهدايا الدعائية للشركات", ImageUrl = "https://cdn.salla.sa/rDvVQ/cOd3EfWfrDP0n64aE7wrEEXzWprxjvx6kvvsO0aR.png", LinkUrl = "/Category/promo", SortOrder = 4, IsActive = true },
                    new Banner { Title = "مستلزمات المعارض والمؤتمرات", ImageUrl = "https://cdn.salla.sa/rDvVQ/OQxmT3F9y8bS0SrRQOIZVusIvSyRHalGpw0j7rJX.png", LinkUrl = "/Category/exhibitions", SortOrder = 5, IsActive = true }
                };
                context.Banners.AddRange(banners);
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 5. المنتجات الضخمة (Massive Products)
            // ==========================================
            if (!context.Products.Any())
            {
                var cats = await context.Categories.ToListAsync();
                int GetId(string slug) => cats.FirstOrDefault(c => c.Slug == slug)?.Id ?? 1;

                string img1 = "https://cdn.salla.sa/rDvVQ/OQxmT3F9y8bS0SrRQOIZVusIvSyRHalGpw0j7rJX.png";
                string img2 = "https://cdn.salla.sa/form-builder/PD53rr1LUNb19Hj3je9ULKSfhYUGg0o3TOBdz1T7.png";
                string img3 = "https://cdn.salla.sa/rDvVQ/1pKR8dtK4Hd9UoYFf1hqwqPQJMn5YFEqyOyJngiZ.jpg";
                string img4 = "https://cdn.salla.sa/form-builder/wzsdRwVsUbNJVFVyQS6oTkganoSdKqnn1KADKYWk.png";
                string img5 = "https://cdn.salla.sa/rDvVQ/cOd3EfWfrDP0n64aE7wrEEXzWprxjvx6kvvsO0aR.png";

                var products = new List<Product>
                {
                    // كروت ومطبوعات ورقية
                    new Product { CategoryId = GetId("business-cards"), NameAr = "كروت شخصية كوشيه", NameEn = "Coated Business Cards", Slug = "bc-coated", DescriptionAr = "كروت شخصية مطبوعة بجودة عالية على ورق كوشيه سميك.", CoverImageUrl = img1, MinQuantity = 100, MaxQuantity = 10000, IsActive = true },
                    new Product { CategoryId = GetId("business-cards"), NameAr = "كروت شخصية ورق مجزع فاخر", NameEn = "Textured Business Cards", Slug = "bc-textured", DescriptionAr = "كروت شخصية على ورق فاخر ذو ملمس مجزع ليعطي طابعاً رسمياً فخماً.", CoverImageUrl = img2, MinQuantity = 100, MaxQuantity = 5000, IsActive = true },
                    new Product { CategoryId = GetId("business-cards"), NameAr = "كروت بلمسة الفويل البارز", NameEn = "Foil Business Cards", Slug = "bc-foil", DescriptionAr = "طباعة ذهبية أو فضية بارزة لتعكس فخامة علامتك.", CoverImageUrl = img3, MinQuantity = 500, MaxQuantity = 5000, IsActive = true },
                    new Product { CategoryId = GetId("business-cards"), NameAr = "كروت بلاستيكية شفافة PVC", NameEn = "Transparent PVC Cards", Slug = "bc-pvc", DescriptionAr = "كروت بلاستيكية شفافة مقاومة للماء والقطع.", CoverImageUrl = img4, MinQuantity = 100, MaxQuantity = 5000, IsActive = true },
                    new Product { CategoryId = GetId("flyers"), NameAr = "فلايرات A4 تسويقية", NameEn = "A4 Flyers", Slug = "flyer-a4", DescriptionAr = "اطبع رسالتك الإعلانية بألوان زاهية وورق لامع.", CoverImageUrl = img5, MinQuantity = 1000, MaxQuantity = 100000, IsActive = true },
                    new Product { CategoryId = GetId("flyers"), NameAr = "مطوية (بروشور) 3 طيات", NameEn = "Tri-fold Brochure", Slug = "brochure-trifold", DescriptionAr = "بروشورات مطوية احترافية لشرح خدماتك أو قائمة طعامك.", CoverImageUrl = img1, MinQuantity = 500, MaxQuantity = 50000, IsActive = true },
                    new Product { CategoryId = GetId("flyers"), NameAr = "بروفايل شركة متكامل", NameEn = "Company Profile", Slug = "company-profile", DescriptionAr = "كتيب تعريفي للشركات (بروفايل) من 8 إلى 24 صفحة.", CoverImageUrl = img2, MinQuantity = 50, MaxQuantity = 5000, IsActive = true },
                    new Product { CategoryId = GetId("folders"), NameAr = "فولدر ملفات كرتوني", NameEn = "Corporate Folders", Slug = "corporate-folders", DescriptionAr = "فولدرات فاخرة مخصصة لحفظ المستندات مع جيب للكارت.", CoverImageUrl = img3, MinQuantity = 250, MaxQuantity = 10000, IsActive = true },
                    new Product { CategoryId = GetId("letterheads"), NameAr = "أوراق خطابات رسمية", NameEn = "Official Letterheads", Slug = "letterheads", DescriptionAr = "أوراق خطابات رسمية مطبوعة على ورق كونسيرت.", CoverImageUrl = img4, MinQuantity = 1000, MaxQuantity = 20000, IsActive = true },
                    new Product { CategoryId = GetId("envelopes"), NameAr = "أظرف مراسلات بمقاسات", NameEn = "Printed Envelopes", Slug = "envelopes", DescriptionAr = "أظرف مراسلات مطبوعة بشعارك (مقاس DL أو A4).", CoverImageUrl = img5, MinQuantity = 500, MaxQuantity = 20000, IsActive = true },
                    new Product { CategoryId = GetId("notebooks"), NameAr = "مفكرات (نوت بوك) سلك", NameEn = "Wire Notebooks", Slug = "wire-notebooks", DescriptionAr = "مفكرات مطبوعة الغلاف والورق الداخلي لتدوين الملاحظات.", CoverImageUrl = img1, MinQuantity = 50, MaxQuantity = 5000, IsActive = true },
                    new Product { CategoryId = GetId("invoices"), NameAr = "سندات قبض وصرف مكربنة", NameEn = "Carbonless Invoices", Slug = "carbonless-invoices", DescriptionAr = "دفاتر فواتير وسندات مكربنة (أصل ونسختين).", CoverImageUrl = img2, MinQuantity = 10, MaxQuantity = 1000, IsActive = true },

                    // بوكسات وتغليف
                    new Product { CategoryId = GetId("shipping-boxes"), NameAr = "بوكس شحن كرتون مضلع", NameEn = "Corrugated Shipping Box", Slug = "box-shipping", DescriptionAr = "بوكسات كرتون مضلع لحماية منتجاتك أثناء الشحن.", CoverImageUrl = img3, MinQuantity = 100, MaxQuantity = 10000, AllowCustomSize = true, IsActive = true },
                    new Product { CategoryId = GetId("luxury-boxes"), NameAr = "بوكس هدايا هارد كفر", NameEn = "Hard Cover Gift Box", Slug = "box-luxury-hard", DescriptionAr = "بوكسات صلبة فاخرة للمجوهرات والعطور.", CoverImageUrl = img4, MinQuantity = 50, MaxQuantity = 2000, AllowCustomSize = true, IsActive = true },
                    new Product { CategoryId = GetId("luxury-boxes"), NameAr = "بوكس مغناطيس فاخر", NameEn = "Magnetic Gift Box", Slug = "box-luxury-mag", DescriptionAr = "بوكسات هدايا بقفلة مغناطيسية لتجربة Unboxing مميزة.", CoverImageUrl = img5, MinQuantity = 50, MaxQuantity = 2000, IsActive = true },
                    new Product { CategoryId = GetId("food-boxes"), NameAr = "بوكسات برجر ووجبات", NameEn = "Burger & Food Boxes", Slug = "box-food", DescriptionAr = "علب ورقية مبطنة بطبقة عازلة للدهون.", CoverImageUrl = img1, MinQuantity = 1000, MaxQuantity = 50000, IsActive = true },
                    new Product { CategoryId = GetId("food-boxes"), NameAr = "بوكسات بيتزا مخصصة", NameEn = "Pizza Boxes", Slug = "box-pizza", DescriptionAr = "بوكسات بيتزا مضلعة تحافظ على الحرارة.", CoverImageUrl = img2, MinQuantity = 500, MaxQuantity = 50000, IsActive = true },
                    new Product { CategoryId = GetId("paper-bags"), NameAr = "أكياس ورقية كوشيه لامع", NameEn = "Glossy Paper Bags", Slug = "bag-paper-glossy", DescriptionAr = "أكياس تسوق ورقية سميكة ومقاومة للقطع.", CoverImageUrl = img3, MinQuantity = 200, MaxQuantity = 10000, IsActive = true },
                    new Product { CategoryId = GetId("paper-bags"), NameAr = "أكياس كرافت بنية بمسكة", NameEn = "Kraft Paper Bags", Slug = "bag-kraft", DescriptionAr = "أكياس كرافت بنية صديقة للبيئة.", CoverImageUrl = img4, MinQuantity = 500, MaxQuantity = 20000, IsActive = true },
                    new Product { CategoryId = GetId("canvas-bags"), NameAr = "أكياس قماشية (Tote Bags)", NameEn = "Canvas Tote Bags", Slug = "bag-tote", DescriptionAr = "أكياس قماشية يعاد استخدامها، ممتازة للمعارض.", CoverImageUrl = img5, MinQuantity = 50, MaxQuantity = 5000, IsActive = true },
                    new Product { CategoryId = GetId("wrapping-paper"), NameAr = "ورق تغليف منتجات (تغريسات)", NameEn = "Tissue Wrapping Paper", Slug = "wrapping-tissue", DescriptionAr = "ورق خفيف لحماية المنتجات الداخلية وإضافة لمسة أناقة.", CoverImageUrl = img1, MinQuantity = 1000, MaxQuantity = 50000, IsActive = true },
                    new Product { CategoryId = GetId("printed-tape"), NameAr = "شريط لاصق مطبوع بشعارك", NameEn = "Printed Packing Tape", Slug = "tape-printed", DescriptionAr = "شريط لاصق (شطرطون) قوي جداً مطبوع بشعارك.", CoverImageUrl = img2, MinQuantity = 50, MaxQuantity = 1000, IsActive = true },
                    new Product { CategoryId = GetId("paper-cups"), NameAr = "أكواب ورقية للمقاهي", NameEn = "Paper Coffee Cups", Slug = "cups-paper", DescriptionAr = "أكواب ورقية دبل ولس (Double Wall) للمشروبات الساخنة.", CoverImageUrl = img3, MinQuantity = 1000, MaxQuantity = 50000, IsActive = true },

                    // ملصقات
                    new Product { CategoryId = GetId("stickers-cut"), NameAr = "استيكر لاصق مقصوص دائري", NameEn = "Die-Cut Circle Stickers", Slug = "sticker-diecut-circle", DescriptionAr = "ملصقات مقصوصة بدقة عالية.", CoverImageUrl = img4, MinQuantity = 500, MaxQuantity = 50000, AllowCustomSize = true, IsActive = true },
                    new Product { CategoryId = GetId("stickers-roll"), NameAr = "استيكر رول للمصانع والعلب", NameEn = "Roll Stickers", Slug = "sticker-roll", DescriptionAr = "استيكرات تأتي على شكل بكرة لتسهيل عملية اللصق.", CoverImageUrl = img5, MinQuantity = 1000, MaxQuantity = 100000, IsActive = true },
                    new Product { CategoryId = GetId("stickers-clear"), NameAr = "استيكر شفاف (Clear)", NameEn = "Clear Stickers", Slug = "sticker-clear", DescriptionAr = "ملصقات شفافة بالكامل لتعطي مظهر الطباعة المباشرة على الزجاج.", CoverImageUrl = img1, MinQuantity = 500, MaxQuantity = 50000, IsActive = true },

                    // هدايا دعائية
                    new Product { CategoryId = GetId("mugs"), NameAr = "كوب سيراميك أبيض مطبوع", NameEn = "Printed White Mug", Slug = "mug-white", DescriptionAr = "أكواب سيراميك مع طباعة حرارية كاملة.", CoverImageUrl = img2, MinQuantity = 24, MaxQuantity = 2000, IsActive = true },
                    new Product { CategoryId = GetId("mugs"), NameAr = "كوب سحري (يتغير بالحرارة)", NameEn = "Magic Mug", Slug = "mug-magic", DescriptionAr = "يظهر التصميم عند سكب المشروب الساخن.", CoverImageUrl = img3, MinQuantity = 24, MaxQuantity = 1000, IsActive = true },
                    new Product { CategoryId = GetId("tshirts"), NameAr = "تيشرت قطني بولو", NameEn = "Polo T-Shirt", Slug = "tshirt-polo", DescriptionAr = "تيشرت بولو فاخر مناسب ليونيفورم الشركات.", CoverImageUrl = img4, MinQuantity = 10, MaxQuantity = 1000, IsActive = true },
                    new Product { CategoryId = GetId("tshirts"), NameAr = "تيشرت ياقة دائرية مطبوع", NameEn = "Round Neck T-Shirt", Slug = "tshirt-round", DescriptionAr = "تيشرتات قطنية مريحة مع طباعة DTF عالية الجودة.", CoverImageUrl = img5, MinQuantity = 10, MaxQuantity = 2000, IsActive = true },
                    new Product { CategoryId = GetId("pens"), NameAr = "أقلام بلاستيكية دعائية", NameEn = "Promotional Pens", Slug = "pens-plastic", DescriptionAr = "أقلام رخيصة وممتازة للتوزيعات بكميات كبيرة.", CoverImageUrl = img1, MinQuantity = 100, MaxQuantity = 10000, IsActive = true },
                    new Product { CategoryId = GetId("pens"), NameAr = "أقلام معدنية فاخرة", NameEn = "Luxury Metal Pens", Slug = "pens-metal", DescriptionAr = "أقلام معدنية محفورة بالليزر لهدايا الـ VIP.", CoverImageUrl = img2, MinQuantity = 20, MaxQuantity = 1000, IsActive = true },
                    new Product { CategoryId = GetId("usbs"), NameAr = "فلاش ميموري كرت محفظة", NameEn = "Card USB Flash Drive", Slug = "usb-card", DescriptionAr = "فلاش ميموري على شكل كرت فيزا يطبع على الوجهين.", CoverImageUrl = img3, MinQuantity = 50, MaxQuantity = 2000, IsActive = true },
                    new Product { CategoryId = GetId("lanyards"), NameAr = "شريط بطاقات (لانيارد)", NameEn = "Printed Lanyards", Slug = "lanyards-printed", DescriptionAr = "أشرطة تعليق للبطاقات مطبوعة بشعار الجهة.", CoverImageUrl = img4, MinQuantity = 50, MaxQuantity = 5000, IsActive = true },

                    // لوحات وأختام
                    new Product { CategoryId = GetId("rollups"), NameAr = "لوحة رول أب ألمنيوم", NameEn = "Rollup Stand", Slug = "rollup-stand", DescriptionAr = "ستاند رول أب 85x200 سم مع طباعة بنر عالي الدقة.", CoverImageUrl = img5, MinQuantity = 1, MaxQuantity = 100, IsActive = true },
                    new Product { CategoryId = GetId("rollups"), NameAr = "لوحة بوب أب جدارية", NameEn = "Popup Display", Slug = "popup-display", DescriptionAr = "ستاند بوب أب كبير كخلفية للتصوير والمعارض.", CoverImageUrl = img1, MinQuantity = 1, MaxQuantity = 20, IsActive = true },
                    new Product { CategoryId = GetId("canvas"), NameAr = "طباعة كانفاس مع إطار خشبي", NameEn = "Canvas Print with Frame", Slug = "canvas-print", DescriptionAr = "اطبع صورك الفنية على قماش الكانفاس المشدود.", CoverImageUrl = img2, MinQuantity = 1, MaxQuantity = 100, AllowCustomSize = true, IsActive = true },
                    new Product { CategoryId = GetId("rubber-stamps"), NameAr = "ختم حبري أتوماتيك", NameEn = "Automatic Rubber Stamp", Slug = "stamp-auto", DescriptionAr = "أختام حبرية جاهزة للاستخدام للشركات والأفراد.", CoverImageUrl = img3, MinQuantity = 1, MaxQuantity = 50, IsActive = true },
                    new Product { CategoryId = GetId("wax-seals"), NameAr = "ختم شمعي معدني", NameEn = "Wax Seal Stamp", Slug = "stamp-wax", DescriptionAr = "ختم معدني محفور بدقة مع أصابع الشمع لتغليف كلاسيكي.", CoverImageUrl = img4, MinQuantity = 1, MaxQuantity = 50, IsActive = true },
                    new Product { CategoryId = GetId("crystal-shields"), NameAr = "درع تكريم كريستال", NameEn = "Crystal Award Shield", Slug = "shield-crystal", DescriptionAr = "دروع كريستالية فاخرة مطبوعة أو محفورة بالليزر للمناسبات.", CoverImageUrl = img5, MinQuantity = 1, MaxQuantity = 100, IsActive = true },
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 6. المولد الذكي الشامل (Smart Matrix & Options Generator)
            // ==========================================
            // هنا السحر الحقيقي، سيقوم بالمرور على جميع المنتجات وبناء خيارات وأسعار تناسب نوع القسم
            var allProds = await context.Products.Include(p => p.Category).ToListAsync();
            var newOptions = new List<ProductOption>();
            var newPricing = new List<PricingMatrix>();

            foreach (var prod in allProds)
            {
                // إذا كان المنتج له خيارات مسبقاً نتخطاه
                if (await context.ProductOptions.AnyAsync(o => o.ProductId == prod.Id)) continue;

                string parentMenu = prod.Category?.ParentMenu ?? "";

                // تحديد خيارات الطباعة بناءً على نوع القسم الأب
                if (parentMenu == "paperprints")
                {
                    newOptions.AddRange(new[] {
                        new ProductOption { ProductId = prod.Id, OptionType = 1, NameAr = "ورق 170 جرام", NameEn = "170g Paper", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 1, NameAr = "ورق كوشيه 300 جرام", NameEn = "300g Coated", SortOrder = 2, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 1, NameAr = "ورق فاخر مجزع", NameEn = "Textured VIP", SortOrder = 3, IsActive = true },

                        new ProductOption { ProductId = prod.Id, OptionType = 2, NameAr = "مقاس قياسي", NameEn = "Standard Size", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 2, NameAr = "مقاس كبير", NameEn = "Large Size", SortOrder = 2, IsActive = true },

                        new ProductOption { ProductId = prod.Id, OptionType = 3, NameAr = "وجه واحد", NameEn = "1 Side", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 3, NameAr = "وجهين", NameEn = "2 Sides", SortOrder = 2, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 3, NameAr = "بدون طباعه", NameEn = "No Print", SortOrder = 3, IsActive = true },

                        new ProductOption { ProductId = prod.Id, OptionType = 4, NameAr = "بدون حماية", NameEn = "None", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 4, NameAr = "سلوفان مطفي", NameEn = "Matte Lam", SortOrder = 2, IsActive = true }
                    });
                }
                else if (parentMenu == "packaging")
                {
                    newOptions.AddRange(new[] {
                        new ProductOption { ProductId = prod.Id, OptionType = 1, NameAr = "كرتون مضلع 3 طبقات", NameEn = "3-Layer Corrugated", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 1, NameAr = "كرتون مقوى فاخر", NameEn = "Hard Board", SortOrder = 2, IsActive = true },

                        new ProductOption { ProductId = prod.Id, OptionType = 2, NameAr = "صغير (S)", NameEn = "Small", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 2, NameAr = "متوسط (M)", NameEn = "Medium", SortOrder = 2, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 2, NameAr = "كبير (L)", NameEn = "Large", SortOrder = 3, IsActive = true },

                        new ProductOption { ProductId = prod.Id, OptionType = 3, NameAr = "طباعة لون واحد (خارجي)", NameEn = "1 Color (Out)", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 3, NameAr = "طباعة ملونة كاملة", NameEn = "Full Color", SortOrder = 2, IsActive = true }
                    });
                }
                else if (parentMenu == "promo")
                {
                    newOptions.AddRange(new[] {
                        new ProductOption { ProductId = prod.Id, OptionType = 1, NameAr = "أبيض قياسي", NameEn = "Standard White", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 1, NameAr = "ملون فاخر", NameEn = "Colored VIP", SortOrder = 2, IsActive = true },

                        new ProductOption { ProductId = prod.Id, OptionType = 2, NameAr = "متوسط (M)", NameEn = "Size M", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 2, NameAr = "كبير (L)", NameEn = "Size L", SortOrder = 2, IsActive = true },

                        new ProductOption { ProductId = prod.Id, OptionType = 3, NameAr = "طباعة جهة واحدة", NameEn = "Front Print", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 3, NameAr = "طباعة جهتين", NameEn = "Front & Back", SortOrder = 2, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 3, NameAr = "بدون طباعه", NameEn = "No Print", SortOrder = 3, IsActive = true },

                    });
                }
                else
                {
                    // خيارات عامة لباقي الأقسام
                    newOptions.AddRange(new[] {
                        new ProductOption { ProductId = prod.Id, OptionType = 1, NameAr = "خامة أساسية", NameEn = "Basic Material", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 1, NameAr = "خامة فاخرة", NameEn = "Premium Material", SortOrder = 2, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 2, NameAr = "مقاس افتراضي", NameEn = "Default Size", SortOrder = 1, IsActive = true },
                        new ProductOption { ProductId = prod.Id, OptionType = 3, NameAr = "تنفيذ قياسي", NameEn = "Standard Execution", SortOrder = 1, IsActive = true }
                    });
                }

                // الإضافات (Extras) لكل منتج
                context.ProductExtras.Add(new ProductExtra { ProductId = prod.Id, NameAr = "تعديل الملف وضبط الألوان", NameEn = "Color Correction", Price = 30m, SortOrder = 1, IsActive = true });
                context.ProductExtras.Add(new ProductExtra { ProductId = prod.Id, NameAr = "طلب تصميم جديد", NameEn = "New Design Service", Price = 150m, SortOrder = 2, IsActive = true });

                // شرائح الخصم (Quantity Tiers)
                context.QuantityTiers.Add(new QuantityTier { ProductId = prod.Id, MinQuantity = prod.MinQuantity * 3, MaxQuantity = prod.MinQuantity * 9, DiscountPercent = 10m });
                context.QuantityTiers.Add(new QuantityTier { ProductId = prod.Id, MinQuantity = prod.MinQuantity * 10, MaxQuantity = prod.MaxQuantity, DiscountPercent = 25m });
            }

            // حفظ الخيارات أولاً لكي نستطيع توليد التقاطعات
            await context.ProductOptions.AddRangeAsync(newOptions);
            await context.SaveChangesAsync();

            // توليد مصفوفة الأسعار (Pricing Matrix) الضخمة!
            foreach (var prod in allProds)
            {
                var opt1 = await context.ProductOptions.Where(o => o.ProductId == prod.Id && o.OptionType == 1).ToListAsync();
                var opt2 = await context.ProductOptions.Where(o => o.ProductId == prod.Id && o.OptionType == 2).ToListAsync();
                var opt3 = await context.ProductOptions.Where(o => o.ProductId == prod.Id && o.OptionType == 3).ToListAsync();

                // لو لم يكن هناك خيارات كافية، نتخطى لمنع الأخطاء
                if (!opt1.Any() || !opt2.Any() || !opt3.Any()) continue;

                var existingMatrix = await context.PricingMatrices.Where(m => m.ProductId == prod.Id).ToListAsync();

                foreach (var o1 in opt1)
                {
                    foreach (var o2 in opt2)
                    {
                        foreach (var o3 in opt3)
                        {
                            if (!existingMatrix.Any(m => m.PaperOptionId == o1.Id && m.SizeOptionId == o2.Id && m.SidesOptionId == o3.Id))
                            {
                                // خوارزمية ذكية لحساب السعر بناءً على الترتيب والكمية الدنيا
                                decimal basePrice = 25m + (o1.SortOrder * 10) + (o2.SortOrder * 5);
                                decimal unitPrice = 0.5m + (o1.SortOrder * 0.2m) + (o3.SortOrder * 0.3m);

                                // المنتجات القليلة العدد (مثل اللوحات والأكواب) سعر الحبة يكون أعلى بكثير
                                if (prod.MinQuantity < 50) unitPrice *= 15m;

                                newPricing.Add(new PricingMatrix
                                {
                                    ProductId = prod.Id,
                                    PaperOptionId = o1.Id,
                                    SizeOptionId = o2.Id,
                                    SidesOptionId = o3.Id,
                                    BasePrice = basePrice,
                                    UnitPrice = unitPrice
                                });
                            }
                        }
                    }
                }
            }
            await context.PricingMatrices.AddRangeAsync(newPricing);
            await context.SaveChangesAsync();

            // ==========================================
            // 7. التقييمات الوهمية (Reviews) بالمئات
            // ==========================================
            if (!context.ProductReviews.Any())
            {
                var reviews = new List<ProductReview>();
                string[] positiveTexts = { "الطباعة ممتازة جداً والتغليف رائع، شكراً برنتس!", "أفضل مطبعة تعاملت معها من حيث سرعة الإنجاز وجودة الألوان.", "المنتج فخم جداً ويفوق الوصف، أنصح بالتعامل معهم بشدة.", "شكراً لخدمة العملاء على التجاوب السريع، النتيجة النهائية ممتازة.", "الخامة المستخدمة قوية والقص دقيق جداً." };
                string[] mixedTexts = { "الجودة جيدة لكن شركة الشحن تأخرت يومين.", "التصميم طُبع بشكل ممتاز لكن تمنيت لو كان الورق أسمك قليلاً." };

                var random = new Random();

                foreach (var prod in allProds)
                {
                    // نضيف من 2 إلى 4 تقييمات لكل منتج
                    int revCount = random.Next(2, 5);
                    for (int i = 0; i < revCount; i++)
                    {
                        var customer = fakeCustomers[random.Next(fakeCustomers.Count)];
                        int rating = random.Next(1, 100) > 20 ? 5 : 4; // 80% خمس نجوم
                        string reviewText = rating == 5 ? positiveTexts[random.Next(positiveTexts.Length)] : mixedTexts[random.Next(mixedTexts.Length)];

                        reviews.Add(new ProductReview
                        {
                            ProductId = prod.Id,
                            UserId = customer.Id,
                            ReviewerName = customer.FullName,
                            ReviewerPhone = customer.PhoneNumber,
                            Rating = (byte)rating,
                            ReviewText = reviewText,
                            IsApproved = true,
                            IsFeatured = rating == 5 && i == 0, // أول تقييم 5 نجوم نجعله مميز
                            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 60)) // تواريخ عشوائية قديمة
                        });
                    }
                }
                await context.ProductReviews.AddRangeAsync(reviews);
                await context.SaveChangesAsync();
            }
        }
    }
}