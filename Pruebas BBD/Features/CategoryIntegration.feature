Feature: Integracion CRUD de Categorias
  Como sistema de gestion
  Quiero poder crear leer actualizar y eliminar categorias
  Para mantener organizado el inventario

Background:
  Given la base de datos esta limpia

# ============================================================
# CREATE - 2 Happy Path + 1 Unhappy Path
# ============================================================

@integration @category @create @happy
Scenario: CA-01 - Crear categoria basica - Happy Path 1
  When creo una categoria con los siguientes datos:
    | Campo       | Valor                     |
    | Name        | Lacteos                   |
    | Description | Productos lacteos frescos |
  Then la categoria debe crearse exitosamente
  And el nombre debe ser "Lacteos"
  And el Status debe ser 1

@integration @category @create @happy
Scenario: CA-02 - Crear categoria con descripcion larga - Happy Path 2
  When creo una categoria con los siguientes datos:
    | Campo       | Valor                                                |
    | Name        | Bebidas                                              |
    | Description | Bebidas alcoholicas no alcoholicas jugos y refrescos |
  Then la categoria debe crearse exitosamente
  And el nombre debe ser "Bebidas"
  And el Status debe ser 1

@integration @category @create @unhappy
Scenario: CA-03 - Crear categoria sin nombre - Unhappy Path
  When creo una categoria con los siguientes datos:
    | Campo       | Valor                    |
    | Name        |                          |
    | Description | Descripcion sin nombre   |
  Then la creacion debe fallar

# ============================================================
# UPDATE - 2 Happy Path + 1 Unhappy Path
# ============================================================

@integration @category @update @happy
Scenario: CA-04 - Actualizar categoria exitosamente - Happy Path 1
  Given existe una categoria con nombre "Lacteos"
  When actualizo la categoria con:
    | Campo       | Valor                                  |
    | Name        | Productos Lacteos                      |
    | Description | Leche queso yogurt y derivados lacteos |
  Then la actualizacion debe ser exitosa
  And los datos deben reflejarse en la base de datos

@integration @category @update @happy
Scenario: CA-05 - Actualizar manteniendo mismo nombre - Happy Path 2
  Given existe una categoria con nombre "Bebidas"
  When actualizo la categoria con:
    | Campo       | Valor                          |
    | Name        | Bebidas                        |
    | Description | Bebidas varias actualizadas    |
  Then la actualizacion debe ser exitosa

@integration @category @update @unhappy
Scenario: CA-06 - Actualizar con nombre duplicado - Unhappy Path
  Given existen las siguientes categorias:
    | Name      | Description    |
    | Lacteos   | Productos      |
    | Bebidas   | Liquidos       |
  When intento actualizar "Bebidas" con nombre "Lacteos"
  Then la actualizacion debe fallar

# ============================================================
# DELETE - Solo Happy Path
# ============================================================

@integration @category @delete @happy
Scenario: CA-07 - Eliminar categoria - Happy Path
  Given existe una categoria con nombre "Temporal"
  When elimino la categoria
  Then el Status de la categoria debe cambiar a 0
  And la categoria no debe aparecer en busquedas activas

# ============================================================
# SELECT - Solo Happy Path
# ============================================================

@integration @category @read @happy
Scenario: CA-08 - Listar categorias activas - Happy Path
  Given existen las siguientes categorias:
    | Name      | Description | Status |
    | Activa1   | Desc1       | 1      |
    | Activa2   | Desc2       | 1      |
    | Inactiva  | Desc3       | 0      |
  When obtengo todas las categorias
  Then debo recibir 2 categorias
  And todas deben tener Status 1
