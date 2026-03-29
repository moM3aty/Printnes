// سكربت خاص بلوحة التحكم فقط (Admin Panel)

document.addEventListener("DOMContentLoaded", function () {

    // 1. التحكم في القائمة الجانبية (Sidebar Toggle)
    const sidebarToggleBtn = document.getElementById('toggle-sidebar');
    const sidebar = document.getElementById('erp-sidebar');

    if (sidebarToggleBtn && sidebar) {
        sidebarToggleBtn.addEventListener('click', function () {
            if (window.innerWidth <= 992) {
                // للموبايل والتابلت
                sidebar.classList.toggle('active');
            } else {
                // للشاشات الكبيرة
                sidebar.classList.toggle('collapsed');
            }
        });
    }

    // إغلاق القائمة عند النقر خارجها في الجوال
    document.addEventListener('click', function (event) {
        if (window.innerWidth <= 992 && sidebar && sidebar.classList.contains('active')) {
            if (!sidebar.contains(event.target) && !sidebarToggleBtn.contains(event.target)) {
                sidebar.classList.remove('active');
            }
        }
    });

    // 2. تفعيل التنبيهات (SweetAlert بديل سريع باستخدام Confirm للعمليات الحساسة)
    const deleteButtons = document.querySelectorAll('.btn-delete-confirm');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            if (!confirm("هل أنت متأكد من رغبتك في حذف هذا العنصر؟ لا يمكن التراجع عن هذه العملية.")) {
                e.preventDefault();
            }
        });
    });

    // 3. مسح رسائل الخطأ/النجاح تلقائياً بعد 5 ثوانٍ
    const alertMessages = document.querySelectorAll('.alert-auto-dismiss');
    if (alertMessages.length > 0) {
        setTimeout(() => {
            alertMessages.forEach(msg => {
                msg.style.transition = "opacity 0.5s ease";
                msg.style.opacity = "0";
                setTimeout(() => msg.remove(), 500);
            });
        }, 5000);
    }
});

// 4. دوال الفلترة الديناميكية (Dynamic Dropdowns) - للاستخدام في صفحات التسعير والخيارات
function fetchProductOptions(productId, targetSelectId, optionType) {
    const targetSelect = document.getElementById(targetSelectId);
    if (!targetSelect || !productId) return;

    targetSelect.innerHTML = '<option value="">جاري التحميل...</option>';

    // مثال على كيفية جلب البيانات من الـ API (سيتم تفعيلها في الكنترولر لاحقاً)
    fetch(`/api/StoreApi/GetOptionsByProduct?productId=${productId}&type=${optionType}`)
        .then(response => response.json())
        .then(data => {
            targetSelect.innerHTML = '<option value="">-- اختر --</option>';
            data.forEach(opt => {
                targetSelect.innerHTML += `<option value="${opt.id}">${opt.nameAr}</option>`;
            });
        })
        .catch(error => {
            console.error('Error fetching options:', error);
            targetSelect.innerHTML = '<option value="">حدث خطأ</option>';
        });
}