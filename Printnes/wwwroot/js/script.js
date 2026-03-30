/*
 * الملف: wwwroot/js/script.js
 * تم تحديثه لربط زر المفضلة وتحديث القلوب في كافة أنحاء الموقع (Home, Products, Details)
 */

let userFavoritedIds = [];

// 1. تحديث العدادات (السلة والمفضلة)
async function updateBadges() {
    let currentCart = JSON.parse(localStorage.getItem('printnesCart')) || [];
    const cartBadge = document.getElementById('cart-count');
    if (cartBadge) cartBadge.innerText = currentCart.length;

    const favBadge = document.getElementById('fav-count');
    if (favBadge) {
        const result = await ApiClient.getFavoritesCount();
        favBadge.innerText = result.count || 0;
    }
}

// 2. تحميل الـ IDs المفضلة وتحديث شكل القلوب في الصفحة
async function loadFavoritesAndUpdateUI() {
    const res = await ApiClient.getUserFavoritesIds();
    userFavoritedIds = res.productIds || [];

    // تحديث أي أيقونة قلب في الصفحة تم رسمها مسبقاً من السيرفر (SSR)
    document.querySelectorAll('.fav-btn-element').forEach(btn => {
        const prodId = parseInt(btn.getAttribute('data-product-id'));
        if (userFavoritedIds.includes(prodId)) {
            btn.innerHTML = '<i class="fas fa-heart" style="color: #ef4444;"></i>';
        } else {
            btn.innerHTML = '<i class="far fa-heart"></i>';
        }
    });
}

// 3. استدعاء التحديث فور تحميل أي صفحة
document.addEventListener("DOMContentLoaded", async function () {
    await loadFavoritesAndUpdateUI();
    updateBadges();
});

// 4. دالة جلب وعرض المنتجات من الـ API (تُستخدم في Home و Products)
async function renderSection(categoryKey, targetId) {
    const grid = document.getElementById(targetId);
    if (!grid) return;

    grid.innerHTML = '<div style="width:100%; text-align:center; padding:40px; grid-column: 1 / -1;"><i class="fas fa-circle-notch fa-spin fa-2x" style="color:var(--orange);"></i></div>';

    let items = [];
    if (categoryKey === "featured") {
        items = await ApiClient.getFeaturedProducts();
    } else {
        items = await ApiClient.getProductsByCategory(categoryKey);
    }

    if (items.length === 0) {
        grid.innerHTML = '<div style="width:100%; text-align:center; color:#999; grid-column: 1 / -1;">لا توجد منتجات في هذا القسم حالياً.</div>';
        return;
    }

    grid.innerHTML = items.map(p => {
        const isFav = userFavoritedIds.includes(p.id);
        const heartIcon = isFav ? '<i class="fas fa-heart" style="color: #ef4444;"></i>' : '<i class="far fa-heart"></i>';

        return `
        <div class="product-card">
            <div class="p-badge">${p.badge || 'منتج'}</div>
            
            <!-- زر المفضلات الديناميكي -->
            <div class="wish-icon fav-btn-element" data-product-id="${p.id}" onclick="handleFavoriteToggle(${p.id}, this, '${p.name.replace(/'/g, "\\'")}', '${p.img}')" style="cursor: pointer; z-index: 5;">
                ${heartIcon}
            </div>

            <div onclick="goToDetails(${p.id})" style="cursor:pointer">
                <img src="${p.img}" alt="${p.name}">
                <h3>${p.name}</h3>
            </div>
            <p style="color:#f26a2e; font-weight:900;">السعر حسب الاختيارات</p>
            <button class="add-to-cart" onclick="goToDetails(${p.id})">
                عرض التفاصيل <i class="fas fa-arrow-left"></i>
            </button>
        </div>
        `;
    }).join('');
}

// 5. معالجة ضغطة زر المفضلات من أي مكان في الموقع
async function handleFavoriteToggle(productId, btnElement, name, img) {
    // إيقاف التوجيه لصفحة المنتج إذا تم الضغط على القلب
    if (event) event.stopPropagation();

    const result = await ApiClient.toggleFavorite(productId);

    if (result.isLoggedIn === false) {
        // توجيه لصفحة الدخول لأن العميل غير مسجل
        window.location.href = `/Account/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
        return;
    }

    if (result.success) {
        // تحديث أيقونة القلب في جميع الأماكن التي يظهر فيها هذا المنتج في نفس الصفحة
        const allBtnsForThisProduct = document.querySelectorAll(`.fav-btn-element[data-product-id="${productId}"]`);

        allBtnsForThisProduct.forEach(btn => {
            if (result.isFavorite) {
                btn.innerHTML = '<i class="fas fa-heart" style="color: #ef4444; animation: pulse 0.3s;"></i>';
            } else {
                btn.innerHTML = '<i class="far fa-heart"></i>';
            }
        });

        // إظهار الإشعار بناءً على الحالة
        if (result.isFavorite) {
            showToastMessage('تمت الإضافة للمفضلة', name, img, false);
            // إضافة للـ Array المحلي
            if (!userFavoritedIds.includes(productId)) userFavoritedIds.push(productId);
        } else {
            showToastMessage('تم الإزالة من المفضلة', name, img, false);
            // إزالة من الـ Array المحلي
            userFavoritedIds = userFavoritedIds.filter(id => id !== productId);
        }

        // تحديث العداد
        const favBadge = document.getElementById('fav-count');
        if (favBadge) favBadge.innerText = result.total;
    }
}

// 6. إضافة المنتج للسلة من صفحة التفاصيل
function addToCartDirectly(id, name, price, img) {
    let cart = JSON.parse(localStorage.getItem('printnesCart')) || [];
    const itemToAdd = { id: id, name: name, price: price, img: img, quantity: 1 };
    cart.push(itemToAdd);
    localStorage.setItem('printnesCart', JSON.stringify(cart));

    updateBadges();
    showToastMessage('تمت الإضافة إلى سلة التسوق', name, img, true);
}

// 7. إظهار الإشعارات (للسلة أو المفضلات)
function showToastMessage(title, productName, productImg, isCart = true) {
    const toast = document.getElementById('cart-toast');
    if (!toast) return;

    document.getElementById('toast-status-icon').innerHTML = `<i class="fas fa-check-circle"></i> ${title}`;
    document.getElementById('toast-product-img').src = productImg;
    document.getElementById('toast-product-name').innerText = productName;

    const priceContainer = document.getElementById('toast-price-container');
    if (priceContainer) priceContainer.style.display = isCart ? 'inline' : 'none';

    const footer = document.getElementById('toast-footer');
    if (footer) footer.style.display = isCart ? 'flex' : 'none';

    toast.classList.remove('hidden');
    setTimeout(() => { hideToast(); }, 3500);
}

function hideToast() {
    const toast = document.getElementById('cart-toast');
    if (toast) toast.classList.add('hidden');
}

// 8. عرض محتويات صفحة السلة (Cart Page)
function renderCartPage() {
    let cart = JSON.parse(localStorage.getItem('printnesCart')) || [];
    const emptyView = document.getElementById('empty-cart-view');
    const fullView = document.getElementById('full-cart-view');
    const tableBody = document.getElementById('cart-table-body');
    const totalDisplay = document.getElementById('grand-total');

    if (!emptyView || !fullView) return;

    if (cart.length === 0) {
        emptyView.classList.remove('hidden');
        fullView.classList.add('hidden');
    } else {
        emptyView.classList.add('hidden');
        fullView.classList.remove('hidden');

        tableBody.innerHTML = cart.map((item, index) => `
            <tr>
                <td>
                    <strong>${item.name}</strong><br>
                    <small style="color:#888;">الكمية: ${item.quantity || 1}</small>
                </td>
                <td><img src="${item.img}" style="width:50px; height:50px; object-fit:cover; border-radius:5px;"></td>
                <td style="color:var(--orange); font-weight:bold;">${(parseFloat(item.price) * (item.quantity || 1)).toFixed(2)} ر.س</td>
                <td>
                    <button class="btn-remove" type="button" onclick="removeItem(${index})">
                        <i class="fas fa-trash-alt"></i>
                    </button>
                </td>
            </tr>
        `).join('');

        let total = cart.reduce((sum, item) => sum + (parseFloat(item.price) * (item.quantity || 1)), 0);
        if (totalDisplay) totalDisplay.innerText = total.toFixed(2);
    }
    updateBadges();
}

function removeItem(index) {
    let cart = JSON.parse(localStorage.getItem('printnesCart')) || [];
    cart.splice(index, 1);
    localStorage.setItem('printnesCart', JSON.stringify(cart));
    renderCartPage();
}

function checkoutFullCart() {
    window.location.href = "/Order/Checkout";
}

function goToDetails(id) {
    window.location.href = `/Products/Details/${id}`;
}

// وظائف القائمة الجانبية
function openSideMenu() {
    const menu = document.getElementById('side-menu');
    const overlay = document.getElementById('menu-overlay');
    if (menu && overlay) {
        menu.classList.add('active');
        overlay.classList.remove('hidden');
        setTimeout(() => overlay.classList.add('active'), 10);
        document.body.style.overflow = 'hidden';
    }
}

function closeSideMenu() {
    const menu = document.getElementById('side-menu');
    const overlay = document.getElementById('menu-overlay');
    if (menu) menu.classList.remove('active');
    if (overlay) {
        overlay.classList.remove('active');
        setTimeout(() => overlay.classList.add('hidden'), 300);
    }
    document.body.style.overflow = 'auto';
}

function toggleSubMenu(element) {
    const parentLi = element.parentElement;
    document.querySelectorAll('.has-submenu').forEach(item => {
        if (item !== parentLi) item.classList.remove('open');
    });
    parentLi.classList.toggle('open');
}

function toggleFaq(element) {
    element.classList.toggle('active');
}