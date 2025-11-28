Feature: Integracion CRUD de Productos
  Como sistema de gestion
  Quiero poder crear leer actualizar y eliminar productos
  Para mantener el inventario actualizado

Background:
  Given la base de datos esta limpia
  And existe una categoria "Alimentos" con id 1

# ============================================================
# CREATE - 2 Happy Path + 1 Unhappy Path
# ============================================================

@integration @product @create @happy
Scenario: PR-01 - Crear producto completo - Happy Path 1
  When creo un producto con los siguientes datos:
    | Campo       | Valor              |
    | Name        | Quinua Organica    |
    | Description | Quinua de Bolivia  |
    | Brand       | Bolivia Natural    |
    | Price       | 28.50              |
    | Stock       | 100                |
    | CategoryId  | 1                  |
  Then el producto debe crearse exitosamente
  And debe tener Stock 100
  And debe estar asociado a la categoria 1

@integration @product @create @happy
Scenario: PR-02 - Crear producto con descripcion vacia - Happy Path 2
  When creo un producto con los siguientes datos:
    | Campo       | Valor                    |
    | Name        | Arroz Premium 1kg        |
    | Description |                          |
    | Brand       | Del Campo                |
    | Price       | 12.50                    |
    | Stock       | 200                      |
    | CategoryId  | 1                        |
  Then el producto debe crearse exitosamente
  And debe tener Stock 200

@integration @product @create @unhappy
Scenario: PR-03 - Crear producto con precio negativo - Unhappy Path
  When intento crear un producto con precio -10.00
  Then la creacion debe fallar
  And debe mostrar error de validacion de precio

# ============================================================
# UPDATE - 2 Happy Path + 1 Unhappy Path
# ============================================================

@integration @product @update @happy
Scenario: PR-04 - Actualizar precio y stock - Happy Path 1
  Given existe un producto "Azucar Refinada" con precio 8.50 y stock 50
  When actualizo el producto con:
    | Campo | Valor |
    | Price | 9.00  |
    | Stock | 75    |
  Then la actualizacion debe ser exitosa
  And el precio debe ser 9.00
  And el stock debe ser 75

@integration @product @update @happy
Scenario: PR-05 - Actualizar manteniendo mismo nombre - Happy Path 2
  Given existe un producto "Sal Fina 1kg" con precio 3.50 y stock 80
  When actualizo el producto con:
    | Campo       | Valor        |
    | Name        | Sal Fina 1kg |
    | Description | Sal refinada |
    | Price       | 4.00         |
  Then la actualizacion debe ser exitosa
  And los nuevos datos deben estar guardados

@integration @product @update @unhappy
Scenario: PR-06 - Actualizar con stock negativo - Unhappy Path
  Given existe un producto "Aceite Vegetal" con stock 30
  When intento actualizar el stock a -5
  Then la actualizacion debe fallar
  And debe mostrar error de validacion de stock

# ============================================================
# DELETE - Solo Happy Path
# ============================================================

@integration @product @delete @happy
Scenario: PR-07 - Eliminar producto - Happy Path
  Given existe un producto "Producto Temporal"
  When elimino el producto
  Then el Status del producto debe cambiar a 0
  And el producto no debe aparecer en busquedas activas

# ============================================================
# SELECT - Solo Happy Path
# ============================================================

@integration @product @read @happy
Scenario: PR-08 - Buscar productos por categoria - Happy Path
  Given existen los siguientes productos activos en categoria 1:
    | Name   | Price | Stock |
    | Arroz  | 10.00 | 50    |
    | Azucar | 8.00  | 60    |
    | Sal    | 3.50  | 100   |
  When busco productos de la categoria 1
  Then debo recibir 3 productos
  And todos deben tener Status 1
