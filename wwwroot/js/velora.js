/**
 * VELORA — Main JavaScript
 * Premium Fashion & Lifestyle Store
 */

'use strict';

// ─── Page Loader ─────────────────────────────────────────────────────────────
(function () {
  const loader = document.getElementById('pageLoader');
  if (loader) {
    window.addEventListener('load', () => {
      setTimeout(() => loader.classList.add('hide'), 300);
    });
  }
})();

// ─── Toast System ─────────────────────────────────────────────────────────────
const Velora = {
  toastContainer: null,

  init() {
    this.toastContainer = document.querySelector('.toast-container-velora');
    if (!this.toastContainer) {
      this.toastContainer = document.createElement('div');
      this.toastContainer.className = 'toast-container-velora';
      document.body.appendChild(this.toastContainer);
    }
    this.initCartBadge();
    this.initSearchAutocomplete();
    this.initHeroCarousel();
    this.initScrollAnimations();
    this.initNavbarScroll();
    this.initProductActions();
    this.initQuantityInputs();
    this.initGallery();
    this.initFilters();
    this.initAdminSidebar();
    this.initTempDataToasts();
  },

  toast(message, type = 'gold', duration = 3500) {
    const icons = {
      success: 'fa-circle-check',
      error:   'fa-circle-xmark',
      info:    'fa-circle-info',
      warning: 'fa-triangle-exclamation',
      gold:    'fa-sparkles'
    };
    const el = document.createElement('div');
    el.className = `velora-toast toast-${type}`;
    el.innerHTML = `
      <i class="fa-solid ${icons[type] || icons.gold} toast-icon"></i>
      <span class="toast-text">${message}</span>
      <button class="toast-close" onclick="this.closest('.velora-toast').remove()">
        <i class="fa-solid fa-xmark"></i>
      </button>`;
    this.toastContainer.appendChild(el);
    setTimeout(() => {
      el.classList.add('removing');
      el.addEventListener('animationend', () => el.remove(), { once: true });
    }, duration);
  },

  // ─── Cart Badge ─────────────────────────────────────────────────────────────
  initCartBadge() {
    this.updateCartBadge();
  },

  async updateCartBadge() {
    try {
      const res = await fetch('/Cart/GetCartCount');
      const data = await res.json();
      const badges = document.querySelectorAll('.cart-badge');
      badges.forEach(b => {
        b.textContent = data.count || '0';
        b.style.display = data.count > 0 ? 'flex' : 'none';
      });
    } catch {}
  },

  // ─── Add to Cart ────────────────────────────────────────────────────────────
  async addToCart(productId, quantity = 1, size = null, color = null) {
    try {
      const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      const res = await fetch('/Cart/AddToCart', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `productId=${productId}&quantity=${quantity}&size=${size || ''}&color=${color || ''}&__RequestVerificationToken=${token}`
      });
      const data = await res.json();
      if (data.success) {
        this.toast(data.message, 'success');
        this.updateCartBadge();
      } else if (data.redirect) {
        this.toast('Please login to continue', 'warning');
        setTimeout(() => window.location.href = data.redirect, 1200);
      }
    } catch { this.toast('Something went wrong', 'error'); }
  },

  // ─── Wishlist Toggle ─────────────────────────────────────────────────────────
  async toggleWishlist(productId, btn) {
    try {
      const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      const res = await fetch('/User/ToggleWishlist', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `productId=${productId}&__RequestVerificationToken=${token}`
      });
      const data = await res.json();
      if (data.success) {
        this.toast(data.message, data.added ? 'gold' : 'info');
        if (btn) {
          btn.classList.toggle('active', data.added);
          btn.querySelector('i')?.classList.toggle('fa-solid', data.added);
          btn.querySelector('i')?.classList.toggle('fa-regular', !data.added);
        }
      }
    } catch { this.toast('Please login first', 'warning'); }
  },

  // ─── Search Autocomplete ─────────────────────────────────────────────────────
  initSearchAutocomplete() {
    const inputs = document.querySelectorAll('.navbar-search-input, .shop-search-input');
    inputs.forEach(input => {
      const wrapper = input.closest('.navbar-search-wrapper, .search-wrapper');
      if (!wrapper) return;

      let dropdown = wrapper.querySelector('.search-suggestions');
      if (!dropdown) {
        dropdown = document.createElement('div');
        dropdown.className = 'search-suggestions';
        wrapper.appendChild(dropdown);
        wrapper.style.position = 'relative';
      }

      let timeout;
      input.addEventListener('input', () => {
        clearTimeout(timeout);
        const q = input.value.trim();
        if (q.length < 2) { dropdown.style.display = 'none'; return; }
        timeout = setTimeout(async () => {
          try {
            const res = await fetch(`/Shop/SearchSuggestions?q=${encodeURIComponent(q)}`);
            const items = await res.json();
            if (!items.length) { dropdown.style.display = 'none'; return; }
            dropdown.innerHTML = items.map(p => `
              <a href="/Shop/Details/${p.id}" class="suggestion-item">
                <img src="${p.image || '/images/placeholder.jpg'}" alt="${p.name}" onerror="this.src='/images/placeholder.jpg'">
                <div>
                  <div class="suggestion-name">${p.name}</div>
                  <div class="suggestion-price">PKR ${Number(p.price).toLocaleString()}</div>
                </div>
              </a>`).join('');
            dropdown.style.display = 'block';
          } catch {}
        }, 280);
      });

      document.addEventListener('click', e => {
        if (!wrapper.contains(e.target)) dropdown.style.display = 'none';
      });
    });
  },

  // ─── Hero Carousel ───────────────────────────────────────────────────────────
  initHeroCarousel() {
    const slides = document.querySelectorAll('.hero-slide');
    const dots   = document.querySelectorAll('.hero-dot');
    if (!slides.length) return;

    let current = 0;
    let timer;

    const go = (index) => {
      slides[current].classList.remove('active');
      dots[current]?.classList.remove('active');
      current = (index + slides.length) % slides.length;
      slides[current].classList.add('active');
      dots[current]?.classList.add('active');
    };

    const autoplay = () => { timer = setInterval(() => go(current + 1), 5000); };

    dots.forEach((dot, i) => dot.addEventListener('click', () => { clearInterval(timer); go(i); autoplay(); }));
    autoplay();
  },

  // ─── Scroll Animations ────────────────────────────────────────────────────────
  initScrollAnimations() {
    if (!('IntersectionObserver' in window)) return;
    const observer = new IntersectionObserver(entries => {
      entries.forEach(el => {
        if (el.isIntersecting) {
          el.target.classList.add('in-view');
          observer.unobserve(el.target);
        }
      });
    }, { threshold: 0.12 });
    document.querySelectorAll('.scroll-reveal').forEach(el => observer.observe(el));
  },

  // ─── Navbar Scroll Effect ────────────────────────────────────────────────────
  initNavbarScroll() {
    const nav = document.querySelector('.velora-navbar');
    if (!nav) return;
    window.addEventListener('scroll', () => {
      nav.classList.toggle('scrolled', window.scrollY > 60);
    }, { passive: true });
  },

  // ─── Product Card Quick-Add ──────────────────────────────────────────────────
  initProductActions() {
    document.addEventListener('click', e => {
      const quickAdd = e.target.closest('[data-quick-add]');
      if (quickAdd) {
        e.preventDefault();
        const id = quickAdd.dataset.productId;
        this.addToCart(id, 1);
      }

      const wishBtn = e.target.closest('[data-wishlist]');
      if (wishBtn) {
        e.preventDefault();
        const id = wishBtn.dataset.productId;
        this.toggleWishlist(id, wishBtn);
      }
    });
  },

  // ─── Cart Quantity Inputs ────────────────────────────────────────────────────
  initQuantityInputs() {
    document.querySelectorAll('.qty-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        const input = btn.closest('.qty-selector')?.querySelector('.qty-input');
        if (!input) return;
        const val = parseInt(input.value) || 1;
        const delta = btn.dataset.delta === '+' ? 1 : -1;
        input.value = Math.max(1, val + delta);
        input.dispatchEvent(new Event('change'));
      });
    });

    // Cart page inline update
    document.querySelectorAll('.cart-qty-input').forEach(input => {
      let timeout;
      input.addEventListener('change', () => {
        clearTimeout(timeout);
        timeout = setTimeout(() => this.updateCartItem(input), 400);
      });
    });
  },

  async updateCartItem(input) {
    const cartItemId = input.dataset.cartItemId;
    const quantity   = parseInt(input.value);
    if (!cartItemId) return;
    try {
      const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      const res = await fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `cartItemId=${cartItemId}&quantity=${quantity}&__RequestVerificationToken=${token}`
      });
      const data = await res.json();
      if (data.success) {
        // Update totals
        const setEl = (sel, val) => { const el = document.querySelector(sel); if (el) el.textContent = 'PKR ' + val; };
        setEl('#subtotal-val', data.subTotal);
        setEl('#shipping-val', data.shipping);
        setEl('#tax-val',      data.tax);
        setEl('#grandtotal-val', data.grandTotal);
        this.updateCartBadge();
      }
    } catch {}
  },

  // ─── Product Image Gallery ────────────────────────────────────────────────────
  initGallery() {
    const thumbs = document.querySelectorAll('.gallery-thumb');
    const main   = document.querySelector('.gallery-main img');
    if (!main || !thumbs.length) return;
    thumbs.forEach(thumb => {
      thumb.addEventListener('click', () => {
        thumbs.forEach(t => t.classList.remove('active'));
        thumb.classList.add('active');
        main.style.opacity = '0';
        main.style.transform = 'scale(0.97)';
        setTimeout(() => {
          main.src = thumb.querySelector('img').src;
          main.style.opacity = '1';
          main.style.transform = 'scale(1)';
        }, 180);
      });
    });
    main.style.transition = 'opacity 0.2s ease, transform 0.2s ease';
  },

  // ─── Size & Color Pickers ─────────────────────────────────────────────────────
  initFilters() {
    // Size
    document.querySelectorAll('.size-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        btn.closest('.sizes-wrapper')?.querySelectorAll('.size-btn')
          .forEach(b => b.classList.remove('selected'));
        btn.classList.add('selected');
        const target = document.getElementById('selectedSize');
        if (target) target.value = btn.textContent.trim();
      });
    });

    // Color
    document.querySelectorAll('.color-swatch').forEach(swatch => {
      swatch.addEventListener('click', () => {
        swatch.closest('.colors-wrapper')?.querySelectorAll('.color-swatch')
          .forEach(s => s.classList.remove('selected'));
        swatch.classList.add('selected');
        const target = document.getElementById('selectedColor');
        if (target) target.value = swatch.dataset.color;
      });
    });

    // Payment Method
    document.querySelectorAll('.payment-method-card').forEach(card => {
      card.addEventListener('click', () => {
        document.querySelectorAll('.payment-method-card').forEach(c => c.classList.remove('selected'));
        card.classList.add('selected');
        const radio = card.querySelector('input[type="radio"]');
        if (radio) radio.checked = true;
      });
    });

    // Price range
    const rangeInput = document.getElementById('priceRange');
    const rangeLabel = document.getElementById('priceRangeLabel');
    if (rangeInput && rangeLabel) {
      rangeInput.addEventListener('input', () => {
        rangeLabel.textContent = `PKR ${Number(rangeInput.value).toLocaleString()}`;
      });
    }
  },

  // ─── Admin Sidebar Toggle ─────────────────────────────────────────────────────
  initAdminSidebar() {
    const toggle = document.getElementById('adminSidebarToggle');
    const sidebar = document.getElementById('adminSidebar');
    if (!toggle || !sidebar) return;
    toggle.addEventListener('click', () => sidebar.classList.toggle('open'));

    // Close on outside click
    document.addEventListener('click', e => {
      if (!sidebar.contains(e.target) && !toggle.contains(e.target))
        sidebar.classList.remove('open');
    });

    // Mark active link
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.admin-nav-link').forEach(link => {
      const href = link.getAttribute('href')?.toLowerCase();
      if (href && currentPath.includes(href.replace(/\/+$/, '')))
        link.classList.add('active');
    });
  },

  // ─── TempData Toasts ─────────────────────────────────────────────────────────
  initTempDataToasts() {
    const success = document.getElementById('tempSuccess');
    const error   = document.getElementById('tempError');
    const info    = document.getElementById('tempInfo');
    if (success?.value) this.toast(success.value, 'success');
    if (error?.value)   this.toast(error.value, 'error');
    if (info?.value)    this.toast(info.value, 'info');
  },

  // ─── Cart Remove ─────────────────────────────────────────────────────────────
  async removeCartItem(cartItemId, row) {
    try {
      const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      const res = await fetch('/Cart/Remove', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `cartItemId=${cartItemId}&__RequestVerificationToken=${token}`
      });
      const data = await res.json();
      if (data.success) {
        row?.remove();
        this.toast('Item removed from cart', 'info');
        this.updateCartBadge();
        if (!document.querySelectorAll('.cart-item-row').length) {
          setTimeout(() => location.reload(), 600);
        }
      }
    } catch {}
  }
};

// ─── DOM Ready ────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => Velora.init());

// ─── Admin Chart (Dashboard) ──────────────────────────────────────────────────
function initRevenueChart(labels, data) {
  const ctx = document.getElementById('revenueChart');
  if (!ctx || typeof Chart === 'undefined') return;

  new Chart(ctx, {
    type: 'line',
    data: {
      labels,
      datasets: [{
        label: 'Revenue (PKR)',
        data,
        borderColor: '#C9A84C',
        backgroundColor: 'rgba(201,168,76,0.08)',
        borderWidth: 2.5,
        fill: true,
        tension: 0.4,
        pointBackgroundColor: '#C9A84C',
        pointBorderColor: '#fff',
        pointBorderWidth: 2,
        pointRadius: 5,
        pointHoverRadius: 7
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: '#0D0D0D',
          titleColor: '#C9A84C',
          bodyColor: '#fff',
          padding: 12,
          callbacks: {
            label: ctx => `PKR ${Number(ctx.parsed.y).toLocaleString()}`
          }
        }
      },
      scales: {
        x: {
          grid: { color: 'rgba(0,0,0,0.04)' },
          ticks: { font: { size: 11 }, color: '#9E9E9E' }
        },
        y: {
          grid: { color: 'rgba(0,0,0,0.04)' },
          ticks: {
            font: { size: 11 }, color: '#9E9E9E',
            callback: v => 'PKR ' + Number(v).toLocaleString()
          }
        }
      }
    }
  });
}

// ─── Order Status Chart ───────────────────────────────────────────────────────
function initOrderStatusChart(data) {
  const ctx = document.getElementById('orderStatusChart');
  if (!ctx || typeof Chart === 'undefined') return;

  new Chart(ctx, {
    type: 'doughnut',
    data: {
      labels: ['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'],
      datasets: [{
        data,
        backgroundColor: ['#ff9800','#2196f3','#9c27b0','#4caf50','#f44336'],
        borderWidth: 0,
        hoverOffset: 6
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'bottom',
          labels: { padding: 16, font: { size: 11 }, usePointStyle: true }
        }
      },
      cutout: '68%'
    }
  });
}
