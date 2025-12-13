Feature: CRUD de Categorias - UI Testing
Como usuario del sistema
Quiero gestionar categorias
Para organizar los productos

Background:
  Given que la aplicacion esta en ejecucion
  And navego a la pagina de categorias

@ui @category @create @pairwise
Scenario Outline: CC-01 - Crear categoria con combinaciones Pairwise
  When hago clic en agregar nueva categoria
  And lleno el formulario de categoria con nombre "<Nombre>" y descripcion "<Descripcion>"
  And hago clic en guardar categoria
  Then <Resultado>

  Examples:
    | Nombre               | Descripcion                        | Resultado                                 |
    | Cat Valida 1         | Descripcion valida normal          | debo ver mensaje de exito en categoria    |
    | Cat Valida 2         |                                    | debo ver mensaje de exito en categoria    |
    | Cat Muy Larga Nombre | Descripcion con 255 caracteres max | debo ver mensaje de exito en categoria    |
    |                      | Descripcion sin nombre             | debo ver error de validacion en categoria |

@ui @category @update @pairwise
Scenario Outline: CC-02 - Actualizar categoria con combinaciones Pairwise
  Given existe una categoria creada con nombre "CatBase"
  When hago clic en editar categoria "CatBase"
  And actualizo el formulario con nombre "<NuevoNombre>" y descripcion "<NuevaDescripcion>"
  And hago clic en guardar categoria
  Then <Resultado>

  Examples:
    | NuevoNombre        | NuevaDescripcion         | Resultado                              |
    | CatActualizada     | Nueva descripcion valida | debo ver mensaje de exito en categoria |
    | CatModificada      | Descripcion modificada   | debo ver mensaje de exito en categoria |
    | NombreNuevo        |                          | debo ver mensaje de exito en categoria |

@ui @category @delete @happy
Scenario: CC-03 - Eliminar categoria
  Given existe una categoria creada con nombre "CategoriaParaEliminar"
  When hago clic en eliminar categoria "CategoriaParaEliminar"
  Then la categoria "CategoriaParaEliminar" no debe aparecer en la lista

@ui @category @read @happy
Scenario: CC-04 - Listar categorias activas
  Given existen las siguientes categorias en el sistema:
    | Nombre      |
    | Categoria A |
    | Categoria B |
    | Categoria C |
  When navego a la pagina de categorias
  Then debo ver al menos 3 categorias en la lista
