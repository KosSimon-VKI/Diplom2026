window.API_BASE = 'https://localhost:7235';

const CURRENT_USER_STORAGE_KEY = 'gardenNook.currentUser.v1';
const CART_STORAGE_PREFIX = 'gardenNook.cart.v2.';
const LEGACY_CART_STORAGE_KEY = 'gardenNook.cart.v1';

function getCartStorageKey() {
    const currentUser = (localStorage.getItem(CURRENT_USER_STORAGE_KEY) || 'guest').trim();
    return `${CART_STORAGE_PREFIX}${currentUser}`;
}

function loadCartForBadge() {
    const cartStorageKey = getCartStorageKey();
    let raw = localStorage.getItem(cartStorageKey);

    if (!raw) {
        raw = localStorage.getItem(LEGACY_CART_STORAGE_KEY);
    }

    if (!raw) return [];

    try {
        return JSON.parse(raw) ?? [];
    } catch {
        return [];
    }
}

function updateCartBadge() {
    const badge = document.getElementById('cart-badge');
    if (!badge) return;

    const cart = loadCartForBadge();
    const count = cart.reduce((sum, item) => sum + (item.quantity || 0), 0);

    badge.textContent = String(count);
    if (count > 0) badge.classList.remove('hidden');
    else badge.classList.add('hidden');
}

function clearMessages() {
    const errorBox = document.getElementById('profile-error');
    const successBox = document.getElementById('profile-success');

    if (errorBox) {
        errorBox.textContent = '';
        errorBox.classList.add('hidden');
    }

    if (successBox) {
        successBox.textContent = '';
        successBox.classList.add('hidden');
    }
}

function showError(message) {
    const box = document.getElementById('profile-error');
    if (!box) return;
    box.textContent = message;
    box.classList.remove('hidden');
}

function showSuccess(message) {
    const box = document.getElementById('profile-success');
    if (!box) return;
    box.textContent = message;
    box.classList.remove('hidden');
}

function formatDate(value) {
    if (!value) return '-';

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return '-';

    return date.toLocaleString('ru-RU');
}

function formatNumber(value) {
    const number = Number(value);
    if (!Number.isFinite(number)) return '0';

    return number.toLocaleString('ru-RU', {
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
    });
}

function escapeHtml(value) {
    return String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}

function renderOrderItems(items) {
    if (!Array.isArray(items) || items.length === 0) {
        return '<div class="order-history-composition empty">Состав заказа недоступен.</div>';
    }

    const itemsHtml = items.map(item => {
        const addons = Array.isArray(item.addons) ? item.addons : [];
        const addonsHtml = addons.length === 0
            ? ''
            : `
                <div class="order-composition-addons-title">Добавки:</div>
                <div class="order-composition-addons">
                    ${addons.map(addon => `
                        <div class="order-composition-addon">
                            ${escapeHtml(addon.name || 'Без названия')} × ${formatNumber(addon.quantity)} · ${formatNumber(addon.totalPrice)} ₽
                        </div>
                    `).join('')}
                </div>
            `;

        return `
            <div class="order-composition-item">
                <div class="order-composition-name">${escapeHtml(item.type || 'Позиция')}: ${escapeHtml(item.name || 'Без названия')}</div>
                <div class="order-composition-meta">Количество: ${formatNumber(item.quantity)} · Стоимость: ${formatNumber(item.totalPrice)} ₽</div>
                ${addonsHtml}
            </div>
        `;
    }).join('');

    return `
        <div class="order-history-composition">
            <div class="order-history-composition-title">Состав заказа:</div>
            <div class="order-composition-list">
                ${itemsHtml}
            </div>
        </div>
    `;
}

function renderOrders(orders) {
    const container = document.getElementById('profile-orders');
    if (!container) return;

    if (!orders || orders.length === 0) {
        container.innerHTML = '<div class="order-history-empty">У вас пока нет заказов.</div>';
        return;
    }

    container.innerHTML = orders.map(order => {
        const status = escapeHtml(order.status || '-');
        const statusClass = !order.canCancel
            ? 'order-history-status cancelled'
            : 'order-history-status';

        const comment = order.comment
            ? `<div class="order-history-comment">Комментарий: ${escapeHtml(order.comment)}</div>`
            : '';
        const pickupAt = order.pickupAt
            ? `<div class="order-history-meta">Самовывоз: ${formatDate(order.pickupAt)}</div>`
            : '';
        const composition = renderOrderItems(order.items || []);

        const cancelButton = order.canCancel
            ? `<button class="order-cancel-btn" data-order-id="${order.orderId}">Отменить заказ</button>`
            : '';

        return `
            <div class="order-history-item">
                <div class="order-history-top">
                    <div class="order-history-title">Заказ №${order.orderId}</div>
                    <div class="${statusClass}">${status}</div>
                </div>

                <div class="order-history-meta">Дата: ${formatDate(order.createdAt)}</div>
                <div class="order-history-meta">Тип: ${escapeHtml(order.orderType || '-')}</div>
                ${pickupAt}
                <div class="order-history-meta">Сумма: ${formatNumber(order.totalPrice)} ₽</div>
                <div class="order-history-meta">Калории: ${formatNumber(order.totalCalories)}</div>

                ${composition}
                ${comment}
                ${cancelButton}
            </div>
        `;
    }).join('');
}

async function loadProfile() {
    clearMessages();

    try {
        const response = await fetch(`${window.API_BASE}/api/profile`, {
            method: 'GET',
            credentials: 'include'
        });

        if (response.status === 401) {
            window.location.href = '/';
            return;
        }

        if (!response.ok) {
            const errorText = await response.text();
            showError(errorText || 'Не удалось загрузить профиль');
            return;
        }

        const data = await response.json();
        document.getElementById('profile-fullname').textContent = data?.client?.fullName || '-';
        document.getElementById('profile-category').textContent = data?.client?.category || '-';

        renderOrders(data?.orders || []);
    } catch {
        showError('Ошибка сети. Попробуйте ещё раз.');
    }
}

async function cancelOrder(orderId, button) {
    clearMessages();
    if (button) button.disabled = true;

    try {
        const response = await fetch(`${window.API_BASE}/api/profile/orders/${orderId}/cancel`, {
            method: 'POST',
            credentials: 'include'
        });

        if (response.status === 401) {
            window.location.href = '/';
            return;
        }

        if (!response.ok) {
            const errorText = await response.text();
            showError(errorText || 'Не удалось отменить заказ');
            return;
        }

        const result = await response.json();
        await loadProfile();
        showSuccess(`Заказ №${result.orderId} обновлён. Текущий статус: ${result.status}`);
    } catch {
        showError('Ошибка сети. Попробуйте ещё раз.');
    } finally {
        if (button) button.disabled = false;
    }
}

document.addEventListener('click', event => {
    const target = event.target;
    if (!(target instanceof HTMLElement)) return;

    if (!target.classList.contains('order-cancel-btn')) return;

    const rawOrderId = target.dataset.orderId;
    const orderId = Number(rawOrderId);
    if (!Number.isFinite(orderId) || orderId <= 0) return;

    cancelOrder(orderId, target);
});

document.addEventListener('DOMContentLoaded', () => {
    updateCartBadge();
    loadProfile();
});

