Feature: Integracion CRUD de Clientes
  Como sistema de gestion
  Quiero poder crear leer actualizar y eliminar clientes
  Para gestionar la cartera de clientes

Background:
  Given la base de datos esta limpia

# ============================================================
# CREATE - 2 Happy Path + 1 Unhappy Path
# ============================================================

@integration @client @create @happy
Scenario: CL-01 - Crear cliente completo - Happy Path 1
  When creo un cliente con los siguientes datos:
    | Campo        | Valor                    |
    | BusinessName | Juan Perez               |
    | Email        | juan.perez@example.com   |
    | TaxDocument  | 1234567                  |
    | Address      | Av. Siempre Viva 123     |
  Then el cliente debe crearse exitosamente
  And debe tener Status 1
  And el TaxDocument debe ser unico

@integration @client @create @happy
Scenario: CL-02 - Crear cliente sin direccion - Happy Path 2
  When creo un cliente con los siguientes datos:
    | Campo        | Valor                      |
    | BusinessName | Maria Garcia               |
    | Email        | maria.garcia@example.com   |
    | TaxDocument  | 8765432                    |
    | Address      |                            |
  Then el cliente debe crearse exitosamente
  And debe tener Status 1

@integration @client @create @unhappy
Scenario: CL-03 - Crear cliente con email invalido - Unhappy Path
  When intento crear un cliente con email "correo_invalido"
  Then la creacion debe fallar
  And debe mostrar error de validacion de email

# ============================================================
# UPDATE - 2 Happy Path + 1 Unhappy Path
# ============================================================

@integration @client @update @happy
Scenario: CL-04 - Actualizar nombre y direccion - Happy Path 1
  Given existe un cliente con TaxDocument "1111111"
  When actualizo el cliente con:
    | Campo        | Valor                    |
    | BusinessName | Pedro Lopez Actualizado  |
    | Address      | Nueva Direccion 789      |
  Then la actualizacion debe ser exitosa
  And los datos deben estar actualizados en BD

@integration @client @update @happy
Scenario: CL-05 - Actualizar email valido - Happy Path 2
  Given existe un cliente "Ana Martinez" con email "ana@example.com"
  When actualizo el email a "ana.martinez@newemail.com"
  Then la actualizacion debe ser exitosa
  And el nuevo email debe ser unico

@integration @client @update @unhappy
Scenario: CL-06 - Actualizar con TaxDocument duplicado - Unhappy Path
  Given existen los siguientes clientes:
    | BusinessName | TaxDocument |
    | Cliente A    | 1111111     |
    | Cliente B    | 2222222     |
  When intento actualizar "Cliente B" con TaxDocument "1111111"
  Then la actualizacion debe fallar
  And debe mostrar error de TaxDocument duplicado

# ============================================================
# DELETE - Solo Happy Path
# ============================================================

@integration @client @delete @happy
Scenario: CL-07 - Eliminar cliente - Happy Path
  Given existe un cliente con TaxDocument "9999999"
  When elimino el cliente
  Then el Status del cliente debe cambiar a 0
  And el cliente no debe aparecer en busquedas activas

# ============================================================
# SELECT - Solo Happy Path
# ============================================================

@integration @client @read @happy
Scenario: CL-08 - Buscar cliente por TaxDocument - Happy Path
  Given existe un cliente "Roberto Vargas" con TaxDocument "5555555"
  When busco el cliente por TaxDocument "5555555"
  Then debo recibir los datos del cliente
  And el BusinessName debe ser "Roberto Vargas"
