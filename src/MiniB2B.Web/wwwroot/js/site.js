function showToast(message, isSuccess = true) {
    let container = document.getElementById('b2b-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'b2b-toast-container';
        container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        container.style.zIndex = '1100';
        document.body.appendChild(container);
    }

    const toastId = 'toast_' + Date.now();
    const bgClass = isSuccess ? 'bg-success text-white' : 'bg-danger text-white';
    const iconClass = isSuccess ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill';

    const html = `
        <div id="${toastId}" class="toast align-items-center ${bgClass} border-0 shadow" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center gap-2">
                    <i class="bi ${iconClass} fs-5"></i>
                    <div>${message}</div>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Kapat"></button>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', html);
    const toastEl = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastEl, { delay: 3500 });
    toast.show();

    toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
}

function updateCartBadge(count) {
    const badge = document.getElementById('cart-badge-count');
    if (badge) {
        badge.textContent = count;
        badge.style.display = count > 0 ? 'inline-block' : 'none';
    }
}

async function quickAddToCart(productId, inputId) {
    let quantity = 1;
    if (inputId) {
        const input = document.getElementById(inputId);
        if (input) {
            quantity = parseInt(input.value) || 1;
        }
    }

    if (quantity < 1) {
        showToast('Lütfen geçerli bir adet giriniz.', false);
        return;
    }

    try {
        const response = await fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ productId, quantity })
        });

        const data = await response.json();

        if (data.requireLogin) {
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
            return;
        }

        if (data.success) {
            showToast(data.message || 'Ürün sepete eklendi.', true);
            updateCartBadge(data.cartCount);
        } else {
            showToast(data.message || 'Ürün sepete eklenemedi.', false);
        }
    } catch (err) {
        showToast('Bir bağlantı hatası oluştu.', false);
    }
}

async function openProductDetail(productId) {
    try {
        const response = await fetch(`/Product/DetailModal?id=${productId}`);
        if (!response.ok) {
            showToast('Ürün detayları yüklenemedi.', false);
            return;
        }

        const html = await response.text();
        let modalContainer = document.getElementById('productDetailModalContainer');
        if (!modalContainer) {
            modalContainer = document.createElement('div');
            modalContainer.id = 'productDetailModalContainer';
            document.body.appendChild(modalContainer);
        }

        modalContainer.innerHTML = html;
        const modalEl = document.getElementById('productDetailModal');
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
    } catch (err) {
        showToast('Detay yüklenirken hata meydana geldi.', false);
    }
}

async function updateCartItemQuantity(cartItemId, newQty) {
    const qty = parseInt(newQty);
    if (isNaN(qty) || qty < 1) return;

    try {
        const response = await fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ cartItemId, quantity: qty })
        });

        const data = await response.json();
        if (data.success) {
            location.reload();
        } else {
            showToast(data.message, false);
        }
    } catch (err) {
        showToast('Sepet güncellenemedi.', false);
    }
}

async function removeCartItem(cartItemId) {
    if (!confirm('Bu ürünü sepetten çıkarmak istediğinize emin misiniz?')) return;

    try {
        const response = await fetch('/Cart/Remove', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ cartItemId })
        });

        const data = await response.json();
        if (data.success) {
            location.reload();
        } else {
            showToast(data.message, false);
        }
    } catch (err) {
        showToast('İşlem başarısız oldu.', false);
    }
}
