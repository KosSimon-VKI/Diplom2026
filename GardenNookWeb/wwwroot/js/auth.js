window.API_BASE = 'https://localhost:7235';
const CURRENT_USER_STORAGE_KEY = 'gardenNook.currentUser.v1';

(function () {
    const form = document.getElementById('auth-form');
    const phoneInput = document.getElementById('PhoneNumber');
    const passwordInput = document.getElementById('Password');
    const errorBox = document.getElementById('auth-error');

    if (!form || !phoneInput || !passwordInput) return;

    function toDigits(value) {
        let digits = (value || '').replace(/\D/g, '');
        if (!digits) return '';

        if (digits[0] === '8') {
            digits = '7' + digits.slice(1);
        } else if (digits[0] !== '7') {
            digits = '7' + digits;
        }

        return digits.slice(0, 11);
    }

    function toMask(digits) {
        if (!digits) return '';
        const rest = digits.slice(1);

        let masked = '+7';
        if (rest.length > 0) masked += ' (' + rest.slice(0, 3);
        if (rest.length >= 3) masked += ')';
        if (rest.length > 3) masked += ' ' + rest.slice(3, 6);
        if (rest.length > 6) masked += '-' + rest.slice(6, 8);
        if (rest.length > 8) masked += '-' + rest.slice(8, 10);

        return masked;
    }

    function setError(message) {
        if (!errorBox) return;
        errorBox.textContent = message || '';
    }

    function applyMask() {
        phoneInput.value = toMask(toDigits(phoneInput.value));
    }

    async function submitAuth(event) {
        event.preventDefault();
        setError('');

        const phoneNumber = toDigits(phoneInput.value);
        const password = passwordInput.value || '';

        if (!phoneNumber || phoneNumber.length !== 11 || !password) {
            setError('Заполните телефон и пароль');
            return;
        }

        phoneInput.value = toMask(phoneNumber);

        try {
            const response = await fetch(`${window.API_BASE}/api/auth/client`, {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ phoneNumber, password })
            });

            if (!response.ok) {
                setError('Ошибка сервера');
                return;
            }

            const result = await response.json();
            if (!result || !result.client) {
                setError('Неверный телефон или пароль');
                return;
            }
            localStorage.setItem(CURRENT_USER_STORAGE_KEY, phoneNumber);
            window.location.href = '/Menu';
        } catch {
            setError('Ошибка сети. Попробуйте еще раз.');
        }
    }

    applyMask();
    phoneInput.addEventListener('input', applyMask);
    form.addEventListener('submit', submitAuth);
})();

