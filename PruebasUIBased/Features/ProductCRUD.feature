Feature: CRUD de Productos - UI Testing
Como usuario del sistema
Quiero gestionar productos
Para mantener el inventario actualizado

Background:
  Given que la aplicacion esta en ejecucion
  And navego a la pagina de productos
  And existe una categoria "Alimentos" para productos

@ui @product @create @pairwise
Scenario Outline: CP-01 - Crear producto con combinaciones Pairwise
  When hago clic en agregar nuevo producto
  And lleno el formulario de producto con los siguientes datos:
    | Campo       | Valor         |
    | Nombre      | <Nombre>      |
    | Descripcion | <Descripcion> |
    | Marca       | <Marca>       |
    | Precio      | <Precio>      |
    | Stock       | <Stock>       |
    | Categoria   | 1             |
  And hago clic en guardar producto
  Then <Resultado>

  Examples:
    | Nombre           | Descripcion     | Marca       | Precio | Stock | Resultado                                |
    | Producto Valid 1 | Desc valida     | MarcaA      | 10.50  | 100   | debo ver mensaje de exito en producto    |
    | Producto Valid 2 |                 | MarcaB      | 5.00   | 1     | debo ver mensaje de exito en producto    |
    | Producto Valid 3 | Desc muy larga  | MarcaC      | 999.99 | 9999  | debo ver mensaje de exito en producto    |
    |                  | Desc sin nombre | MarcaSinNom | 10.00  | 50    | debo ver error de validacion en producto |

@ui @product @update @pairwise
Scenario Outline: CP-02 - Actualizar producto con combinaciones Pairwise
  Given existe un producto creado con nombre "ProductoParaActualizar"
  When hago clic en editar producto "ProductoParaActualizar"
  And actualizo el formulario de producto con:
    | Campo       | Valor              |
    | Nombre      | <NuevoNombre>      |
    | Descripcion | <NuevaDescripcion> |
    | Precio      | <NuevoPrecio>      |
    | Stock       | <NuevoStock>       |
  And hago clic en guardar producto
  Then <Resultado>

  Examples:
    | NuevoNombre            | NuevaDescripcion | NuevoPrecio | NuevoStock | Resultado                             |
    | ProductoActualizado    | Desc actualizada | 15.00       | 200        | debo ver mensaje de exito en producto |
    | ProductoParaActualizar | Nueva desc       | 20.00       | 150        | debo ver mensaje de exito en producto |
    | ProductoNuevoPrecio    |                  | 25.50       | 1          | debo ver mensaje de exito en producto |

@ui @product @delete @happy
Scenario: CP-03 - Eliminar producto
  Given existe un producto creado con nombre "ProductoParaEliminar"
  When hago clic en eliminar producto "ProductoParaEliminar"
  Then el producto "ProductoParaEliminar" no debe aparecer en la lista

@ui @product @read @happy
Scenario: CP-04 - Listar productos activos
  Given existen los siguientes productos en el sistema:
    | Nombre     |
    | Producto A |
    | Producto B |
    | Producto C |
  When navego a la pagina de productos
  Then debo ver al menos 3 productos en la lista
