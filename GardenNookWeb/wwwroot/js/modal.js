let currentItemCard = null;
let currentToppingCategory = null;
let selectedToppings = {};

// ===== OPEN MODAL =====
function openToppingsModal(button, toppingCategory) {
    currentItemCard = button.closest('.item-card');
    if (!currentItemCard || currentItemCard.dataset.available === 'false') {
        return;
    }

    currentToppingCategory = toppingCategory;
    selectedToppings = {};

    const allToppings = window.menuData.toppings;

    const filtered = allToppings
        .filter(t =>
            t.category.toLowerCase() === toppingCategory.toLowerCase()
            || t.category.toLowerCase() === 'общие'
        )
        .sort((a, b) => {
            const aAvailable = a.isAvailable !== false ? 1 : 0;
            const bAvailable = b.isAvailable !== false ? 1 : 0;
            if (aAvailable !== bAvailable) return bAvailable - aAvailable;
            return (a.name ?? '').localeCompare(b.name ?? '', 'ru');
        });

    renderToppings(filtered);
    updateConfirmButtonText();

    document.getElementById('topping-modal').classList.remove('hidden');
}

// ===== CLOSE =====
function closeToppingsModal() {
    document.getElementById('topping-modal').classList.add('hidden');
    selectedToppings = {};
    currentItemCard = null;

    if (typeof window.resetPendingDrinkModifiers === 'function') {
        window.resetPendingDrinkModifiers();
    }
}

// ===== RENDER =====
function renderToppings(toppings) {
    const list = document.getElementById('topping-list');
    list.innerHTML = '';

    toppings.forEach(t => {
        const isAvailable = t.isAvailable !== false;
        const disabled = isAvailable ? '' : 'disabled';
        const quantity = selectedToppings[t.id] || 0;

        list.innerHTML += `
            <div class="topping-item ${isAvailable ? '' : 'topping-item-unavailable'}">
                <span class="topping-name">${t.name}</span>
                <span class="topping-price">${t.price} ₽</span>

                <div class="topping-actions">
                    <div class="topping-qty-controls">
                        <button onclick="changeToppingQty(${t.id}, -1)" ${disabled}>-</button>
                        <span class="topping-qty">${quantity}</span>
                        <button onclick="changeToppingQty(${t.id}, 1)" ${disabled}>+</button>
                    </div>
                    ${isAvailable ? '' : '<div class="topping-unavailable">Недоступно</div>'}
                </div>
            </div>
        `;
    });
}

function updateConfirmButtonText() {
    const btn = document.getElementById('confirm-toppings-btn');
    if (!btn) return;

    btn.textContent = Object.keys(selectedToppings).length === 0
        ? 'Без добавок'
        : 'Добавить';
}

// ===== SELECT =====
function changeToppingQty(id, delta) {
    const topping = window.menuData.toppings.find(t => t.id === id);
    if (!topping || topping.isAvailable === false) {
        return;
    }

    const current = selectedToppings[id] || 0;
    const next = Math.max(0, current + delta);

    if (next === 0) delete selectedToppings[id];
    else selectedToppings[id] = next;

    const allToppings = window.menuData.toppings;
    const filtered = allToppings
        .filter(t =>
            t.category.toLowerCase() === currentToppingCategory.toLowerCase()
            || t.category.toLowerCase() === 'общие'
        )
        .sort((a, b) => {
            const aAvailable = a.isAvailable !== false ? 1 : 0;
            const bAvailable = b.isAvailable !== false ? 1 : 0;
            if (aAvailable !== bAvailable) return bAvailable - aAvailable;
            return (a.name ?? '').localeCompare(b.name ?? '', 'ru');
        });

    updateConfirmButtonText();
    renderToppings(filtered);
}


