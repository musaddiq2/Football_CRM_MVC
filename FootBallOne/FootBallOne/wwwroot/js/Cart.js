// cart.js - Place in wwwroot/js/cart.js

// Add to Cart functionality
function addToCart(productId, variantId = null, quantity = 1) {
    // Show loading state
    const button = event.target;
    const originalText = button.innerHTML;
    button.disabled = true;
    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Adding...';

    const data = {
        productId: productId,
        variantId: variantId,
        quantity: quantity
    };

    fetch('/Cart/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        },
        body: JSON.stringify(data)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update cart count
            updateCartCount(data.cartItemCount);
            
            // Show success message
            showNotification('success', data.message);
            
            // Reset button
            button.disabled = false;
            button.innerHTML = '<i class="fas fa-check"></i> Added!';
            
            setTimeout(() => {
                button.innerHTML = originalText;
            }, 2000);
        } else {
            // Show error message
            showNotification('error', data.message);
            button.disabled = false;
            button.innerHTML = originalText;
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('error', 'An error occurred. Please try again.');
        button.disabled = false;
        button.innerHTML = originalText;
    });
}

// Update Cart Quantity
function updateCartQuantity(cartItemId, quantity) {
    const data = {
        cartItemId: cartItemId,
        quantity: quantity
    };

    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Reload page to show updated cart
            location.reload();
        } else {
            showNotification('error', data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('error', 'An error occurred.');
    });
}

// Update cart count in header
function updateCartCount(count) {
    const cartCountElements = document.querySelectorAll('.cart-count, .cart-badge');
    cartCountElements.forEach(element => {
        element.textContent = count;
        if (count > 0) {
            element.style.display = 'inline-block';
        }
    });
}

// Load cart count on page load
document.addEventListener('DOMContentLoaded', function() {
    fetch('/Cart/GetCartCount')
        .then(response => response.json())
        .then(data => {
            updateCartCount(data.count);
        })
        .catch(error => console.error('Error loading cart count:', error));
});

// Notification System
function showNotification(type, message) {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.notification-toast');
    existingNotifications.forEach(n => n.remove());

    // Create notification
    const notification = document.createElement('div');
    notification.className = `notification-toast notification-${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>
            <span>${message}</span>
        </div>
    `;

    // Add to page
    document.body.appendChild(notification);

    // Show notification
    setTimeout(() => {
        notification.classList.add('show');
    }, 100);

    // Hide and remove after 3 seconds
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => {
            notification.remove();
        }, 300);
    }, 3000);
}

// Quantity increase/decrease buttons
document.addEventListener('DOMContentLoaded', function() {
    // Quantity buttons in cart
    const quantityButtons = document.querySelectorAll('.quantity-btn');
    quantityButtons.forEach(button => {
        button.addEventListener('click', function() {
            const input = this.parentElement.querySelector('.quantity-input');
            const cartItemId = input.dataset.cartItemId;
            let currentValue = parseInt(input.value);
            
            if (this.classList.contains('quantity-increase')) {
                currentValue++;
            } else if (this.classList.contains('quantity-decrease') && currentValue > 1) {
                currentValue--;
            }
            
            input.value = currentValue;
            
            if (cartItemId) {
                updateCartQuantity(cartItemId, currentValue);
            }
        });
    });

    // Quantity input change
    const quantityInputs = document.querySelectorAll('.quantity-input');
    quantityInputs.forEach(input => {
        input.addEventListener('change', function() {
            const cartItemId = this.dataset.cartItemId;
            const quantity = parseInt(this.value);
            
            if (quantity < 1) {
                this.value = 1;
                return;
            }
            
            if (cartItemId) {
                updateCartQuantity(cartItemId, quantity);
            }
        });
    });
});

// Product Details - Variant Selection
function selectVariant(variantId, variantPrice) {
    // Update selected variant
    document.getElementById('selectedVariantId').value = variantId;
    
    // Update price display
    const priceElement = document.getElementById('productPrice');
    if (priceElement) {
        priceElement.textContent = '₹' + variantPrice.toFixed(2);
    }
    
    // Update active state of variant buttons
    const variantButtons = document.querySelectorAll('.variant-btn');
    variantButtons.forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.variantId == variantId) {
            btn.classList.add('active');
        }
    });
}