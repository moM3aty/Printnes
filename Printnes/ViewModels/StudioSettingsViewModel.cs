using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Printnes.ViewModels
{
    public class StudioSettingsViewModel
    {
        [Required]
        public decimal BasePrintCost { get; set; } = 0.50m;

        // أنواع العلب المتاحة في الاستوديو
        public List<StudioBoxType> BoxTypes { get; set; } = new List<StudioBoxType>();

        public List<StudioMaterial> Materials { get; set; } = new List<StudioMaterial>();
        public List<StudioSize> Sizes { get; set; } = new List<StudioSize>();
        public List<StudioDiscountTier> DiscountTiers { get; set; } = new List<StudioDiscountTier>();
    }

    public class StudioBoxType
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string InternalValue { get; set; } // mailer, folding, rigid, shipping
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class StudioMaterial
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "اسم الخامة مطلوب")]
        public string Name { get; set; }

        [Required(ErrorMessage = "كود اللون مطلوب")]
        public string ColorHex { get; set; } = "#ffffff";

        public bool IsGlossy { get; set; } = false;

        [Required(ErrorMessage = "القيمة البرمجية مطلوبة")]
        public string InternalValue { get; set; } = "white";

        [Required(ErrorMessage = "سعر السنتيمتر المربع مطلوب")]
        public decimal CostPerCm2 { get; set; } = 0.015m;
    }

    public class StudioSize
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "اسم المقاس مطلوب")]
        public string Name { get; set; }

        [Required]
        public double Length { get; set; }

        [Required]
        public double Width { get; set; }

        [Required]
        public double Depth { get; set; }

        public string DimensionsString => $"{Length}x{Width}x{Depth}";
    }

    public class StudioDiscountTier
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public int MinQuantity { get; set; }

        [Required]
        public decimal DiscountPercentage { get; set; }
    }
}