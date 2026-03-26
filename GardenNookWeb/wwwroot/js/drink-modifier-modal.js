let pendingDrinkModifierContext = null;
let pendingDrinkModifiers = null;

const COFFEE_DRINK_CATEGORY_ID = 1;
const MODIFIER_EXCLUDED_DRINK_IDS = new Set([5, 6, 43, 12]);
const DEFAULT_MILK_OPTION_NAME = '\u041A\u041E\u0420\u041E\u0412\u042C\u0415 \u041C\u041E\u041B\u041E\u041A\u041E';
const DEFAULT_COFFEE_OPTION_NAME = '\u041A\u043E\u0444\u0435 \u0432 \u0437\u0435\u0440\u043D\u0430\u0445 \u0422\u0410\u0412 Galaxy';

function openDrinkModifiersModal(button) {
    const itemCard = button?.closest?.('.item-card');
    if (!itemCard || itemCard.dataset.available === 'false') {
        return;
    }

    const drinkItemId = parseInt(itemCard.dataset.itemId, 10);
    const drinkCategoryId = parseInt(itemCard.dataset.drinkCategoryId, 10);
    const isCoffeeDrink = !Number.isNaN(drinkCategoryId) && drinkCategoryId === COFFEE_DRINK_CATEGORY_ID;
    const isExcludedDrink = !Number.isNaN(drinkItemId) && MODIFIER_EXCLUDED_DRINK_IDS.has(drinkItemId);
    if (!isCoffeeDrink || isExcludedDrink) {
        pendingDrinkModifierContext = null;
        resetPendingDrinkModifiers();
        openToppingsModal(button, '\u041A \u043D\u0430\u043F\u0438\u0442\u043A\u0430\u043C');
        return;
    }

    pendingDrinkModifierContext = {
        button,
        drinkItemId: drinkItemId
    };

    renderDrinkModifierOptions();
    document.getElementById('drink-modifier-modal')?.classList.remove('hidden');
}

function closeDrinkModifiersModal() {
    document.getElementById('drink-modifier-modal')?.classList.add('hidden');
    pendingDrinkModifierContext = null;
    resetPendingDrinkModifiers();
}

function confirmDrinkModifiers() {
    if (!pendingDrinkModifierContext || Number.isNaN(pendingDrinkModifierContext.drinkItemId)) {
        return;
    }

    const milkIngredientId = readSelectedModifierId('drink-modifier-milk');
    const milkIngredientName = readSelectedModifierLabel('drink-modifier-milk');
    const coffeeIngredientId = readSelectedModifierId('drink-modifier-coffee');
    const coffeeIngredientName = readSelectedModifierLabel('drink-modifier-coffee');

    pendingDrinkModifiers = {
        drinkItemId: pendingDrinkModifierContext.drinkItemId,
        milkIngredientId: milkIngredientId,
        milkIngredientName: milkIngredientName,
        coffeeIngredientId: coffeeIngredientId,
        coffeeIngredientName: coffeeIngredientName
    };

    const sourceButton = pendingDrinkModifierContext.button;
    pendingDrinkModifierContext = null;

    document.getElementById('drink-modifier-modal')?.classList.add('hidden');
    openToppingsModal(sourceButton, '\u041A \u043D\u0430\u043F\u0438\u0442\u043A\u0430\u043C');
}

function consumePendingDrinkModifiers(expectedDrinkItemId) {
    if (!pendingDrinkModifiers) {
        return null;
    }

    if (pendingDrinkModifiers.drinkItemId !== expectedDrinkItemId) {
        pendingDrinkModifiers = null;
        return null;
    }

    const result = {
        milkIngredientId: pendingDrinkModifiers.milkIngredientId ?? null,
        milkIngredientName: pendingDrinkModifiers.milkIngredientName ?? null,
        coffeeIngredientId: pendingDrinkModifiers.coffeeIngredientId ?? null,
        coffeeIngredientName: pendingDrinkModifiers.coffeeIngredientName ?? null
    };

    pendingDrinkModifiers = null;
    return result;
}

function resetPendingDrinkModifiers() {
    pendingDrinkModifiers = null;
}

function renderDrinkModifierOptions() {
    const modifiers = window.menuData?.drinkModifiers ?? {};

    renderModifierGroup(
        'drink-modifier-milk-options',
        'drink-modifier-milk',
        modifiers.milkOptions ?? [],
        DEFAULT_MILK_OPTION_NAME);

    renderModifierGroup(
        'drink-modifier-coffee-options',
        'drink-modifier-coffee',
        modifiers.coffeeOptions ?? [],
        DEFAULT_COFFEE_OPTION_NAME);
}

function renderModifierGroup(containerId, inputName, options, defaultOptionName) {
    const container = document.getElementById(containerId);
    if (!container) {
        return;
    }

    const normalizedDefaultName = normalizeModifierName(defaultOptionName);
    const mappedOptions = [];

    options.forEach(option => {
        const id = parseInt(option.id, 10);
        if (Number.isNaN(id)) {
            return;
        }

        mappedOptions.push({
            value: String(id),
            label: option.name ?? `Ingredient #${id}`
        });
    });

    if (mappedOptions.length === 0) {
        container.innerHTML = '';
        return;
    }

    const selectedValue =
        mappedOptions.find(option => normalizeModifierName(option.label) === normalizedDefaultName)?.value
        ?? mappedOptions[0].value;

    const entries = mappedOptions.map(option =>
        buildModifierOptionHtml(inputName, option.value, option.label, option.value === selectedValue));

    container.innerHTML = entries.join('');
}

function buildModifierOptionHtml(inputName, value, label, checked) {
    const optionId = `${inputName}-${value}`;
    const safeLabel = escapeHtml(label);
    const checkedAttr = checked ? 'checked' : '';

    return `
        <label class="drink-modifier-option" for="${optionId}">
            <input id="${optionId}" type="radio" name="${inputName}" value="${value}" ${checkedAttr} />
            <span>${safeLabel}</span>
        </label>
    `;
}

function readSelectedModifierId(inputName) {
    const selected = document.querySelector(`input[name="${inputName}"]:checked`);
    if (!selected || !selected.value) {
        return null;
    }

    const parsed = parseInt(selected.value, 10);
    return Number.isNaN(parsed) ? null : parsed;
}

function readSelectedModifierLabel(inputName) {
    const selected = document.querySelector(`input[name="${inputName}"]:checked`);
    if (!selected) {
        return null;
    }

    const labelNode = selected.closest('label')?.querySelector('span');
    const value = labelNode?.textContent?.trim() ?? '';
    return value.length > 0 ? value : null;
}

function normalizeModifierName(value) {
    return String(value ?? '')
        .trim()
        .replace(/\s+/g, ' ')
        .toLocaleUpperCase('ru-RU');
}

function escapeHtml(value) {
    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/\"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

window.consumePendingDrinkModifiers = consumePendingDrinkModifiers;
window.resetPendingDrinkModifiers = resetPendingDrinkModifiers;
