
using Printnes.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Printnes.Models
{
    public class ProductReview
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    // يمكن أن يكون بدون مستخدم (ضيف) أو مرتبط بمستخدم مسجل
    public string? UserId { get; set; }

    [Required(ErrorMessage = "اسم المراجع مطلوب")]
    [StringLength(200)]
    public string ReviewerName { get; set; }

    [Required(ErrorMessage = "رقم الجوال مطلوب للتواصل")]
    [StringLength(20)]
    public string ReviewerPhone { get; set; }

    // التقييم من 1 إلى 5 نجوم
    [Required(ErrorMessage = "التقييم مطلوب (1-5)")]
    [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
    public byte Rating { get; set; }

    [Required(ErrorMessage = "نص المراجعة مطلوب (10 أحرف على الأقل)")]
    [StringLength(2000, MinimumLength = 10)]
    public string ReviewText { get; set; }

    // صورة مراجعة اختيارية (مثل صورة المنتج من جوال العميل)
    [StringLength(500)]
    public string? ReviewerImageUrl { get; set; }

    // === التحكم والإدارة ===

    // هل التقييم معتمد ونظهر للعملاء؟ (يحتاج موافقة الأدمن أولاً)
    public bool IsApproved { get; set; } = false;

    // هل التقييم مميز ويظهر في الصفحة الرئيسية؟
    public bool IsFeatured { get; set; } = false;

    // رد الأدمن على المراجعة
    [StringLength(500)]
    public string? AdminReply { get; set; }

        public DateTime? AdminReplyDate { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // === Navigation Properties ===

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; }

    // === دوال مساعدة ===

    // نص التقييم المختصر (أول 100 حرف للعرض في الكروت)
    public string ShortReviewText
    {
        get
        {
            if (string.IsNullOrEmpty(ReviewText)) return "";

            if (ReviewText.Length <= 100) return ReviewText;

            return ReviewText.Substring(0, 97) + "...";
        }
    }

    // عدد النجوم كنص (⭐⭐⭐⭐)
    public string StarsHtml
    {
        get
        {
            int full = 5;
            int empty = full - Rating;
            string filled = string.Join("", Enumerable.Range(0, Rating).Select(_ => "<i class=\"fas fa-star\" style=\"color: #f59e0b;\"></i>"));
            string emptyStars = string.Join("", Enumerable.Range(0, empty).Select(_ => "<i class=\"far fa-star\" style=\"color: #e2e8f0;\"></i>"));
            return filled + emptyStars;
        }
    }

    // لون التقييم النسبي المئوية (مثال: 4.5 من 5)
    public double AverageRating => (double)Rating;

    // نسبة التقييم كنسبة (مثال: 90%)
    public double RatingPercentage => Rating * 20;
}
}