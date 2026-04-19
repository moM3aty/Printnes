using Printnes.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Printnes.Helpers
{
    public static class StudioSettingsManager
    {
        private static readonly string filePath = "studio_settings.json";

        public static StudioSettingsViewModel LoadSettings()
        {
            if (!File.Exists(filePath))
            {
                var defaultSettings = new StudioSettingsViewModel
                {
                    BasePrintCost = 0.50m,
                    Materials = new List<StudioMaterial>
                    {
                        new StudioMaterial { Name = "ورق مقوى أبيض", InternalValue = "white", ColorHex = "#ffffff", IsGlossy = false, CostPerCm2 = 0.015m },
                        new StudioMaterial { Name = "ورق كرافت بني", InternalValue = "kraft", ColorHex = "#c29b70", IsGlossy = false, CostPerCm2 = 0.01m },
                        new StudioMaterial { Name = "أسود فاخر لامع", InternalValue = "supergloss", ColorHex = "#1a1a1a", IsGlossy = true, CostPerCm2 = 0.02m }
                    },
                    Sizes = new List<StudioSize>
                    {
                        new StudioSize { Name = "مقاس قياسي 1", Length = 15, Width = 10, Depth = 5 },
                        new StudioSize { Name = "مقاس قياسي 2", Length = 20, Width = 15, Depth = 8 }
                    },
                    DiscountTiers = new List<StudioDiscountTier>
                    {
                        new StudioDiscountTier { MinQuantity = 500, DiscountPercentage = 0.15m },
                        new StudioDiscountTier { MinQuantity = 1000, DiscountPercentage = 0.25m }
                    }
                };
                SaveSettings(defaultSettings);
                return defaultSettings;
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<StudioSettingsViewModel>(json);
        }

        public static void SaveSettings(StudioSettingsViewModel settings)
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }
}