
const ApiClient = {
    // جلب المنتجات حسب القسم
    getProductsByCategory: async function (categorySlug) {
        try {
            const response = await fetch(`/api/StoreApi/GetProductsByCategory?slug=${categorySlug}`);
            if (!response.ok) throw new Error('Network response error');
            return await response.json();
        } catch (error) {
            console.error('Error fetching products:', error);
            return [];
        }
    },

    // جلب المنتجات المميزة للصفحة الرئيسية
    getFeaturedProducts: async function () {
        try {
            const response = await fetch('/api/StoreApi/GetFeaturedProducts');
            if (!response.ok) throw new Error('Network response error');
            return await response.json();
        } catch (error) {
            console.error('Error fetching featured products:', error);
            return [];
        }
    },

    // --- الدوال الجديدة لاستكمال دورة النظام ---

    // حساب السعر الديناميكي بناءً على مصفوفة الأسعار والخيارات
    calculatePrice: async function (productId, paperId, sizeId, sidesId, quantity) {
        try {
            const response = await fetch(`/api/StoreApi/CalculatePrice?productId=${productId}&paperId=${paperId}&sizeId=${sizeId}&sidesId=${sidesId}&quantity=${quantity}`);
            if (!response.ok) throw new Error('Network response error');
            return await response.json();
        } catch (error) {
            console.error('Error calculating price:', error);
            return null;
        }
    },

    // رفع ملفات التصميم الخاصة بالعميل
    uploadDesignFile: async function (file) {
        const formData = new FormData();
        formData.append('file', file);

        try {
            const response = await fetch('/api/StoreApi/UploadDesign', {
                method: 'POST',
                body: formData
            });
            if (!response.ok) throw new Error('Network response error');
            return await response.json(); // يعود بـ FileId لربطه بالطلب
        } catch (error) {
            console.error('Error uploading file:', error);
            return null;
        }
    },

    // إرسال الطلب النهائي (Checkout)
    submitOrder: async function (orderData) {
        try {
            const response = await fetch('/api/StoreApi/SubmitOrder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(orderData)
            });
            if (!response.ok) throw new Error('Network response error');
            return await response.json();
        } catch (error) {
            console.error('Error submitting order:', error);
            return { success: false, message: 'حدث خطأ أثناء إرسال الطلب' };
        }
    }
};