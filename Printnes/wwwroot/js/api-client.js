/*
 * الملف: wwwroot/js/api-client.js
 * تم تحديث دالة CalculatePrice لتتعامل مع الإضافات وإرسالها للـ API
 */

const ApiClient = {
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

    // دالة حساب السعر المتصلة بـ Backend
    calculatePrice: async function (productId, paperId, sizeId, sidesId, quantity, extraIds = []) {
        try {
            let url = `/api/StoreApi/CalculatePrice?productId=${productId}&paperId=${paperId}&sizeId=${sizeId}&sidesId=${sidesId}&quantity=${quantity}`;

            if (extraIds && extraIds.length > 0) {
                extraIds.forEach(id => url += `&extraIds=${id}`);
            }

            const response = await fetch(url);
            if (!response.ok) throw new Error('Network response error');
            return await response.json();
        } catch (error) {
            console.error('Error calculating price:', error);
            return { success: false };
        }
    },

    uploadDesignFile: async function (file) {
        const formData = new FormData();
        formData.append('file', file);
        try {
            const response = await fetch('/api/StoreApi/UploadDesign', { method: 'POST', body: formData });
            if (!response.ok) throw new Error('Network response error');
            return await response.json();
        } catch (error) {
            console.error('Error uploading file:', error);
            return null;
        }
    }
};