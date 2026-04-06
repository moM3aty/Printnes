/*
 * الملف: Helpers/SiteSettingsManager.cs
 * مدير الإعدادات: يقوم بقراءة وحفظ الإعدادات في ملف JSON لسرعة فائقة
 */

using Printnes.ViewModels;
using System.IO;
using System.Text.Json;

namespace Printnes.Helpers
{
    public static class SiteSettingsManager
    {
        private static readonly string filePath = "site_settings.json";

        public static StoreSettingsViewModel LoadSettings()
        {
            if (!File.Exists(filePath))
            {
                // الإعدادات الافتراضية المطابقة تماماً لـ StoreSettingsViewModel الخاص بك
                return new StoreSettingsViewModel
                {
                    StoreName = "مطابع برنتس (Printnes)",
                    TaxPercentage = 15.0m,
                    DefaultShippingCost = 25.0m,
                    ContactEmail = "support@printnes.co",
                    ContactPhone = "0554804857",
                    WhatsappNumber = "966554804857",
                    StoreDescription = "PRINTNES مؤسسة سعودية معرّفه في وزارة التجارة متخصصين في تصنيع المطبوعات وتقديم خدمات التصميم الفريدة والمتميزة",
                    Currency = "ر.س",
                    FacebookUrl = "",
                    InstagramUrl = "",
                    TiktokUrl = "",
                    TwitterUrl = "",
                    EnableReviews = true,
                    EnableUserRegistration = true,
                    EnableGuestCheckout = true,
                    MinimumOrderAmount = 0,
                    FreeShippingAbove = 500
                };
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<StoreSettingsViewModel>(json);
        }

        public static void SaveSettings(StoreSettingsViewModel settings)
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }
}