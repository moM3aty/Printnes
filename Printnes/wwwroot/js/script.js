/**
 * script.js - النسخة الديناميكية المتصلة بقاعدة البيانات
 * تحافظ على نفس الـ Style والـ Logic الخاص بك ولكن مع داتا حقيقية
 */

let cart = JSON.parse(localStorage.getItem('printnesCart')) || [];
let activeProduct = { name: "", price: 0, total: 0 };

function updateCartBadge() {
    const badge = document.getElementById('cart-count');
    if (badge) badge.innerText = cart.length;
}

// دالة العرض الديناميكية (تستدعي البيانات من الـ API بدلاً من storeData)
async function renderSection(categoryKey, targetId) {
    const grid = document.getElementById(targetId);
    if (!grid) return;

    // عرض علامة تحميل (Loader) لحين وصول البيانات
    grid.innerHTML = '<div style="width:100%; text-align:center; padding:40px; grid-column: 1 / -1;"><i class="fas fa-circle-notch fa-spin fa-2x" style="color:var(--orange);"></i></div>';

    let items = [];
    if (categoryKey === "featured") {
        items = await ApiClient.getFeaturedProducts();
    } else {
        // نعتبر الـ categoryKey هو الـ Slug الخاص بالقسم في قاعدة البيانات
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

// تعديل الدالة لتستقبل بيانات المنتج مباشرة وتضيفه للسلة
function addToCartDirectly(id, name, price, img) {
    const itemToAdd = {
        id: id,
        name: name,
        price: price,
        img: img,
        quantity: 1
    };

    cart.push(itemToAdd);
    localStorage.setItem('printnesCart', JSON.stringify(cart));
    updateCartBadge();
    showCartToast(itemToAdd);
}

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

function renderCartPage() {
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
                <td><strong>${item.name}</strong><br><small style="color:#888;">الكمية: ${item.quantity || 1}</small></td>
                <td><img src="${item.img}" style="width:50px; height:50px; object-fit:cover; border-radius:5px;"></td>
                <td style="color:var(--orange); font-weight:bold;">${(parseFloat(item.price) * (item.quantity || 1)).toFixed(2)} ر.س</td>
                <td><button class="btn-remove" onclick="removeItem(${index})"><i class="fas fa-trash-alt"></i></button></td>
            </tr>
        `).join('');

        let total = cart.reduce((sum, item) => sum + (parseFloat(item.price) * (item.quantity || 1)), 0);
        if (totalDisplay) totalDisplay.innerText = total.toFixed(2);
    }
}

function removeItem(index) {
    cart.splice(index, 1);
    localStorage.setItem('printnesCart', JSON.stringify(cart));
    renderCartPage();
    updateCartBadge();
}

function checkoutFullCart() {
    // توجيه العميل لصفحة إتمام الطلب الرسمية في الـ C#
    window.location.href = "/Order/Checkout";
}

function openSideMenu() {
    const menu = document.getElementById('side-menu');
    const overlay = document.getElementById('menu-overlay');
    if (menu && overlay) {
        menu.classList.add('active');
        overlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    }
}

function closeSideMenu() {
    const menu = document.getElementById('side-menu');
    const overlay = document.getElementById('menu-overlay');
    if (menu) menu.classList.remove('active');
    if (overlay) overlay.classList.remove('active');
    document.body.style.overflow = 'auto';
}

function toggleSubMenu(element) {
    const parentLi = element.parentElement;
    document.querySelectorAll('.has-submenu').forEach(item => {
        if (item !== parentLi) item.classList.remove('open');
    });
    parentLi.classList.toggle('open');
}

function openLoginModal() {
    const modal = document.getElementById('login-modal');
    if (modal) modal.classList.remove('hidden');
}

function closeLoginModal() {
    const modal = document.getElementById('login-modal');
    if (modal) modal.classList.add('hidden');
}

function toggleFaq(element) {
    element.classList.toggle('active');
}

function goToDetails(id) {
    window.location.href = `/Products/Details/${id}`;
}

function changeQty(val) {
    const input = document.getElementById('prod-qty');
    if (!input) return;
    let current = parseInt(input.value);
    if (current + val >= 1) input.value = current + val;
    if (typeof calculateFinalPrice === "function") calculateFinalPrice();
}

function addToCartFromDetails() {
    const titleEl = document.getElementById('product-title');
    const imgEl = document.getElementById('main-product-img');
    const priceEl = document.getElementById('sticky-price');
    const qtyEl = document.getElementById('prod-qty');

    if (!titleEl || !priceEl) return;

    // جمع بيانات الخيارات المختارة
    const design = document.getElementById('option-design')?.value || "";
    const paperType = document.getElementById('option-paper-type')?.value || "";
    const sides = document.getElementById('option-sides')?.value || "";

    const item = {
        name: titleEl.innerText,
        price: parseFloat(priceEl.innerText.replace(' ر.س', '')),
        img: imgEl ? imgEl.src : '',
        quantity: parseInt(qtyEl.value) || 1,
        details: { design, paperType, sides }
    };

    cart.push(item);
    localStorage.setItem('printnesCart', JSON.stringify(cart));
    updateCartBadge();
    showCartToast(item);
}