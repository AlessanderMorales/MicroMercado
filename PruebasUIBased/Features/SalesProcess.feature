Feature: Proceso de Ventas - UI Testing con Pairwise
Como usuario del sistema
Quiero realizar ventas completas
Para gestionar transacciones con clientes

Background:
  Given que la aplicacion esta en ejecucion
  And navego a la pagina de ventas


@ui @sales @pairwise @pw-sales-01
Scenario: PW-SALES-01 - Venta con un producto y pago exacto en efectivo
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Yogurt" con cantidad 1 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "10.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente
  And debo ver un mensaje de exito

@ui @sales @pairwise @pw-sales-02
Scenario: PW-SALES-02 - Venta con un producto y efectivo mayor (con cambio)
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Yogurt" con cantidad 2 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "50.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente
  And el carrito debe estar vacio

@ui @sales @pairwise @pw-sales-03
Scenario: PW-SALES-03 - Venta con dos productos diferentes
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Yogurt Bebible Sabor Durazno Pil 1000 Gr" con cantidad 2 al carrito
  And agrego el producto "Leche de Soya sabor Banana Soy 946 Ml" con cantidad 1 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "50.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente

@ui @sales @pairwise @pw-sales-04
Scenario: PW-SALES-04 - Venta con multiples productos (3+)
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Antitranspirante en Barra Speed Stick 50 Gr" con cantidad 3 al carrito
  And agrego el producto "Agua Lavandina Aditiva Marina X-5 1 Lt" con cantidad 2 al carrito
  And agrego el producto "Toalla De Cocina Hogar 1 Unidad" con cantidad 1 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "200.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente
  And el carrito debe estar vacio

@ui @sales @pairwise @pw-sales-05
Scenario: PW-SALES-05 - Venta con productos de diferentes categorias
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Pasta al Huevo Tagliatelle Anita 400 Gr" con cantidad 2 al carrito
  And agrego el producto "Endulzante con Stevia Equal 50 Unds." con cantidad 3 al carrito
  And agrego el producto "Arroz Familiar Caisy 1 Kg" con cantidad 1 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "200.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente
  And debo ver un mensaje de exito

@ui @sales @pairwise @pw-sales-06
Scenario: PW-SALES-06 - Venta con cantidad alta de un solo producto
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Fideo Codo Rayado Don Vittorio 400 Gr" con cantidad 10 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "500.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente

@ui @sales @pairwise @pw-sales-07
Scenario: PW-SALES-07 - Venta con monto exacto sin cambio
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Pan casero marraqueta" con cantidad 1 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "10.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente

@ui @sales @pairwise @pw-sales-08
Scenario: PW-SALES-08 - Venta con productos lacteos multiples
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Mix de Frutos Secos Varios Maya 260 Gr" con cantidad 2 al carrito
  And agrego el producto "Pipoca sabor Mantequilla Act II 91 Gr" con cantidad 2 al carrito
  And agrego el producto "Sopa de Pollo Maruchan 85 Gr" con cantidad 1 al carrito
  And agrego el producto "Ketchup Original Doypack Kris 1000 Gr" con cantidad 1 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "300.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente
  And el carrito debe estar vacio

@ui @sales @pairwise @pw-sales-09
Scenario: PW-SALES-09 - Venta con cambio grande
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Llajua Churrasquera B&R 220 Gr" con cantidad 1 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "500.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente

@ui @sales @pairwise @pw-sales-10
Scenario: PW-SALES-10 - Venta con 5 productos diferentes
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Endulzante con Stevia Equal 50 Unds." con cantidad 1 al carrito
  And agrego el producto "Arroz Superior" con cantidad 1 al carrito
  And agrego el producto "Palo Trapeador Movica Unidad" con cantidad 1 al carrito
  And agrego el producto "Recogedor de Basura con Mango Movica Unidad" con cantidad 2 al carrito
  And agrego el producto "Guante Naranja T7 1/2 Master Unidad" con cantidad 1 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "350.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente
  And el carrito debe estar vacio

@ui @sales @pe @pe-sales-01
Scenario: PE-SALES-01 - Venta simple minima (1 producto cantidad 1)
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Yogurt Bebible Sabor Durazno Pil 1000 Gr" con cantidad 1 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "20.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente
  And el carrito debe estar vacio

@ui @sales @pe @pe-sales-02
Scenario: PE-SALES-02 - Venta compleja con multiples productos y cantidades variadas
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Yogurt Bebible Sabor Durazno Pil 1000 Gr" con cantidad 5 al carrito
  And agrego el producto "Leche de Soya sabor Banana Soy 946 Ml" con cantidad 3 al carrito
  And agrego el producto "Mantequilla con Sal Pil 900 Gr" con cantidad 2 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "500.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente
  And debo ver un mensaje de exito
