Feature: CRUD de Clientes - UI Testing
Como usuario del sistema
Quiero gestionar clientes
Para mantener la informacion actualizada

Background:
  Given que la aplicacion esta en ejecucion
  And navego a la pagina de clientes

@ui @client @create @pairwise
Scenario Outline: CCL-01 - Crear cliente con combinaciones Pairwise
  When hago clic en agregar nuevo cliente
  And lleno el formulario de cliente con los siguientes datos:
    | Campo     | Valor       |
    | Nombre    | <Nombre>    |
    | Email     | <Email>     |
    | Documento | <Documento> |
    | Direccion | <Direccion> |
  And hago clic en guardar cliente
  Then <Resultado>

  Examples:
    | Nombre          | Email                | Documento | Direccion        | Resultado                               |
    | Cliente Valid 1 | cliente1@example.com | 1111111   | Av Principal 123 | debo ver mensaje de exito en cliente   |
    | Cliente Valid 2 | cliente2@example.com | 2222222   |                  | debo ver mensaje de exito en cliente   |
    | Cliente Valid 3 | cliente3@example.com | 3333333   | Direccion larga  | debo ver mensaje de exito en cliente   |
    |                 | cliente4@example.com | 4444444   | Direccion 456    | debo ver error de validacion en cliente |

@ui @client @update @pairwise
Scenario Outline: CCL-02 - Actualizar cliente con combinaciones Pairwise
  Given existe un cliente creado con documento "5555555"
  When hago clic en editar el cliente con documento "5555555"
  And actualizo el formulario de cliente con:
    | Campo     | Valor            |
    | Nombre    | <NuevoNombre>    |
    | Email     | <NuevoEmail>     |
    | Direccion | <NuevaDireccion> |
  And hago clic en guardar cliente
  Then <Resultado>

  Examples:
    | NuevoNombre        | NuevoEmail              | NuevaDireccion  | Resultado                             |
    | ClienteActualizado | actualizado@example.com | Nueva Av 789    | debo ver mensaje de exito en cliente |
    | ClienteModificado  | modificado@example.com  |                 | debo ver mensaje de exito en cliente |
    | ClienteNuevo       | nuevo@example.com       | Direccion Nueva | debo ver mensaje de exito en cliente |

@ui @client @delete @happy
Scenario: CCL-03 - Eliminar cliente
  Given existe un cliente creado con documento "9999999"
  When hago clic en eliminar el cliente con documento "9999999"
  Then el cliente con documento "9999999" no debe aparecer en la lista

@ui @client @read @happy
Scenario: CCL-04 - Listar clientes activos
  Given existen los siguientes clientes en el sistema:
    | Nombre    | Documento |
    | Cliente A | 7777777   |
    | Cliente B | 8888888   |
    | Cliente C | 6666666   |
  When navego a la pagina de clientes
  Then debo ver al menos 3 clientes en la lista
