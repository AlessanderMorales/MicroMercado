// ~/js/sales.js

$(document).ready(function () {
    class SalesManager {
        constructor() {
            this.cart = [];
            this.cartTable = null;
            this.searchTimeout = null;
            this.selectedClientId = 0;
            this.selectedClientName = "";
            this.selectedClientTaxDocument = "";
            this.isProcessingSale = false; 

            this.init();
        }

        init() {
            this.initializeAutocomplete();
            this.initializeDataTable();
            this.bindEvents();
        }

        // Buscar productos en el backend
        async searchProducts(term) {
            const response = await fetch(`/Sales?handler=SearchProducts&term=${encodeURIComponent(term)}`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return await response.json();
        }

        initializeAutocomplete() {
            const self = this; 

            $('#product_id').autocomplete({
                minLength: 2,
                delay: 300,
                source: function (request, response) {
                    clearTimeout(self.searchTimeout);

                    self.searchTimeout = setTimeout(async () => {
                        try {
                            const result = await self.searchProducts(request.term);

                            if (result.success) {
                                const items = result.data.map(product => ({
                                    label: `${product.name} - ${product.brand} (Stock: ${product.stock})`,
                                    value: product.name, 
                                    product: product 
                                }));
                                response(items);
                            } else {
                                response([]);
                                self.showNotification('error', 'Error al buscar productos');
                            }
                        } catch (error) {
                            console.error('Error en búsqueda de productos:', error);
                            response([]);
                            self.showNotification('error', 'Error de conexión al buscar productos');
                        }
                    }, 300);
                },
                select: function (event, ui) {
                    event.preventDefault(); 
                    self.addProductToCart(ui.item.product); 
                    $('#product_id').val(''); 
                    return false;
                },
                focus: function (event, ui) {
                    event.preventDefault();
                    $('#product_id').val(ui.item.product.name);
                    return false;
                }
            }).autocomplete("instance")._renderItem = function (ul, item) {
                const stockClass = item.product.stock > 0 ? 'text-success' : 'text-danger';

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

        initializeDataTable() {
            this.cartTable = $('#lstProductosVenta').DataTable({
                data: [],
                columns: [
                    { data: 'productCode' },
                    { data: 'productName' },
                    { data: 'categoryName' },
                    {
                        data: 'quantity',
                        render: function (data, type, row) {
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
                        render: function (data) {
                            return `Bs. ${data.toFixed(2)}`;
                        }
                    },
                    {
                        data: null,
                        render: function (data, type, row) {
                            return `Bs. ${row.total.toFixed(2)}`;
                        }
                    },
                    {
                        data: null,
                        render: function (data, type, row) {
                            return `<button class="btn btn-danger btn-sm remove-product" 
                                    data-product-id="${row.productId}">
                                    <i class="fas fa-trash"></i>
                                </button>`;
                        },
                        className: 'text-center'
                    }
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
                drawCallback: function () {
                    if (this.api().data().count() === 0) {
                        if ($('#lstProductosVenta tbody .empty-cart-message').length === 0) {
                            $(this).find('tbody').html(`
                                <tr class="empty-cart-message">
                                    <td colspan="7" class="text-center text-muted py-4">
                                        <i class="fas fa-shopping-basket fa-3x mb-3 d-block"></i>
                                        <p class="mb-0">No hay productos para venta</p>
                                        <small>Busque y agregue productos usando el buscador de arriba</small>
                                    </td>
                                </tr>
                            `);
                        }
                    } else {
                        $('#lstProductosVenta tbody .empty-cart-message').remove();
                    }
                }
            });
            this.cartTable.draw();
        }
        updateClientInfo(client) {
            if (client) {
                this.selectedClientId = client.id;
                this.selectedClientName = client.businessName;;
                this.selectedClientTaxDocument = client.taxDocument;

                $('#nombreCliente').val(this.selectedClientName);
                $('#idDocumentoRecibido').val(this.selectedClientTaxDocument);
            } else {
                this.selectedClientId = 0;
                this.selectedClientName = "";
                this.selectedClientTaxDocument = "";
                $('#nombreCliente').val('');
                $('#idDocumentoRecibido').val('');
            }
        }

        async searchClientByTaxDocument() {
            const taxDocument = $('#idDocumentoRecibido').val().trim();

            if (taxDocument === "") {
                this.showNotification('warning', 'Por favor, ingrese el NIT/CI del cliente.');
                return;
            }

            const $btn = $('#btnBuscarCliente');
            const originalBtnHtml = $btn.html(); 
            $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Buscando...');

            try {
                const response = await $.ajax({
                    url: '?handler=SearchClientByTaxDocument', 
                    type: 'POST',
                    data: { ClientTaxDocument: taxDocument }, 
                    headers: {
                        RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val()
                    }
                });

                if (response.success) {
                    this.updateClientInfo(response.client); 
                    //this.showNotification('success', 'Cliente encontrado exitosamente.');
                } else {
                    this.updateClientInfo(null); 
                    this.showNotification('info', response.message || 'Cliente no encontrado.');
                }
            } catch (error) {
                this.updateClientInfo(null);
                console.error('Error al buscar cliente:', error);
                this.showNotification('error', 'Ocurrió un error al buscar el cliente. Intente nuevamente.');
            } finally {
                $btn.prop('disabled', false).html(originalBtnHtml); 
            }
        }
        addProductToCart(product) {
            if (product.stock <= 0) {
                this.showNotification('warning', 'Producto sin stock disponible');
                return;
            }

            const existingIndex = this.cart.findIndex(item => item.productId === product.id);

            if (existingIndex !== -1) {
                const newQuantity = this.cart[existingIndex].quantity + 1;

                if (newQuantity > product.stock) {
                    this.showNotification('warning', `Stock insuficiente. Disponible: ${product.stock}`);
                    return;
                }

                this.cart[existingIndex].quantity = newQuantity;
                this.cart[existingIndex].total = newQuantity * this.cart[existingIndex].price;
            } else {
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

        updateCartDisplay() {
            this.cartTable.clear();
            if (this.cart.length > 0) {
                this.cartTable.rows.add(this.cart);
            }
            this.cartTable.draw();
            this.updateTotals();
        }

        updateTotals() {
            const total = this.cart.reduce((sum, item) => sum + item.total, 0);

            $('#totalVenta').text(total.toFixed(2));
            $('#totalVentaRegistrar').text(total.toFixed(2));
            $('#boleta_total').text(total.toFixed(2));

            this.updateChange();
        }

        updateChange() {
            const totalText = $('#boleta_total').text().trim();
            const total = parseFloat(totalText) || 0;
            const efectivoRecibido = parseFloat($('#iptEfectivoRecibido').val()) || 0;

            const vuelto = efectivoRecibido - total;

            $('#EfectivoEntregado').text(efectivoRecibido.toFixed(2));
            $('#Vuelto').text(vuelto >= 0 ? vuelto.toFixed(2) : '0.00');
        }

        removeProduct(productId) {
            const index = this.cart.findIndex(item => item.productId === productId);

            if (index !== -1) {
                const productName = this.cart[index].productName;
                this.cart.splice(index, 1);
                this.updateCartDisplay();
                this.showNotification('info', `${productName} eliminado de la venta`);
            }
        }

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

        clearCart() {
            if (this.cart.length === 0) {
                this.showNotification('info', 'La lista está vacía');
                return;
            }
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: '¿Está seguro?',
                    text: "¿Desea vaciar la lista de productos?",
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Sí, vaciar!',
                    cancelButtonText: 'No'
                }).then((result) => {
                    if (result.isConfirmed) {
                        this.cart = [];
                        this.updateCartDisplay();
                        $('#product_id').val('');
                        $('#iptEfectivoRecibido').val('');
                        $('#chkEfectivoExacto').prop('checked', false);
                        this.showNotification('info', 'Lista vaciada');
                    }
                });
            } else {
                const confirmHtml = `
                    <div class="modal fade" id="confirmClearModal" tabindex="-1">
                        <div class="modal-dialog">
                            <div class="modal-content">
                                <div class="modal-header bg-warning">
                                    <h5 class="modal-title"><i class="fas fa-exclamation-triangle"></i> Confirmar</h5>
                                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                                </div>
                                <div class="modal-body">
                                    ¿Está seguro de vaciar el detalle de venta?
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                                    <button type="button" class="btn btn-danger" id="confirmClearBtn">Sí, vaciar</button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
                
                $('body').append(confirmHtml);
                const modal = new bootstrap.Modal(document.getElementById('confirmClearModal'));
                
                $('#confirmClearBtn').on('click', () => {
                    this.cart = [];
                    this.updateCartDisplay();
                    $('#product_id').val('');
                    $('#iptEfectivoRecibido').val('');
                    $('#chkEfectivoExacto').prop('checked', false);
                    this.showNotification('info', 'Lista vaciada');
                    modal.hide();
                    $('#confirmClearModal').remove();
                });
                
                modal.show();
                
                $('#confirmClearModal').on('hidden.bs.modal', function () {
                    $(this).remove();
                });
            }
        }

        async confirmSale() {
            console.log('Método confirmSale ejecutándose...');

            
            if (this.isProcessingSale) {
                console.warn('Ya hay una venta en proceso. Ignorando click adicional.');
                this.showNotification('warning', 'Ya hay una venta en proceso, por favor espere...');
                return;
            }
            
            if (this.cart.length === 0) {
                this.showNotification('warning', 'No hay productos en la lista de venta');
                return;
            }
            if (this.selectedClientId === 0) {
                this.showNotification('warning', 'Debe seleccionar un cliente antes de confirmar la venta.');
                return;
            }

            const total = this.cart.reduce((sum, item) => sum + item.total, 0);
            const tipoPago = parseInt($('#selTipoPago').val());

            if (tipoPago === 0) {
                this.showNotification('warning', 'Debe seleccionar un tipo de pago');
                return;
            }

            let efectivoRecibido = 0;
            if (tipoPago === 1) {
                const efectivoInput = $('#iptEfectivoRecibido').val();
                efectivoRecibido = efectivoInput ? parseFloat(efectivoInput) : 0;

                if (efectivoRecibido < total) {
                    this.showNotification('warning', 'El efectivo recibido es menor al total de la venta');
                    return;
                }
            } else {
                efectivoRecibido = total;
            }

           const shouldContinue = await new Promise((resolve) => {
                const confirmHtml = `
                    <div class="modal fade" id="confirmSaleModal" tabindex="-1">
                        <div class="modal-dialog">
                            <div class="modal-content">
                                <div class="modal-header bg-primary text-white">
                                    <h5 class="modal-title"><i class="fas fa-check-circle"></i> Confirmar Venta</h5>
                                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                                </div>
                                <div class="modal-body">
                                    <p class="mb-0">¿Confirmar venta por <strong class="text-success">Bs. ${total.toFixed(2)}</strong>?</p>
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                                    <button type="button" class="btn btn-primary" id="confirmSaleBtn">Sí, confirmar</button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
                
                $('body').append(confirmHtml);
                const modal = new bootstrap.Modal(document.getElementById('confirmSaleModal'));
                
                $('#confirmSaleBtn').on('click', () => {
                    modal.hide();
                    resolve(true);
                });
                
                $('#confirmSaleModal').on('hidden.bs.modal', function () {
                    $(this).remove();
                    if (!$('#confirmSaleBtn').data('clicked')) {
                        resolve(false);
                    }
                });
                
                $('#confirmSaleBtn').on('click', function() {
                    $(this).data('clicked', true);
                });
                
                modal.show();
            });
            
            if (!shouldContinue) {
                return;
            }

            this.isProcessingSale = true;

            try {
                
                const $btnConfirmar = $('#btnIniciarVenta');
                const $btnVaciar = $('#btnVaciarListado');
                
                $('#btnIniciarVenta').prop('disabled', true)
                    .html('<i class="fas fa-spinner fa-spin"></i> Procesando...');

                $btnVaciar.prop('disabled', true);
                
                const vuelto = efectivoRecibido - total;
                const saleData = {
                    clientId: this.selectedClientId,
                    paymentType: tipoPago,
                    totalAmount: parseFloat(total.toFixed(2)),
                    cashReceived: parseFloat(efectivoRecibido.toFixed(2)),
                    change: parseFloat((vuelto >= 0 ? vuelto : 0).toFixed(2)),
                    items: this.cart.map(item => ({
                        productId: item.productId,
                        quantity: item.quantity,
                        price: parseFloat(item.price.toFixed(2)),
                        total: parseFloat(item.total.toFixed(2))
                    }))
                };

                console.log('Datos de venta a enviar:', saleData);

                const response = await fetch('/Sales?handler=ConfirmSale', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': $('input:hidden[name="__RequestVerificationToken"]').val()
                    },
                    body: JSON.stringify(saleData)
                });

                if (!response.ok) {
                    const errorText = await response.text();
                    console.error('Error del servidor:', response.status, errorText);
                    throw new Error(`Error del servidor: ${response.status}`);
                }

                const result = await response.json();
                console.log('Respuesta del servidor:', result);

                if (result.success) {
                    const saleResponse = {
                        saleId: result.data.saleId, 
                        saleDate: result.data.saleDate,
                        totalAmount: result.data.totalAmount,
                        cashReceived: result.data.cashReceived,
                        change: result.data.change,
                        itemsCount: result.data.itemsCount
                    };
                    this.showNotification('success', `¡Venta #${saleResponse.saleId} registrada exitosamente!`);
                    this.cart = [];
                    this.updateCartDisplay();
                    this.selectedClientId = 0;
                    this.selectedClientName = "";
                    this.selectedClientTaxDocument = "";
                    $('#idDocumentoRecibido').val('');
                    $('#nombreCliente').val('');
                    $('#iptEfectivoRecibido').val('');
                    $('#chkEfectivoExacto').prop('checked', false);
                    $('#selTipoPago').val('1');
                    $('#product_id').val('');

                    this.showSaleSummary(saleResponse);
                } else {
                    this.showNotification('error', result.message || 'Error al procesar la venta');
                    if (result.errors && result.errors.length > 0) {
                        console.error('Errores:', result.errors);
                    }
                }

            } catch (error) {
                console.error('❌ Excepción en la venta:', error);
                this.showNotification('error', 'Error al procesar la venta: ' + error.message);
            } finally {
                this.isProcessingSale = false;
        
                const $btnConfirmar = $('#btnIniciarVenta');
                const $btnVaciar = $('#btnVaciarListado');
                
                $btnConfirmar.prop('disabled', false)
                    .html('<i class="fas fa-check-circle"></i> Confirmar Venta');
                
                $btnVaciar.prop('disabled', false);
            }
        }

        showSaleSummary(saleData) {
            if (!saleData) {
                console.error('saleData es undefined');
                return;
            }
            const fechaFormateada = new Date(saleData.saleDate).toLocaleString('es-BO', {
                year: 'numeric',
                month: '2-digit',
                day: '2-digit',
                hour: '2-digit',
                minute: '2-digit'
            });

            const summary = `
                <div style="text-align: left;">
                    <p><strong>Número de Venta:</strong> #${saleData.saleId}</p>
                    <p><strong>Fecha:</strong> ${fechaFormateada}</p>
                    <p><strong>Total:</strong> Bs. ${saleData.totalAmount.toFixed(2)}</p>
                    <p><strong>Efectivo Recibido:</strong> Bs. ${saleData.cashReceived.toFixed(2)}</p>
                    <p><strong>Vuelto:</strong> Bs. ${saleData.change.toFixed(2)}</p>
                    <p><strong>Productos:</strong> ${saleData.itemsCount} items</p>
                </div>
            `;

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'success',
                    title: '¡Venta Confirmada!',
                    html: summary,
                    confirmButtonText: 'Aceptar'
                });
            } else {
                const modalHtml = `
                    <div class="modal fade" id="saleSummaryModal" tabindex="-1">
                        <div class="modal-dialog">
                            <div class="modal-content">
                                <div class="modal-header bg-success text-white">
                                    <h5 class="modal-title">
                                        <i class="fas fa-check-circle"></i> ¡Venta Confirmada!
                                    </h5>
                                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                                </div>
                                <div class="modal-body">
                                    <div class="alert alert-success mb-3">
                                        <strong>Venta #${saleData.saleId}</strong> registrada exitosamente
                                    </div>
                                    <table class="table table-sm">
                                        <tbody>
                                            <tr>
                                                <td><strong>Fecha:</strong></td>
                                                <td>${fechaFormateada}</td>
                                            </tr>
                                            <tr>
                                                <td><strong>Total:</strong></td>
                                                <td class="text-success"><strong>Bs. ${saleData.totalAmount.toFixed(2)}</strong></td>
                                            </tr>
                                            <tr>
                                                <td><strong>Efectivo Recibido:</strong></td>
                                                <td>Bs. ${saleData.cashReceived.toFixed(2)}</td>
                                            </tr>
                                            <tr>
                                                <td><strong>Vuelto:</strong></td>
                                                <td class="text-danger">Bs. ${saleData.change.toFixed(2)}</td>
                                            </tr>
                                            <tr>
                                                <td><strong>Productos:</strong></td>
                                                <td>${saleData.itemsCount} items</td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn btn-primary" data-bs-dismiss="modal">Aceptar</button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
                
                $('body').append(modalHtml);
                const modal = new bootstrap.Modal(document.getElementById('saleSummaryModal'));
                modal.show();
                
                $('#saleSummaryModal').on('hidden.bs.modal', function () {
                    $(this).remove();
                });
            }
        }

        showNotification(type, message) {
            if (typeof toastr !== 'undefined') {
                toastr.options = {
                    "positionClass": "toast-top-right",
                    "timeOut": "3000"
                };
                toastr[type](message);
            } else if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: type,
                    title: (type === 'error' ? 'Error' : (type === 'warning' ? 'Advertencia' : (type === 'success' ? 'Éxito' : 'Información'))),
                    text: message,
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 3000,
                    timerProgressBar: true
                });
            } else {
  
                const icons = {
                    success: 'fa-check-circle',
                    error: 'fa-exclamation-triangle',
                    warning: 'fa-exclamation-circle',
                    info: 'fa-info-circle'
                };
                
                const colors = {
                    success: 'alert-success',
                    error: 'alert-danger',
                    warning: 'alert-warning',
                    info: 'alert-info'
                };
        
                const alertHtml = `
                    <div class="alert ${colors[type]} alert-dismissible fade show" role="alert">
                        <i class="fas ${icons[type]} me-2"></i>
                        ${message}
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                    </div>
                `;
        
                const container = $('#notification-container');
                container.append(alertHtml);
                
                setTimeout(() => {
                    container.find('.alert').first().fadeOut(300, function() {
                        $(this).remove();
                    });
                }, 5000);
            }
        }

        bindEvents() {
            const self = this;
            $('#lstProductosVenta').on('click', '.remove-product', function () { 
                const productId = parseInt($(this).data('product-id'));
                self.removeProduct(productId);
            });

            $('#lstProductosVenta').on('change', '.quantity-input', function () { 
                const productId = parseInt($(this).data('product-id'));
                const newQuantity = parseInt($(this).val());
                self.updateQuantity(productId, newQuantity);
            });

            $('#btnVaciarListado').on('click', () => self.clearCart());

            $('#iptEfectivoRecibido').on('input', () => self.updateChange()); 

            $('#chkEfectivoExacto').on('change', function (e) { 
                if (e.target.checked) {
                    const total = parseFloat($('#totalVentaRegistrar').text()); 
                    $('#iptEfectivoRecibido').val(total.toFixed(2));
                    self.updateChange();
                }
            });

            $('#product_id').on('keypress', function (e) { 
                if (e.which === 13) {
                    e.preventDefault();
                }
            });
            $('#btnBuscarCliente').on('click', () => self.searchClientByTaxDocument());
            $('#btnIniciarVenta').on('click', () => {
                self.confirmSale();
            });
        }
    }

    console.log('Inicializando SalesManager...');
    window.salesManager = new SalesManager();
    console.log('SalesManager inicializado');
});