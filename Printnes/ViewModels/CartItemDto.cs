using System.Text.Json.Serialization;

namespace Printnes.ViewModels
{
    // هذا الكلاس وظيفته استقبال وفك تشفير الـ JSON الخاص بالسلة 
    // القادم من الـ LocalStorage في الـ Front-end (مهم لعملية إتمام الطلب)
    public class CartItemDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("img")]
        public string Img { get; set; }

        [JsonPropertyName("details")]
        public CartItemDetailsDto Details { get; set; }
    }

    public class CartItemDetailsDto
    {
        [JsonPropertyName("design")]
        public string Design { get; set; }

        [JsonPropertyName("sides")]
        public string Sides { get; set; }

        [JsonPropertyName("cover")]
        public string Cover { get; set; }

        [JsonPropertyName("paperType")]
        public string PaperType { get; set; }

        [JsonPropertyName("corners")]
        public string Corners { get; set; }
    }
}