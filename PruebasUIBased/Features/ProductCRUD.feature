Feature: CRUD de Productos - UI Testing con Pairwise
Como usuario del sistema
Quiero gestionar productos
Para mantener el inventario actualizado

Background:
  Given que la aplicacion esta en ejecucion
  And navego a la pagina de productos
  And existe una categoria "Alimentos" para productos

@ui @product @create @pairwise @pw-prod-01
Scenario Outline: PW-PROD-01 - Crear producto con combinaciones Pairwise
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor         |
    | Nombre      | <Nombre>      |
    | Descripcion | <Descripcion> |
    | Marca       | <Marca>       |
    | Precio      | <Precio>      |
    | Stock       | <Stock>       |
    | Categoria   | <Categoria>   |
  And hago clic en guardar producto
  Then <Resultado>

  Examples:
    | TestID | Nombre             | Descripcion          | Marca       | Precio  | Stock | Categoria | Resultado                                |
    | PW01   | ProdPairwise01     | Descripcion valida   | MarcaA      | 10.50   | 100   | 1         | debo ver mensaje de exito en producto    |
    | PW02   | ProdPairwise02     |                      | MarcaB      | 5.00    | 1     | 1         | debo ver mensaje de exito en producto    |
    | PW03   | ProdDescLarga03    | Desc muy larga test  | MarcaC      | 999.99  | 9999  | 1         | debo ver mensaje de exito en producto    |
    | PW04   | ProdPairwise04     | Desc normal          |             | 15.00   | 50    | 1         | debo ver mensaje de exito en producto    |
    | PW05   | ProdPairwise05     | Desc test            | MarcaLarga  | 0.01    | 500   | 1         | debo ver mensaje de exito en producto    |
    | PW06   | ProdPairwise06     |                      | MarcaD      | 100.00  | 10    | 1         | debo ver mensaje de exito en producto    |
    | PW07   | ProdStockAlto07    | Desc stock alto      | MarcaE      | 50.00   | 32767 | 1         | debo ver mensaje de exito en producto    |
    | PW08   | ProdPrecioAlto08   | Desc precio alto     | MarcaF      | 9999.99 | 25    | 1         | debo ver mensaje de exito en producto    |
    | PW09   |                    | Desc sin nombre      | MarcaG      | 20.00   | 30    | 1         | debo ver error de validacion en producto |
    | PW10   | ProdSinPrecio10    | Desc sin precio      | MarcaH      | 0       | 40    | 1         | debo ver error de validacion en producto |

@ui @product @update @pairwise @pw-prod-02
Scenario Outline: PW-PROD-02 - Actualizar producto con combinaciones Pairwise
  Given existe un producto creado con nombre "ProdBaseUpdate"
  When hago clic en editar producto "ProdBaseUpdate"
  And actualizo el formulario de producto con:
    | Campo       | Valor              |
    | Nombre      | <NuevoNombre>      |
    | Descripcion | <NuevaDescripcion> |
    | Precio      | <NuevoPrecio>      |
    | Stock       | <NuevoStock>       |
  And hago clic en guardar producto
  Then <Resultado>

  Examples:
    | TestID | NuevoNombre          | NuevaDescripcion      | NuevoPrecio | NuevoStock | Resultado                             |
    | PW11   | ProdActualizado11    | Desc actualizada      | 15.00       | 200        | debo ver mensaje de exito en producto |
    | PW12   | ProdBaseUpdate       | Nueva desc mismo nom  | 20.00       | 150        | debo ver mensaje de exito en producto |
    | PW13   | ProdNuevoPrecio13    |                       | 25.50       | 1          | debo ver mensaje de exito en producto |
    | PW14   | ProdStockMod14       | Stock modificado      | 30.00       | 9999       | debo ver mensaje de exito en producto |
    | PW15   | ProdPrecioMin15      | Precio minimo update  | 0.01        | 100        | debo ver mensaje de exito en producto |
    | PW16   | ProdDescLargaUpd16   | Descripcion extensa para validar campos largos | 45.00 | 75 | debo ver mensaje de exito en producto |
    | PW17   | ProdCompletoUpd17    | Actualizacion total   | 99.99       | 500        | debo ver mensaje de exito en producto |
    | PW18   | ProdFinalUpd18       |                       | 12.50       | 250        | debo ver mensaje de exito en producto |

@ui @product @bva @bva-prod-01
Scenario: BVA-PROD-01 - Crear producto con precio minimo valido (0.01)
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor              |
    | Nombre      | ProdPrecioMinBVA   |
    | Descripcion | Precio al minimo   |
    | Marca       | MarcaBVA           |
    | Precio      | 0.01               |
    | Stock       | 50                 |
    | Categoria   | 1                  |
  And hago clic en guardar producto
  Then debo ver mensaje de exito en producto

@ui @product @bva @bva-prod-02
Scenario: BVA-PROD-02 - Crear producto con stock minimo valido (1)
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor              |
    | Nombre      | ProdStockMinBVA    |
    | Descripcion | Stock al minimo    |
    | Marca       | MarcaBVA2          |
    | Precio      | 25.00              |
    | Stock       | 1                  |
    | Categoria   | 1                  |
  And hago clic en guardar producto
  Then debo ver mensaje de exito en producto

@ui @product @bva @bva-prod-03
Scenario: BVA-PROD-03 - Crear producto con stock maximo valido (32767)
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor              |
    | Nombre      | ProdStockMaxBVA    |
    | Descripcion | Stock al maximo    |
    | Marca       | MarcaBVA3          |
    | Precio      | 35.00              |
    | Stock       | 32767              |
    | Categoria   | 1                  |
  And hago clic en guardar producto
  Then debo ver mensaje de exito en producto

@ui @product @bva @bva-prod-04
Scenario: BVA-PROD-04 - Crear producto con nombre de 1 caracter
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor              |
    | Nombre      | P                  |
    | Descripcion | Nombre minimo      |
    | Marca       | MarcaBVA4          |
    | Precio      | 10.00              |
    | Stock       | 20                 |
    | Categoria   | 1                  |
  And hago clic en guardar producto
  Then debo ver mensaje de exito en producto

@ui @product @bva @bva-prod-05
Scenario: BVA-PROD-05 - Crear producto con nombre de 100 caracteres (maximo)
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor              |
    | Nombre      | ProductoConNombreDeCienCaracteresExactosParaProbarElLimiteMaximoPermitidoEnElCampoDeNombreDelSistema |
    | Descripcion | Nombre al maximo   |
    | Marca       | MarcaBVA5          |
    | Precio      | 15.00              |
    | Stock       | 30                 |
    | Categoria   | 1                  |
  And hago clic en guardar producto
  Then debo ver mensaje de exito en producto

@ui @product @bva @bva-prod-06
Scenario: BVA-PROD-06 - Crear producto con precio maximo valido (9999.99)
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor              |
    | Nombre      | ProdPrecioMaxBVA   |
    | Descripcion | Precio al maximo   |
    | Marca       | MarcaBVA6          |
    | Precio      | 9999.99            |
    | Stock       | 15                 |
    | Categoria   | 1                  |
  And hago clic en guardar producto
  Then debo ver mensaje de exito en producto

@ui @product @bva @bva-prod-07
Scenario: BVA-PROD-07 - Crear producto con marca de 1 caracter
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor              |
    | Nombre      | ProdMarcaMinBVA    |
    | Descripcion | Marca minima       |
    | Marca       | M                  |
    | Precio      | 20.00              |
    | Stock       | 40                 |
    | Categoria   | 1                  |
  And hago clic en guardar producto
  Then debo ver mensaje de exito en producto

@ui @product @bva @bva-prod-08
Scenario: BVA-PROD-08 - Crear producto con descripcion vacia (opcional)
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor              |
    | Nombre      | ProdSinDescBVA     |
    | Descripcion |                    |
    | Marca       | MarcaBVA8          |
    | Precio      | 22.00              |
    | Stock       | 55                 |
    | Categoria   | 1                  |
  And hago clic en guardar producto
  Then debo ver mensaje de exito en producto

@ui @product @pe @pe-prod-01
Scenario: PE-PROD-01 - Crear producto con nombre duplicado
  Given existe un producto creado con nombre "ProdDuplicadoPE"
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor              |
    | Nombre      | ProdDuplicadoPE    |
    | Descripcion | Intentando duplicar|
    | Marca       | MarcaPE            |
    | Precio      | 30.00              |
    | Stock       | 60                 |
    | Categoria   | 1                  |
  And hago clic en guardar producto
  Then debo ver error de validacion en producto

@ui @product @pe @pe-prod-02
Scenario: PE-PROD-02 - Eliminar producto existente (borrado logico)
  Given existe un producto creado con nombre "ProdParaEliminarPE"
  When hago clic en eliminar producto "ProdParaEliminarPE"
  Then el producto "ProdParaEliminarPE" no debe aparecer en la lista

@ui @product @pe @pe-prod-03
Scenario: PE-PROD-03 - Listar productos activos
  Given existen los siguientes productos en el sistema:
    | Nombre        |
    | ProdListadoA  |
    | ProdListadoB  |
    | ProdListadoC  |
  When navego a la pagina de productos
  Then debo ver al menos 3 productos en la lista

@ui @product @pe @pe-prod-04
Scenario: PE-PROD-04 - Actualizar producto manteniendo nombre unico propio
  Given existe un producto creado con nombre "ProdMismoNombrePE"
  When hago clic en editar producto "ProdMismoNombrePE"
  And actualizo el formulario de producto con:
    | Campo       | Valor              |
    | Nombre      | ProdMismoNombrePE  |
    | Descripcion | Solo cambio desc   |
    | Precio      | 45.00              |
    | Stock       | 80                 |
  Then debo ver mensaje de exito en producto

@ui @product @pe @pe-prod-05
Scenario: PE-PROD-05 - Crear producto con categoria valida existente
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor              |
    | Nombre      | ProdCatValidaPE    |
    | Descripcion | Categoria valida   |
    | Marca       | MarcaPE5           |
    | Precio      | 55.00              |
    | Stock       | 90                 |
    | Categoria   | 1                  |
  And hago clic en guardar producto
  Then debo ver mensaje de exito en producto

@ui @product @pe @pe-prod-06
Scenario: PE-PROD-06 - Actualizar solo precio de producto
  Given existe un producto creado con nombre "ProdSoloPrecioPE"
  When hago clic en editar producto "ProdSoloPrecioPE"
  And actualizo el formulario de producto con:
    | Campo       | Valor              |
    | Nombre      | ProdSoloPrecioPE   |
    | Descripcion |                    |
    | Precio      | 75.50              |
    | Stock       | 100                |
  Then debo ver mensaje de exito en producto

@ui @product @pe @pe-prod-07
Scenario: PE-PROD-07 - Actualizar solo stock de producto
  Given existe un producto creado con nombre "ProdSoloStockPE"
  When hago clic en editar producto "ProdSoloStockPE"
  And actualizo el formulario de producto con:
    | Campo       | Valor              |
    | Nombre      | ProdSoloStockPE    |
    | Descripcion |                    |
    | Precio      | 35.00              |
    | Stock       | 999                |
  Then debo ver mensaje de exito en producto

@ui @product @pe @pe-prod-08
Scenario: PE-PROD-08 - Crear producto con todos los campos completos
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor                                                                    |
    | Nombre      | ProdCompletoPE08                                                         |
    | Descripcion | Descripcion completa con todos los detalles del producto para prueba PE  |
    | Marca       | MarcaCompletaPE                                                          |
    | Precio      | 125.99                                                                   |
    | Stock       | 250                                                                      |
    | Categoria   | 1                                                                        |
  And hago clic en guardar producto
  Then debo ver mensaje de exito en producto
