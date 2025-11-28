Feature: Integracion del Proceso Principal de Ventas
  Como sistema de gestion
  Quiero procesar ventas completas
  Para actualizar inventario y registrar transacciones correctamente

Background:
  Given la base de datos esta limpia
  And existe una categoria "Alimentos" con id 1
  And existen los siguientes productos:
    | Name           | Price | Stock | CategoryId |
    | Quinua Premium | 28.50 | 100   | 1          |
    | Arroz Integral | 15.00 | 80    | 1          |
    | Aceite Vegetal | 12.00 | 50    | 1          |
  And existe un cliente "Juan Perez" con TaxDocument "12345678" para ventas

# ============================================================
# PROCESO PRINCIPAL - 1 Solo Happy Path
# ============================================================

@integration @sales @process @happy
Scenario: VT-01 - Procesar venta completa con multiples productos - Happy Path
  Given tengo los siguientes productos en el carrito:
    | ProductName    | Quantity |
    | Quinua Premium | 3        |
    | Arroz Integral | 5        |
    | Aceite Vegetal | 2        |
  When proceso la venta con los siguientes datos:
    | Campo        | Valor    |
    | ClientId     | 1        |
    | PaymentType  | Efectivo |
    | CashReceived | 200.00   |
  Then la venta debe registrarse exitosamente
  And el total debe ser 184.50
  And el cambio debe ser 15.50
  And el stock de "Quinua Premium" debe ser 97
  And el stock de "Arroz Integral" debe ser 75
  And el stock de "Aceite Vegetal" debe ser 48
  And debe existir 1 registro en Sales
  And deben existir 3 registros en SaleItems
  And la fecha de venta debe ser la actual
