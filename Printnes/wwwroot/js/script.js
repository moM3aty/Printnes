/*
 * الملف: wwwroot/js/script.js
 * النسخة المحدثة لحل مشكلة عداد السلة وتحديثه في الهيدر والتزامن مع LocalStorage
 */

// 1. تحديث عداد السلة من الـ LocalStorage مباشرة لضمان الدقة
function updateCartBadge() {
    let currentCart = JSON.parse(localStorage.getItem('printnesCart')) || [];
    const badge = document.getElementById('cart-count');
    if (badge) {
        badge.innerText = currentCart.length;
    }
}

// 2. استدعاء التحديث فور تحميل أي صفحة
document.addEventListener("DOMContentLoaded", function () {
    updateCartBadge();
});

// 3. دالة جلب وعرض المنتجات من الـ API
async function renderSection(categoryKey, targetId) {
    const grid = document.getElementById(targetId);
    if (!grid) return;

    // عرض علامة تحميل (Loader) لحين وصول البيانات
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

    grid.innerHTML = items.map(p => `
        <div class="product-card">
            <div class="p-badge">${p.badge || 'منتج مميز'}</div>
            <div onclick="goToDetails(${p.id})" style="cursor:pointer">
                <img src="${p.img}" alt="${p.name}">
                <h3>${p.name}</h3>
            </div>
            <p style="color:#f26a2e; font-weight:900;">${parseFloat(p.price).toFixed(2)} ر.س</p>
            <button class="add-to-cart" onclick="addToCartDirectly(${p.id}, '${p.name}', ${p.price}, '${p.img}')">
                إضافة للسلة <i class="fas fa-cart-plus"></i>
            </button>
        </div>
    `).join('');
}

// 4. إضافة المنتج للسلة من الصفحة الرئيسية أو الأقسام
function addToCartDirectly(id, name, price, img) {
    // جلب السلة الحالية
    let cart = JSON.parse(localStorage.getItem('printnesCart')) || [];

    const itemToAdd = {
        id: id,
        name: name,
        price: price,
        img: img,
        quantity: 1
    };

    // إضافة المنتج وحفظه
    cart.push(itemToAdd);
    localStorage.setItem('printnesCart', JSON.stringify(cart));

    // تحديث العداد في الهيدر وإظهار الإشعار
    updateCartBadge();
    showCartToast(itemToAdd);
}

// 5. إظهار وإخفاء إشعار السلة الجانبي
function showCartToast(p) {
    const toast = document.getElementById('cart-toast');
    if (!toast) return;
    document.getElementById('toast-product-img').src = p.img;
    document.getElementById('toast-product-name').innerText = p.name;
    document.getElementById('toast-product-price').innerText = parseFloat(p.price).toFixed(2);

    const footer = document.getElementById('toast-footer');
    if (footer) footer.style.display = 'flex';

    toast.classList.remove('hidden');
    setTimeout(() => { hideToast(); }, 4000);
}

function hideToast() {
    const toast = document.getElementById('cart-toast');
    if (toast) toast.classList.add('hidden');
}

// 6. عرض محتويات صفحة السلة (Cart Page)
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

    // لضمان تزامن العداد دائماً
    updateCartBadge();
}

// 7. حذف عنصر من السلة
function removeItem(index) {
    let cart = JSON.parse(localStorage.getItem('printnesCart')) || [];
    cart.splice(index, 1);
    localStorage.setItem('printnesCart', JSON.stringify(cart));

    // إعادة رسم الصفحة وتحديث العداد
    renderCartPage();
    updateCartBadge();
}

// 8. التوجيه لصفحة إتمام الطلب
function checkoutFullCart() {
    window.location.href = "/Order/Checkout";
}

// 9. التوجيه لصفحة التفاصيل
function goToDetails(id) {
    window.location.href = `/Products/Details/${id}`;
}

// ==========================================
// وظائف واجهة المستخدم (UI Functions)
// ==========================================

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