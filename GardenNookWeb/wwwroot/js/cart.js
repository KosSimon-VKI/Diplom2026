window.API_BASE = 'https://localhost:7235';
const CURRENT_USER_STORAGE_KEY = 'gardenNook.currentUser.v1';
const CART_STORAGE_PREFIX = 'gardenNook.cart.v2.';
const LEGACY_CART_STORAGE_KEY = 'gardenNook.cart.v1';
const DEFAULT_TAKEAWAY_ORDER_TYPE_ID = 2;

function getCartStorageKey() {
    const currentUser = (localStorage.getItem(CURRENT_USER_STORAGE_KEY) || 'guest').trim();
    return `${CART_STORAGE_PREFIX}${currentUser}`;
}


function loadCart() {
    const cartStorageKey = getCartStorageKey();
    let raw = localStorage.getItem(cartStorageKey);

    // Одноразовая миграция со старого общего ключа на новый, привязанный к пользователю.
    if (!raw) {
        const legacy = localStorage.getItem(LEGACY_CART_STORAGE_KEY);
        if (legacy) {
            raw = legacy;
            localStorage.setItem(cartStorageKey, legacy);
            localStorage.removeItem(LEGACY_CART_STORAGE_KEY);
        }
    }
    if (!raw) return [];
    try { return JSON.parse(raw) ?? []; }
    catch { return []; }
}

function saveCart() {
    localStorage.setItem(getCartStorageKey(), JSON.stringify(cart));
}

let cart = loadCart();
let pickupTakeawayOrderTypeId = DEFAULT_TAKEAWAY_ORDER_TYPE_ID;
let pickupSlots = [];
let pickupSlotsLoaded = false;
let pickupSlotsLoadError = null;

// ===== бейдж над кнопкой корзины =====
function updateCartBadge() {
    const badge = document.getElementById('cart-badge');
    if (!badge) return;

    const count = cart.reduce((sum, i) => sum + (i.quantity || 0), 0);

    badge.textContent = String(count);
    if (count > 0) badge.classList.remove('hidden');
    else badge.classList.add('hidden');
}

//=============ОТОБРАЖЕНИЕ НАДПИСИ В ЗАВИСИМОСТИ ОТ КОЛИЧЕСТВА==================//
function initMenuQuantities() {
    document.querySelectorAll('.item-card').forEach(card => {
        const qtyEl = card.querySelector('.quantity');
        const labelEl = card.querySelector('.quantity-label');

        if (!qtyEl) return;

        const qty = parseInt(qtyEl.textContent || '0', 10);

        if (qty === 0) {
            qtyEl.classList.add("hidden-quantity");
            labelEl?.classList.add("hidden-quantity");
        } else {
            qtyEl.classList.remove("hidden-quantity");
            labelEl?.classList.remove("hidden-quantity");
        }
    });
}

//=============СИНХРОНИЗАЦИЯ КОЛ-ВА В МЕНЮ И КОРЗИНЕ=============//
function syncMenuQuantity(itemName, itemType) {
    const cards = document.querySelectorAll(`.item-card[data-type="${itemType}"]`);

    cards.forEach(card => {
        const name = card.querySelector('.item-name')?.textContent?.trim();
        if (name !== itemName) return;

        const totalQty = cart
            .filter(i => i.name === itemName && i.type === itemType)
            .reduce((sum, i) => sum + i.quantity, 0);

        const qtyEl = card.querySelector('.quantity');
        const labelEl = card.querySelector('.quantity-label');
        if (!qtyEl) return;

        qtyEl.textContent = totalQty;

        if (totalQty === 0) {
            qtyEl.classList.add("hidden-quantity");
            labelEl?.classList.add("hidden-quantity");
        } else {
            qtyEl.classList.remove("hidden-quantity");
            labelEl?.classList.remove("hidden-quantity");
        }
    });
}


function syncAllMenuQuantitiesFromCart() {
    const cards = document.querySelectorAll('.item-card[data-item-id][data-type]');
    if (!cards.length) return;

    cards.forEach(card => {
        const qtyEl = card.querySelector('.quantity');
        if (qtyEl) qtyEl.textContent = '0';
    });

    cards.forEach(card => {
        const type = card.dataset.type;
        const itemId = parseInt(card.dataset.itemId, 10);
        const qtyEl = card.querySelector('.quantity');
        const labelEl = card.querySelector('.quantity-label');
        if (!qtyEl || Number.isNaN(itemId)) return;

        const totalQty = cart
            .filter(i => i.type === type && i.itemId === itemId)
            .reduce((sum, i) => sum + (i.quantity || 0), 0);

        qtyEl.textContent = String(totalQty);

        if (totalQty === 0) {
            qtyEl.classList.add("hidden-quantity");
            labelEl?.classList.add("hidden-quantity");
        } else {
            qtyEl.classList.remove("hidden-quantity");
            labelEl?.classList.remove("hidden-quantity");
        }
    });
}

//=====Отдельно добавки(без привязки к блюду)=====//
function addStandaloneTopping(card) {
    if (!card || card.dataset.available === 'false') {
        return;
    }

    const data = getItemDataFromCard(card);

    cart.push({
        id: crypto.randomUUID(),
        number: cart.length + 1,
        type: 'toppings',
        itemId: data.itemId,
        name: data.name,
        price: data.price,
        quantity: 1
    });

    saveCart();
    updateCartBadge();

    syncMenuQuantity(data.name, 'toppings');
    renderCart();
}

// ===== CONFIRM FROM MODAL =====
function confirmToppings() {
    if (!currentItemCard) return;
    if (currentItemCard.dataset.available === 'false') return;

    const itemData = getItemDataFromCard(currentItemCard);
    const selectedDrinkModifiers = itemData.type === 'drinks' && typeof window.consumePendingDrinkModifiers === 'function'
        ? window.consumePendingDrinkModifiers(itemData.itemId)
        : null;

    const cartItem = {
        id: crypto.randomUUID(),
        number: cart.length + 1,
        type: itemData.type,
        itemId: itemData.itemId,
        name: itemData.name,
        price: itemData.price,
        quantity: 1,
        toppings: []
    };

    if (itemData.type === 'drinks') {
        cartItem.milkIngredientId = selectedDrinkModifiers?.milkIngredientId ?? null;
        cartItem.milkIngredientName = selectedDrinkModifiers?.milkIngredientName ?? null;
        cartItem.coffeeIngredientId = selectedDrinkModifiers?.coffeeIngredientId ?? null;
        cartItem.coffeeIngredientName = selectedDrinkModifiers?.coffeeIngredientName ?? null;
    }

    for (const toppingId in selectedToppings) {
        const topping = window.menuData.toppings.find(t => t.id == toppingId);
        if (!topping || topping.isAvailable === false) continue;

        cartItem.toppings.push({
            id: topping.id,
            name: topping.name,
            price: topping.price,
            quantity: selectedToppings[toppingId]
        });
    }

    cart.push(cartItem);

    saveCart();
    updateCartBadge();

    syncMenuQuantity(cartItem.name, cartItem.type);

    renderCart();
    closeToppingsModal();
}

function getItemDataFromCard(card) {
    return {
        type: card.dataset.type,
        name: card.querySelector('.item-name')?.textContent?.trim() ?? '',
        itemId: parseInt(card.dataset.itemId, 10),
        price: parseFloat(card.querySelector('.item-price')?.textContent ?? '0'),
    };
}

function buildCartItemDetailsHtml(item) {
    const lines = [];

    if (item.type === 'drinks') {
        const modifierLines = buildDrinkModifierLines(item);
        modifierLines.forEach(line => {
            lines.push(`<div class="cart-topping cart-modifier">${line}</div>`);
        });
    }

    (item.toppings ?? []).forEach(t => {
        lines.push(`<div class="cart-topping">+ ${t.name} ×${t.quantity}</div>`);
    });

    if (lines.length === 0) {
        return '';
    }

    return `<div class="cart-toppings">${lines.join('')}</div>`;
}

function buildDrinkModifierLines(item) {
    const lines = [];

    const milkLine = resolveModifierLine('Молоко', item.milkIngredientName, item.milkIngredientId);
    if (milkLine) {
        lines.push(milkLine);
    }

    const coffeeLine = resolveModifierLine('Кофе', item.coffeeIngredientName, item.coffeeIngredientId);
    if (coffeeLine) {
        lines.push(coffeeLine);
    }

    return lines;
}

function resolveModifierLine(label, name, id) {
    const normalizedName = typeof name === 'string' ? name.trim() : '';
    if (normalizedName) {
        return `${label}: ${normalizedName}`;
    }

    if (id === null || id === undefined || id === '') {
        return null;
    }

    return `${label} #${id}`;
}

// ===== RENDER CART =====
function renderCart() {
    const container = document.getElementById('cart-items');
    if (!container) {
        updateCartTotal();
        return;
    }

    container.innerHTML = '';

    cart.forEach(item => {
        const detailsHtml = buildCartItemDetailsHtml(item);

        container.innerHTML += `
            <div class="cart-item">
                <div class="cart-main">
                    <span class="cart-name">${item.number})</span>
                    <span class="cart-name">${item.name}</span>
                    <span class="cart-price">${item.price} ₽</span>
                </div>

                <div class="cart-controls">
                    <button class="cart-quanity-btn" onclick="changeCartQty('${item.id}', -1)">-</button>
                    <span class="cart-quanity">${item.quantity}</span>
                    <button class="cart-quanity-btn" onclick="changeCartQty('${item.id}', 1)">+</button>
                    <button class="cart-remove" onclick="removeCartItem('${item.id}')">✕</button>
                </div>

                ${detailsHtml}
            </div>
        `;
    });

    updateCartTotal();
}

// ===== CART ACTIONS =====
function removeCartItem(id) {
    const item = cart.find(i => i.id === id);
    if (!item) return;

    cart = cart.filter(i => i.id !== id);
    cart.forEach((x, idx) => x.number = idx + 1);

    saveCart();
    updateCartBadge();

    syncMenuQuantity(item.name, item.type);

    renderCart();
    syncAllMenuQuantitiesFromCart();
}

function changeCartQty(id, delta) {
    const item = cart.find(i => i.id === id);
    if (!item) return;

    item.quantity += delta;

    if (item.quantity <= 0) {
        removeCartItem(id);
        return;
    }

    saveCart();
    updateCartBadge();

    syncMenuQuantity(item.name, item.type);
    renderCart();
    syncAllMenuQuantitiesFromCart();
}

// ===== TOTAL =====
function updateCartTotal() {
    let total = 0;

    cart.forEach(item => {
        total += item.price * item.quantity;

        item.toppings?.forEach(t => {
            total += t.price * t.quantity * item.quantity;
        });
    });

    const totalEl = document.getElementById('cart-total-price');
    if (totalEl) totalEl.textContent = String(total);
}

// ===== Checkout helpers =====
function getSelectedOrderTypeId() {
    const el = document.querySelector('input[name="orderType"]:checked');
    return el ? parseInt(el.value, 10) : 1;
}

function hasPickupControls() {
    return !!document.getElementById('pickup-slot-select');
}

function isTakeawayOrderType(orderTypeId) {
    return orderTypeId === pickupTakeawayOrderTypeId;
}

function getPickupSlotSelectEl() {
    return document.getElementById('pickup-slot-select');
}

function getPickupTimeSectionEl() {
    return document.getElementById('pickup-time-section');
}

function getPickupSlotNoteEl() {
    return document.getElementById('pickup-slot-note');
}

function setPickupSlotNote(message) {
    const note = getPickupSlotNoteEl();
    if (!note) return;

    note.textContent = message;
    note.classList.remove('hidden');
}

function clearPickupSlotNote() {
    const note = getPickupSlotNoteEl();
    if (!note) return;

    note.textContent = '';
    note.classList.add('hidden');
}

function renderPickupSlotOptions({ keepSelection = true } = {}) {
    const select = getPickupSlotSelectEl();
    if (!select) return;

    const prevValue = keepSelection ? select.value : '';
    select.innerHTML = '';

    pickupSlots.forEach(slot => {
        const option = document.createElement('option');
        option.value = slot.value;
        option.textContent = slot.label;
        select.appendChild(option);
    });

    if (prevValue && Array.from(select.options).some(x => x.value === prevValue)) {
        select.value = prevValue;
        return;
    }

    if (pickupSlots.length > 0) {
        select.value = pickupSlots[0].value;
        return;
    }

    select.value = '';
}

function clearPickupSlotSelection() {
    const select = getPickupSlotSelectEl();
    if (!select) return;
    select.value = '';
}

function updatePickupSlotVisibility() {
    if (!hasPickupControls()) {
        return;
    }

    const section = getPickupTimeSectionEl();
    if (!section) return;

    const orderTypeId = getSelectedOrderTypeId();
    if (!isTakeawayOrderType(orderTypeId)) {
        section.classList.add('hidden');
        clearPickupSlotSelection();
        clearPickupSlotNote();
        return;
    }

    section.classList.remove('hidden');

    if (pickupSlotsLoadError) {
        setPickupSlotNote('Не удалось загрузить слоты самовывоза. Можно оформить заказ без времени.');
        return;
    }

    if (!pickupSlotsLoaded) {
        setPickupSlotNote('Загружаем доступные слоты...');
        return;
    }

    if (pickupSlots.length === 0) {
        setPickupSlotNote('На сегодня нет доступных слотов. Можно оформить заказ без времени.');
        return;
    }

    const select = getPickupSlotSelectEl();
    if (select && !(select.value ?? '').trim()) {
        select.value = pickupSlots[0].value;
    }

    clearPickupSlotNote();
}

function getSelectedPickupAt(orderTypeId) {
    if (!isTakeawayOrderType(orderTypeId)) {
        return null;
    }

    const select = getPickupSlotSelectEl();
    if (!select) {
        return null;
    }

    const value = (select.value ?? '').trim();
    return value || null;
}

async function loadPickupSlots() {
    if (!hasPickupControls()) {
        return;
    }

    pickupSlotsLoaded = false;
    pickupSlotsLoadError = null;
    updatePickupSlotVisibility();

    try {
        const response = await fetch(`${window.API_BASE}/api/orders/pickup-slots`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error(`Pickup slots request failed with status ${response.status}`);
        }

        const payload = await response.json();
        const parsedOrderTypeId = Number(payload?.takeawayOrderTypeId);

        pickupTakeawayOrderTypeId = Number.isFinite(parsedOrderTypeId)
            ? parsedOrderTypeId
            : DEFAULT_TAKEAWAY_ORDER_TYPE_ID;

        pickupSlots = Array.isArray(payload?.slots)
            ? payload.slots
                .filter(x => x && typeof x.value === 'string' && typeof x.label === 'string')
                .map(x => ({ value: x.value, label: x.label }))
            : [];

        pickupSlotsLoaded = true;
        pickupSlotsLoadError = null;
        renderPickupSlotOptions();
    } catch {
        pickupSlotsLoaded = true;
        pickupSlotsLoadError = 'failed';
        pickupSlots = [];
        renderPickupSlotOptions({ keepSelection: false });
    }

    updatePickupSlotVisibility();
}

//================ DTO =================//
function buildOrderRequest(orderTypeId, comment, pickupAt) {
    const dto = {
        orderTypeId: orderTypeId,
        comment: comment ?? null,
        pickupAt: pickupAt ?? null,
        dishes: [],
        drinks: [],
        toppings: []
    };

    for (const item of cart) {
        if (item.type === 'dishes') {
            dto.dishes.push({
                dishId: item.itemId,
                quantity: item.quantity,
                toppings: (item.toppings ?? []).map(t => ({
                    toppingId: t.id,
                    quantity: t.quantity
                }))
            });
        } else if (item.type === 'drinks') {
            dto.drinks.push({
                drinkId: item.itemId,
                quantity: item.quantity,
                milkIngredientId: item.milkIngredientId ?? null,
                coffeeIngredientId: item.coffeeIngredientId ?? null,
                toppings: (item.toppings ?? []).map(t => ({
                    toppingId: t.id,
                    quantity: t.quantity
                }))
            });
        } else if (item.type === 'toppings') {
            dto.toppings.push({
                toppingId: item.itemId,
                quantity: item.quantity
            });
        }
    }

    return dto;
}

function showOrderError(message) {
    const box = document.getElementById('order-error');
    if (!box) return;
    box.textContent = message;
    box.classList.remove('hidden');
}

function clearOrderError() {
    const box = document.getElementById('order-error');
    if (!box) return;
    box.textContent = '';
    box.classList.add('hidden');
}

function showOrderSuccess(message) {
    const box = document.getElementById('order-success');
    if (!box) return;
    box.textContent = message;
    box.classList.remove('hidden');
}

function clearOrderSuccess() {
    const box = document.getElementById('order-success');
    if (!box) return;
    box.textContent = '';
    box.classList.add('hidden');
}

//================ SUBMIT ORDER (на API, с cookie) =================//
async function submitOrder() {
    clearOrderError();
    clearOrderSuccess();

    const btn = document.getElementById('btn-submit-order');
    const orderTypeId = getSelectedOrderTypeId();
    const comment = document.getElementById('order-comment')?.value ?? null;
    const pickupAt = getSelectedPickupAt(orderTypeId);

    const dto = buildOrderRequest(orderTypeId, comment, pickupAt);

    if (!dto.dishes.length && !dto.drinks.length && !dto.toppings.length) {
        showOrderError('Корзина пуста');
        return;
    }

    if (btn) btn.disabled = true;

    try {
        const res = await fetch(`${window.API_BASE}/api/orders`, {
            method: 'POST',
            credentials: 'include', //  обязательно
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        if (!res.ok) {
            if (res.status === 401) {
                showOrderError('Нужно войти в аккаунт (401)');
                return;
            }

            if (res.status === 409) {
                let conflictMessage = 'Недостаточно заготовок. Обнови меню и повтори заказ.';
                try {
                    const conflict = await res.json();
                    if (conflict?.message) {
                        conflictMessage = conflict.message;
                    }
                } catch {
                    // no-op
                }
                showOrderError(conflictMessage);
                return;
            }

            const errText = await res.text();
            showOrderError(errText || 'Не удалось оформить заказ');
            return;
        }

        const data = await res.json();
        console.log('Order created:', data);

        cart = [];
        saveCart();
        updateCartBadge();

        renderCart();
        syncAllMenuQuantitiesFromCart();

        const c = document.getElementById('order-comment');
        if (c) c.value = '';
        renderPickupSlotOptions({ keepSelection: false });
        updatePickupSlotVisibility();

        showOrderSuccess(`Заказ №${data.orderId} оформлен. Статус: ${data.status}`);

    } catch (e) {
        showOrderError('Ошибка сети. Попробуй ещё раз.');
    } finally {
        if (btn) btn.disabled = false;
    }
}

// ===== init =====
document.addEventListener('DOMContentLoaded', () => {
    updateCartBadge();
    syncAllMenuQuantitiesFromCart();

    renderCart();
    initMenuQuantities();

    if (hasPickupControls()) {
        renderPickupSlotOptions({ keepSelection: false });
        document.querySelectorAll('input[name="orderType"]').forEach(el => {
            el.addEventListener('change', updatePickupSlotVisibility);
        });

        updatePickupSlotVisibility();
        loadPickupSlots();
    }

    const btn = document.getElementById('btn-submit-order');
    if (btn) btn.addEventListener('click', submitOrder);
});

