Feature: CRUD de Clientes - UI Testing con Pairwise
Como usuario del sistema
Quiero gestionar clientes
Para mantener la informacion actualizada

Background:
  Given que la aplicacion esta en ejecucion
  And navego a la pagina de clientes

@ui @client @create @pairwise @pw-cli-01
Scenario Outline: PW-CLI-01 - Crear cliente con combinaciones Pairwise
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
    | TestID | Nombre             | Email                      | Documento | Direccion                        | Resultado                               |
    | PW01   | ClientePairwise01  | cliente.pw01@example.com   | 11111111  | Av Principal 123                 | debo ver mensaje de exito en cliente    |
    | PW02   | ClientePairwise02  | cliente.pw02@example.com   | 22222222  |                                  | debo ver mensaje de exito en cliente    |
    | PW03   | ClientePairwise03  | cliente.pw03@example.com   | 33333333  | Calle Larga 456 Zona Central     | debo ver mensaje de exito en cliente    |
    | PW04   | ClienteDescLargo04 | cliente.pw04@example.com   | 44444444  | Direccion muy larga para probar limites del campo | debo ver mensaje de exito en cliente |
    | PW05   | ClientePairwise05  | email.largo.pw05@dominio.com.bo | 55555555 | Calle 789                     | debo ver mensaje de exito en cliente    |
    | PW06   | ClientePairwise06  | pw06@test.com              | 66666666  | Zona Norte 321                   | debo ver mensaje de exito en cliente    |
    | PW07   |                    | cliente.pw07@example.com   | 77777777  | Direccion 987                    | debo ver error de validacion en cliente |
    | PW08   | ClienteSinEmail08  |                            | 88888888  | Calle Sin Email                  | debo ver error de validacion en cliente |

@ui @client @update @pairwise @pw-cli-02
Scenario Outline: PW-CLI-02 - Actualizar cliente con combinaciones Pairwise
  Given existe un cliente creado con documento "99990001"
  When hago clic en editar el cliente con documento "99990001"
  And actualizo el formulario de cliente con:
    | Campo     | Valor            |
    | Nombre    | <NuevoNombre>    |
    | Email     | <NuevoEmail>     |
    | Direccion | <NuevaDireccion> |
  And hago clic en guardar cliente
  Then <Resultado>

  Examples:
    | TestID | NuevoNombre            | NuevoEmail                  | NuevaDireccion               | Resultado                             |
    | PW09   | ClienteActualizado09   | actualizado09@example.com   | Nueva Av 789                 | debo ver mensaje de exito en cliente  |
    | PW10   | ClienteModificado10    | modificado10@example.com    |                              | debo ver mensaje de exito en cliente  |
    | PW11   | ClienteNuevoEmail11    | nuevo.email11@example.com   | Direccion Nueva 456          | debo ver mensaje de exito en cliente  |
    | PW12   | ClienteDirLarga12      | dirlarga12@test.com         | Direccion actualizada muy larga para validar | debo ver mensaje de exito en cliente |
    | PW13   | ClienteCompUpdate13    | completo13@dominio.com      | Actualizacion completa 123   | debo ver mensaje de exito en cliente  |
    | PW14   | ClienteFinalUpd14      | final14@test.com            | Final update direccion       | debo ver mensaje de exito en cliente  |

@ui @client @bva @bva-cli-01
Scenario: BVA-CLI-01 - Crear cliente con nombre de 3 caracteres (minimo)
  When hago clic en agregar nuevo cliente
  And lleno el formulario de cliente con los siguientes datos:
    | Campo     | Valor                      |
    | Nombre    | Cli                        |
    | Email     | nombmin.bva01@example.com  |
    | Documento | 10000001                   |
    | Direccion | Direccion BVA nombre min   |
  And hago clic en guardar cliente
  Then debo ver mensaje de exito en cliente

@ui @client @bva @bva-cli-02
Scenario: BVA-CLI-02 - Crear cliente con nombre de 100 caracteres (maximo)
  When hago clic en agregar nuevo cliente
  And lleno el formulario de cliente con los siguientes datos:
    | Campo     | Valor                                                                                                |
    | Nombre    | ClienteConNombreDeNegocioMuyLargoQueTieneCienCaracteresExactosParaProbarElLimiteMaximoDelCampoNombre |
    | Email     | nombmax.bva02@example.com                                                                            |
    | Documento | 10000002                                                                                             |
    | Direccion | Direccion BVA nombre max                                                                             |
  And hago clic en guardar cliente
  Then debo ver mensaje de exito en cliente

@ui @client @bva @bva-cli-03
Scenario: BVA-CLI-03 - Crear cliente con documento de 7 digitos (minimo tipico)
  When hago clic en agregar nuevo cliente
  And lleno el formulario de cliente con los siguientes datos:
    | Campo     | Valor                      |
    | Nombre    | ClienteDocMinBVA           |
    | Email     | docmin.bva03@example.com   |
    | Documento | 1000003                    |
    | Direccion | Direccion BVA doc min      |
  And hago clic en guardar cliente
  Then debo ver mensaje de exito en cliente

@ui @client @bva @bva-cli-04
Scenario: BVA-CLI-04 - Crear cliente con documento de 9 digitos (maximo)
  When hago clic en agregar nuevo cliente
  And lleno el formulario de cliente con los siguientes datos:
    | Campo     | Valor                      |
    | Nombre    | ClienteDocMaxBVA           |
    | Email     | docmax.bva04@example.com   |
    | Documento | 100000004                  |
    | Direccion | Direccion BVA doc max      |
  And hago clic en guardar cliente
  Then debo ver mensaje de exito en cliente

@ui @client @bva @bva-cli-05
Scenario: BVA-CLI-05 - Crear cliente con email formato minimo valido
  When hago clic en agregar nuevo cliente
  And lleno el formulario de cliente con los siguientes datos:
    | Campo     | Valor                      |
    | Nombre    | ClienteEmailMinBVA         |
    | Email     | a@b.co                     |
    | Documento | 10000005                   |
    | Direccion | Direccion BVA email min    |
  And hago clic en guardar cliente
  Then debo ver mensaje de exito en cliente

@ui @client @bva @bva-cli-06
Scenario: BVA-CLI-06 - Crear cliente con direccion vacia (campo opcional)
  When hago clic en agregar nuevo cliente
  And lleno el formulario de cliente con los siguientes datos:
    | Campo     | Valor                      |
    | Nombre    | ClienteSinDirBVA           |
    | Email     | sindir.bva06@example.com   |
    | Documento | 10000006                   |
    | Direccion |                            |
  And hago clic en guardar cliente
  Then debo ver mensaje de exito en cliente

@ui @client @pe @pe-cli-01
Scenario: PE-CLI-01 - Crear cliente con documento duplicado
  Given existe un cliente creado con documento "20000001"
  When hago clic en agregar nuevo cliente
  And lleno el formulario de cliente con los siguientes datos:
    | Campo     | Valor                      |
    | Nombre    | ClienteDuplicadoPE         |
    | Email     | duplicado.pe@example.com   |
    | Documento | 20000001                   |
    | Direccion | Direccion duplicada        |
  And hago clic en guardar cliente
  Then debo ver error de validacion en cliente

@ui @client @pe @pe-cli-02
Scenario: PE-CLI-02 - Eliminar cliente existente (borrado logico)
  Given existe un cliente creado con documento "30000002"
  When hago clic en eliminar el cliente con documento "30000002"
  Then el cliente con documento "30000002" no debe aparecer en la lista

@ui @client @pe @pe-cli-03
Scenario: PE-CLI-03 - Listar clientes activos
  Given existen los siguientes clientes en el sistema:
    | Nombre       | Documento |
    | ClienteListA | 40000001  |
    | ClienteListB | 40000002  |
    | ClienteListC | 40000003  |
  When navego a la pagina de clientes
  Then debo ver al menos 3 clientes en la lista

@ui @client @pe @pe-cli-04
Scenario: PE-CLI-04 - Actualizar cliente manteniendo documento unico propio
  Given existe un cliente creado con documento "50000004"
  When hago clic en editar el cliente con documento "50000004"
  And actualizo el formulario de cliente con:
    | Campo     | Valor                          |
    | Nombre    | ClienteMismoDocPE              |
    | Email     | mismodoc.pe@example.com        |
    | Direccion | Solo cambio nombre y email     |
  And hago clic en guardar cliente
  Then debo ver mensaje de exito en cliente

@ui @client @pe @pe-cli-05
Scenario: PE-CLI-05 - Crear cliente con email formato valido complejo
  When hago clic en agregar nuevo cliente
  And lleno el formulario de cliente con los siguientes datos:
    | Campo     | Valor                                |
    | Nombre    | ClienteEmailComplejoPE               |
    | Email     | nombre.apellido+tag@subdominio.empresa.com.bo |
    | Documento | 60000005                             |
    | Direccion | Direccion email complejo             |
  And hago clic en guardar cliente
  Then debo ver mensaje de exito en cliente

@ui @client @pe @pe-cli-06
Scenario: PE-CLI-06 - Crear cliente con todos los campos completos
  When hago clic en agregar nuevo cliente
  And lleno el formulario de cliente con los siguientes datos:
    | Campo     | Valor                                                           |
    | Nombre    | Cliente Empresa Completa SA                                     |
    | Email     | contacto.comercial@empresa-completa.com.bo                      |
    | Documento | 70000006                                                        |
    | Direccion | Av. Principal 1234, Edificio Central, Piso 5, Oficina 501, Zona Sur |
  And hago clic en guardar cliente
  Then debo ver mensaje de exito en cliente
