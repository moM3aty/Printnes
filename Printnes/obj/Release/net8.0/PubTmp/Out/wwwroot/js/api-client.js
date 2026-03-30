/*
 * الملف: wwwroot/js/api-client.js
 * يحتوي على جميع استدعاءات الـ API بما فيها المفضلات (Favorites)
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
    },

    // ==========================================
    // دوال المفضلات (Favorites) التي تسببت في الخطأ
    // ==========================================

    toggleFavorite: async function (productId) {
        try {
            const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            const token = tokenElement ? tokenElement.value : '';

            const response = await fetch('/Favorites/Toggle', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: `productId=${productId}`
            });
            return await response.json();
        } catch (error) {
            console.error('Error toggling favorite:', error);
            return { success: false, error: true };
        }
    },

    getFavoritesCount: async function () {
        try {
            const response = await fetch('/Favorites/Count');
            if (!response.ok) return { count: 0 };
            return await response.json();
        } catch (error) {
            return { count: 0 };
        }
    },

    getUserFavoritesIds: async function () {
        try {
            const response = await fetch('/Favorites/GetUserFavorites');
            if (!response.ok) return { productIds: [] };
            return await response.json();
        } catch (error) {
            return { productIds: [] };
        }
    }
};