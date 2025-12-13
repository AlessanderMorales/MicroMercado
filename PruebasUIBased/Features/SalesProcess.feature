Feature: Proceso Principal de Ventas - UI Testing
Como usuario del sistema
Quiero realizar ventas completas
Para gestionar transacciones con clientes

Background:
  Given que la aplicacion esta en ejecucion
  And navego a la pagina de ventas

@ui @sales @happy
Scenario: VP-01 - Venta con un solo producto en efectivo
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Yogurt" con cantidad 2 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "50.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente
  And debo ver un mensaje de exito

  @ui @sales @complex
  Scenario: VP-02 - Venta con multiples productos en efectivo
    Given que existe un cliente con documento "9404688"
    When agrego el producto "Yogurt Bebible Sabor Durazno Pil 1000 Gr" con cantidad 3 al carrito
    And agrego el producto "Leche de Soya sabor Banana Soy 946 Ml" con cantidad 2 al carrito
    And agrego el producto "Mantequilla sin Sal Pil 200 Gr" con cantidad 1 al carrito
    And busco el cliente con documento "9404688"
    And selecciono tipo de pago "1"
    And ingreso efectivo recibido "200.00" 
    And confirmo la venta
    
    Then la venta debe procesarse exitosamente
    And el carrito debe estar vacio

@ui @sales @happy
Scenario: VP-03 - Venta con productos de diferentes categorias
  Given que existe un cliente con documento "9404688"
  When agrego el producto "Yogurt" con cantidad 2 al carrito
  And agrego el producto "Manzana" con cantidad 3 al carrito
  And agrego el producto "Pasta Dental" con cantidad 1 al carrito
  And busco el cliente con documento "9404688"
  And selecciono tipo de pago "1"
  And ingreso efectivo recibido "200.00"
  And confirmo la venta
  Then la venta debe procesarse exitosamente
  And debo ver un mensaje de exito
