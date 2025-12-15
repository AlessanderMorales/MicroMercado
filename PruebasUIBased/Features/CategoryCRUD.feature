Feature: CRUD de Categorias - UI Testing con Pairwise
Como usuario del sistema
Quiero gestionar categorias
Para organizar los productos

Background:
  Given que la aplicacion esta en ejecucion
  And navego a la pagina de categorias

@ui @category @create @pairwise @pw-cat-01
Scenario Outline: PW-CAT-01 - Crear categoria con combinaciones Pairwise
  When hago clic en agregar nueva categoria
  And lleno el formulario de categoria con nombre "<Nombre>" y descripcion "<Descripcion>"
  And hago clic en guardar categoria
  Then <Resultado>

  Examples:
    | TestID | Nombre                   | Descripcion                              | Resultado                                 |
    | PW01   | CatPairwise01            | Descripcion valida normal                | debo ver mensaje de exito en categoria    |
    | PW02   | CatPairwise02            |                                          | debo ver mensaje de exito en categoria    |
    | PW03   | CatDescLarga03           | Esta es una descripcion muy larga que tiene muchos caracteres | debo ver mensaje de exito en categoria |
    | PW04   |                          | Descripcion sin nombre categoria         | debo ver error de validacion en categoria |

@ui @category @update @pairwise @pw-cat-02
Scenario Outline: PW-CAT-02 - Actualizar categoria con combinaciones Pairwise
  Given existe una categoria creada con nombre "CatBaseUpdate"
  When hago clic en editar categoria "CatBaseUpdate"
  And actualizo el formulario con nombre "<NuevoNombre>" y descripcion "<NuevaDescripcion>"
  And hago clic en guardar categoria
  Then <Resultado>

  Examples:
    | TestID | NuevoNombre        | NuevaDescripcion                         | Resultado                              |
    | PW05   | CatActualizada05   | Nueva descripcion valida actualizada     | debo ver mensaje de exito en categoria |
    | PW06   | CatBaseUpdate      | Mismo nombre diferente descripcion       | debo ver mensaje de exito en categoria |
    | PW07   | CatModificada07    |                                          | debo ver mensaje de exito en categoria |
    | PW08   | CatDescLargaUpd08  | Descripcion actualizada muy extensa para validar | debo ver mensaje de exito en categoria |

@ui @category @bva @bva-cat-01
Scenario: BVA-CAT-01 - Crear categoria con nombre de 1 caracter (minimo)
  When hago clic en agregar nueva categoria
  And lleno el formulario de categoria con nombre "A" y descripcion "Categoria con nombre minimo"
  And hago clic en guardar categoria
  Then debo ver mensaje de exito en categoria

@ui @category @bva @bva-cat-02
Scenario: BVA-CAT-02 - Crear categoria con nombre largo (Excede limite)
  When hago clic en agregar nueva categoria
  And lleno el formulario de categoria con nombre "CategoriaConNombreDeCincuentaCaracteresExac" y descripcion "Test de longitud"
  And hago clic en guardar categoria
  Then la creacion debe fallar

@ui @category @bva @bva-cat-03
Scenario: BVA-CAT-03 - Crear categoria con descripcion de 1 caracter (minimo)
  When hago clic en agregar nueva categoria
  And lleno el formulario de categoria con nombre "CatDescMinBVA" y descripcion "D"
  And hago clic en guardar categoria
  Then debo ver mensaje de exito en categoria

@ui @category @bva @bva-cat-04
Scenario: BVA-CAT-04 - Crear categoria con descripcion de 255 caracteres (maximo)
  When hago clic en agregar nueva categoria
  And lleno el formulario de categoria con nombre "CatDescMaxBVA" y descripcion "Esta descripcion tiene exactamente doscientos cincuenta y cinco caracteres para probar el limite maximo permitido en el campo de descripcion de categoria del sistema MicroMercado que estamos probando ahora mismo con pruebas automatizadas de Reqnroll"
  And hago clic en guardar categoria
  Then la creacion debe fallar

@ui @category @pe @pe-cat-01
Scenario: PE-CAT-01 - Crear categoria con nombre duplicado
  Given existe una categoria creada con nombre "CatDuplicadaPE"
  When hago clic en agregar nueva categoria
  And lleno el formulario de categoria con nombre "CatDuplicadaPE" y descripcion "Intentando duplicar"
  And hago clic en guardar categoria
  Then debo ver error de validacion en categoria

@ui @category @pe @pe-cat-02
Scenario: PE-CAT-02 - Eliminar categoria existente (borrado logico)
  Given existe una categoria creada con nombre "CatParaEliminarPE"
  When hago clic en eliminar categoria "CatParaEliminarPE"
  Then la categoria "CatParaEliminarPE" no debe aparecer en la lista

@ui @category @pe @pe-cat-03
Scenario: PE-CAT-03 - Listar categorias activas
  Given existen las siguientes categorias en el sistema:
    | Nombre         |
    | CatListadoA    |
    | CatListadoB    |
    | CatListadoC    |
  When navego a la pagina de categorias
  Then debo ver al menos 3 categorias en la lista

@ui @category @pe @pe-cat-04
Scenario: PE-CAT-04 - Actualizar categoria manteniendo nombre unico propio
  Given existe una categoria creada con nombre "CatMismoNombrePE"
  When hago clic en editar categoria "CatMismoNombrePE"
  And actualizo el formulario con nombre "CatMismoNombrePE" y descripcion "Solo cambio descripcion no nombre"
  And hago clic en guardar categoria
  Then debo ver mensaje de exito en categoria
