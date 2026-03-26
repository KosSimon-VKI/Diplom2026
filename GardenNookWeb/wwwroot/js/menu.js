// ===== DOM READY =====
document.addEventListener("DOMContentLoaded", () => {
    initCategoryButtons();
    initSearch();
    initPriceFilter();
    updateSubcategories("all");
});

// ===== CATEGORY FILTER =====
function initCategoryButtons() {
    document.querySelectorAll('.main-category-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            document.querySelectorAll('.main-category-btn')
                .forEach(b => b.classList.remove('active'));

            this.classList.add('active');

            const type = this.dataset.type;
            filterItems(type, null);
            updateSubcategories(type);
        });
    });
}



// ===== SEARCH =====
function initSearch() {
    const input = document.getElementById('search-input');
    input.addEventListener('input', e => {
        const searchTerm = e.target.value.toLowerCase();
        const activeType = document.querySelector('.main-category-btn.active').dataset.type;
        filterItems(activeType, searchTerm);
    });
}

// ===== PRICE FILTER =====
function initPriceFilter() {
    const priceFilter = document.getElementById('price-filter');
    const priceValue = document.getElementById('price-value');

    priceFilter.addEventListener('input', () => {
        priceValue.textContent = priceFilter.value;
        const activeType = document.querySelector('.main-category-btn.active').dataset.type;
        const searchTerm = document.getElementById('search-input').value.toLowerCase();
        filterItems(activeType, searchTerm);
    });
}

// ===== FILTER LOGIC =====
function filterItems(type, searchTerm) {
    const items = document.querySelectorAll('.item-card');
    const maxPrice = parseInt(document.getElementById('price-filter').value);

    items.forEach(item => {
        const itemType = item.dataset.type;
        const name = item.querySelector('.item-name')?.textContent.toLowerCase() ?? '';
        const category = item.querySelector('.item-category')?.textContent.toLowerCase() ?? '';
        const price = parseFloat(item.querySelector('.item-price')?.textContent) || 0;

        const matchesType = type === 'all' || itemType === type;
        const matchesSearch = !searchTerm || name.includes(searchTerm) || category.includes(searchTerm);
        const matchesPrice = price <= maxPrice;

        item.style.display = (matchesType && matchesSearch && matchesPrice)
            ? 'flex'
            : 'none';
    });
}

// ===== SUBCATEGORIES =====
function updateSubcategories(type) {
    const container = document.getElementById('subcategories');
    container.innerHTML = '';

    if (type === 'all') return;

    const items = document.querySelectorAll(`.item-card[data-type="${type}"]`);
    const categories = new Set();

    items.forEach(i => categories.add(i.dataset.category));

    let html = '<div class="subcategory-title">Подкатегории:</div><div class="subcategory-buttons">';
    categories.forEach(c => {
        html += `<button class="subcategory-btn" data-category="${c}">${c}</button>`;
    });
    html += '</div>';

    container.innerHTML = html;

    document.querySelectorAll('.subcategory-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            filterBySubcategory(btn.dataset.category);
        });
    });
}

function filterBySubcategory(category) {
    document.querySelectorAll('.item-card').forEach(item => {
        item.style.display = item.dataset.category === category ? 'flex' : 'none';
    });
}