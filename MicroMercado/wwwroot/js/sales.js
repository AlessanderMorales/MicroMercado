class SalesManager {
    constructor() {
        this.cart = [];
        this.cartTable = null;
        this.searchTimeout = null;
        this.init();
    }

    init() {
        this.initializeAutocomplete();
        this.initializeDataTable();
        this.bindEvents();
    }

    // Inicializar autocomplete con jQuery UI
    initializeAutocomplete() {
        $('#product_id').autocomplete({
            minLength: 2,
            delay: 300,
            source: (request, response) => {
                clearTimeout(this.searchTimeout);
                
                this.searchTimeout = setTimeout(async () => {
                    try {
                        const result = await this.searchProducts(request.term);
                        
                        if (result.success) {
                            const items = result.data.map(product => ({
                                label: `${product.name} - ${product.brand} (Stock: ${product.stock})`,
                                value: product.name,
                                product: product
                            }));
                            response(items);
                        } else {
                            response([]);
                            this.showNotification('error', 'Error al buscar productos');
                        }
                    } catch (error) {
                        console.error('Error en búsqueda:', error);
                        response([]);
                        this.showNotification('error', 'Error de conexión');
                    }
                }, 300);
            },
            select: (event, ui) => {
                event.preventDefault();
                this.addProductToCart(ui.item.product);
                $('#product_id').val('');
                return false;
            },
            focus: (event, ui) => {
                event.preventDefault();
                $('#product_id').val(ui.item.product.name);
                return false;
            }
        }).autocomplete("instance")._renderItem = function(ul, item) {
            const stockClass = item.product.stock > 0 ? 'text-success' : 'text-danger';
            const stockText = item.product.stock > 0 ? 'Disponible' : 'Sin stock';
            
            return $("<li>")
                .append(`<div class="autocomplete-item">
                    <div class="fw-bold">${item.product.name}</div>
                    <div class="small text-muted">
                        ${item.product.brand} | ${item.product.categoryName} | 
                        <span class="${stockClass}">Stock: ${item.product.stock}</span> | 
                        Bs. ${item.product.price.toFixed(2)}
                    </div>
                </div>`)
                .appendTo(ul);
        };
    }

    // Buscar productos en el backend
    async searchProducts(term) {
        const response = await fetch(`/Sales?handler=SearchProducts&term=${encodeURIComponent(term)}`);
        return await response.json();
    }

    // Inicializar DataTable
    initializeDataTable() {
        this.cartTable = $('#lstProductosVenta').DataTable({
            data: [],
            columns: [
                { data: 'productCode' },
                { data: 'productName' },
                { data: 'categoryName' },
                { 
                    data: 'quantity',
                    render: (data, type, row) => {
                        return `<input type="number" 
                                class="form-control form-control-sm quantity-input" 
                                value="${data}" 
                                min="1" 
                                max="${row.stock}" 
                                data-product-id="${row.productId}">`;
                    }
                },
                { 
                    data: 'price',
                    render: (data) => `Bs. ${data.toFixed(2)}`
                },
                { 
                    data: null,
                    render: (data, type, row) => {
                        return `Bs. ${row.total.toFixed(2)}`;
                    }
                },
                { 
                    data: null,
                    render: (data, type, row) => {
                        return `<button class="btn btn-danger btn-sm remove-product" 
                                data-product-id="${row.productId}">
                                <i class="fas fa-trash"></i>
                            </button>`;
                    },
                    className: 'text-center'
                },
                { data: 'appliesWeight', visible: false }
            ],
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json',
                emptyTable: '',
                zeroRecords: ''
            },
            paging: false,
            searching: false,
            info: false,
            order: [],
            drawCallback: function() {
            if (this.api().data().count() === 0) {
                $(this).find('tbody').html(`
                    <tr class="empty-cart-message">
                        <td colspan="7" class="text-center text-muted py-4">
                            <i class="fas fa-shopping-basket fa-3x mb-3 d-block"></i>
                            <p class="mb-0">No hay productos</p>
                            <small>Busque y agregue productos usando el buscador de arriba</small>
                        </td>
                    </tr>
                `);
            }
        }
        });
        this.cartTable.draw();
    }

    // Agregar producto al carrito
    async addProductToCart(product) {
        // Verificar stock
        if (product.stock <= 0) {
            this.showNotification('warning', 'Producto sin stock disponible');
            return;
        }

        // Verificar si el producto ya está en el carrito
        const existingIndex = this.cart.findIndex(item => item.productId === product.id);

        if (existingIndex !== -1) {
            // Incrementar cantidad
            const newQuantity = this.cart[existingIndex].quantity + 1;
            
            if (newQuantity > product.stock) {
                this.showNotification('warning', `Stock insuficiente. Disponible: ${product.stock}`);
                return;
            }

            this.cart[existingIndex].quantity = newQuantity;
            this.cart[existingIndex].total = newQuantity * this.cart[existingIndex].price;
        } else {
            // Agregar nuevo producto
            this.cart.push({
                productId: product.id,
                productCode: product.id.toString().padStart(5, '0'),
                productName: product.name,
                categoryName: product.categoryName,
                quantity: 1,
                price: product.price,
                total: product.price,
                stock: product.stock,
                appliesWeight: false
            });
        }

        this.updateCartDisplay();
        this.showNotification('success', `${product.name} agregado a la venta`);
    }

    // Actualizar visualización del carrito
    updateCartDisplay() {
        this.cartTable.clear();
        if (this.cart.length === 0) {
        this.cartTable.draw();
        $('#lstProductosVenta tbody').html(`
            <tr class="empty-cart-message">
                <td colspan="7" class="text-center text-muted py-4">
                    <i class="fas fa-shopping-basket fa-3x mb-3 d-block"></i>
                    <p class="mb-0">No hay productos</p>
                    <small>Busque y agregue productos usando el buscador de arriba</small>
                </td>
            </tr>
        `);
    } else {
        // Mostrar productos
        this.cartTable.rows.add(this.cart);
        this.cartTable.draw();
    }
    
        this.updateTotals();
    }

    // Actualizar totales
    updateTotals() {
        const total = this.cart.reduce((sum, item) => sum + item.total, 0);
        
        $('#totalVenta').text(total.toFixed(2));
        $('#totalVentaRegistrar').text(total.toFixed(2));
        $('#subtotal').text(total.toFixed(2));
        $('#boleta_total').text(total.toFixed(2));
        
        this.updateChange();
    }

    // Actualizar vuelto
    updateChange() {
        const total = parseFloat($('#totalVentaRegistrar').text());
        const efectivoRecibido = parseFloat($('#iptEfectivoRecibido').val()) || 0;
        
        const vuelto = efectivoRecibido - total;
        
        $('#EfectivoEntregado').text(efectivoRecibido.toFixed(2));
        $('#Vuelto').text(vuelto >= 0 ? vuelto.toFixed(2) : '0.00');
    }

    // Remover producto del carrito
    removeProduct(productId) {
        const index = this.cart.findIndex(item => item.productId === productId);
        
        if (index !== -1) {
            const productName = this.cart[index].productName;
            this.cart.splice(index, 1);
            this.updateCartDisplay();
            this.showNotification('info', `${productName} eliminado de la lista`);
        }
    }

    // Actualizar cantidad de producto
    updateQuantity(productId, newQuantity) {
        const index = this.cart.findIndex(item => item.productId === productId);
        
        if (index !== -1) {
            if (newQuantity <= 0) {
                this.removeProduct(productId);
                return;
            }

            if (newQuantity > this.cart[index].stock) {
                this.showNotification('warning', `Stock insuficiente. Disponible: ${this.cart[index].stock}`);
                this.updateCartDisplay();
                return;
            }

            this.cart[index].quantity = newQuantity;
            this.cart[index].total = newQuantity * this.cart[index].price;
            this.updateCartDisplay();
        }
    }

    // Vaciar carrito
    clearCart() {
        if (this.cart.length === 0) {
            this.showNotification('info', 'El detalle de venta está vacío');
            return;
        }

        if (confirm('¿Está seguro de vaciar la lista?')) {
            this.cart = [];
            this.updateCartDisplay();
            $('#product_id').val('');
            $('#iptEfectivoRecibido').val('');
            this.showNotification('info', 'Lista vaciada');
        }
    }

    // Vincular eventos
    bindEvents() {
        // Evento para remover productos
        $('#lstProductosVenta').on('click', '.remove-product', (e) => {
            const productId = parseInt($(e.currentTarget).data('product-id'));
            this.removeProduct(productId);
        });

        // Evento para actualizar cantidad
        $('#lstProductosVenta').on('change', '.quantity-input', (e) => {
            const productId = parseInt($(e.target).data('product-id'));
            const newQuantity = parseInt($(e.target).val());
            this.updateQuantity(productId, newQuantity);
        });

        // Botón vaciar carrito
        $('#btnVaciarListado').on('click', () => {
            this.clearCart();
        });

        // Efectivo recibido
        $('#iptEfectivoRecibido').on('input', () => {
            this.updateChange();
        });

        // Checkbox efectivo exacto
        $('#chkEfectivoExacto').on('change', (e) => {
            if (e.target.checked) {
                const total = parseFloat($('#totalVentaRegistrar').text());
                $('#iptEfectivoRecibido').val(total.toFixed(2));
                this.updateChange();
            }
        });

        // Búsqueda por Enter
        $('#product_id').on('keypress', (e) => {
            if (e.which === 13) {
                e.preventDefault();
            }
        });
        
        // Confirmar venta
        $('#btnIniciarVenta').on('click', () => {
            this.confirmSale();
        });
    }

    // Mostrar notificaciones
    showNotification(type, message) {
        // Usar Toast de Bootstrap o Toastr si está disponible
        if (typeof Toastr !== 'undefined') {
            Toastr[type](message);
        } else {
            // Fallback a alert
            alert(message);
        }
    }
}

// Inicializar cuando el DOM esté listo
$(document).ready(() => {
    window.salesManager = new SalesManager();
});