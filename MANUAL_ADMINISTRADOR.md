# Manual de Administrador SGA

Este manual resume el funcionamiento actual de la plataforma `SGA` según la interfaz visible en `http://localhost:3636/` y los módulos activos disponibles para administración.

## 1. Objetivo del sistema

SGA centraliza la operación interna en una sola plataforma. Hoy la app reúne estos módulos principales:

- `Predios`
- `RR.HH.`
- `Informes`
- `Contratistas`
- `Cuentas`
- `Casino`

Además, existen flujos complementarios que se usan desde esos módulos:

- `Personas`
- `Registros del día`
- `Registros por trabajador`

## 2. Ingreso y navegación principal

Al iniciar sesión, el sistema muestra un panel principal con accesos directos a los módulos más usados.

La barra superior concentra:

- logo y nombre del sistema
- navegación principal por módulos
- acceso al menú del usuario

### Menú del usuario

Desde el nombre del usuario, en la esquina superior derecha, se puede:

- abrir `Mi cuenta`
- cerrar sesión con `Salir`
- cambiar el tema visual

Temas disponibles:

- `Arena`
- `Glaciar`
- `Grafito`
- `Bosque`

La selección del tema queda guardada en el navegador.

## 3. Convenciones visuales y de uso

### Campos obligatorios

Los campos obligatorios muestran un `*` junto al label.

### Botones

La interfaz usa una lógica consistente:

- botones principales para crear, guardar o confirmar
- botones secundarios para volver, cancelar o limpiar
- botones de tabla para acciones puntuales como `Editar`, `Detalle`, `Eliminar`

### Búsqueda

Los listados principales usan buscadores amplios sobre la tabla, para filtrar rápidamente sin salir de la vista.

### Imágenes

En los módulos que muestran fotos, puedes hacer clic sobre la imagen para verla ampliada.

## 4. Módulos principales

## 4.1 Predios

Este módulo administra la estructura física de operación.

Desde `Predios` puedes:

- revisar el listado de predios
- buscar por nombre, lugar o encargado
- crear un nuevo predio
- editar un predio existente
- revisar el detalle de cada predio
- eliminar un predio si tu cuenta tiene ese permiso

### Datos manejados en Predios

Cada predio considera:

- nombre
- lugar
- coordenadas
- encargado
- asistente
- correo
- teléfono
- teléfono del asistente

### Uso recomendado

Usa este módulo para mantener actualizada la estructura territorial y los responsables operativos de cada predio.

## 4.2 RR.HH.

Este es el módulo central de trabajadores.

Desde `RR.HH.` puedes:

- revisar el listado general de trabajadores
- buscar por `RUT`, nombre o `UID`
- crear un nuevo trabajador
- editar fichas existentes
- revisar el detalle completo de cada trabajador
- eliminar trabajadores si el permiso lo permite
- entrar al historial de registros del trabajador
- abrir `Registros del día`

### Información que maneja la ficha de trabajador

La ficha permite administrar:

- identidad
- RUT
- número de pulsera
- contratista
- correo y teléfono
- dirección
- sexo
- estado habilitado / no habilitado
- fecha de expiración
- documentos contractuales
- documentos de seguridad
- fotos de referencia

### Documentos asociados

Entre los documentos que puede guardar una ficha están:

- nómina
- contrato
- anexo
- ODI
- registro EPP
- registro RIOHS
- registro de capacitación
- examen de altura
- procedimientos de trabajo
- documento Covid-19

### Uso recomendado

Usa `RR.HH.` como fuente principal para mantener actualizada la ficha laboral y documental de cada persona operativa.

## 4.3 Informes

Este módulo agrupa la salida documental del sistema.

Desde `Informes` puedes:

- generar informes por predio
- filtrar por contratista
- definir rango de fechas
- descargar en `PDF` o `Excel`
- abrir informes de `Horas extras`
- abrir informes de `Asistencias`

### Informe principal

El informe principal permite:

- seleccionar un predio
- seleccionar un contratista si corresponde
- definir `Inicio` y `Fin`
- elegir formato
- generar el archivo final

### Horas extras

El submódulo `Horas extras` sirve para:

- subir uno o más archivos `CSV`
- asignar un título al informe
- marcar `Horario diferido` cuando corresponda
- generar el archivo consolidado

### Asistencias mensuales

El submódulo `Asistencias` permite:

- definir el mes de referencia
- seleccionar todos los trabajadores
- desmarcar o marcar casos puntuales
- generar el documento mensual

### Uso recomendado

Usa `Informes` para todo lo que implique salida documental, consolidado operativo y respaldo para revisión o envío.

## 4.4 Contratistas

Este módulo administra empresas contratistas y su personal habilitado.

Desde `Contratistas` puedes:

- revisar el listado de contratistas
- buscar por nombre o RUT
- crear un nuevo contratista
- editar un contratista
- ver el detalle
- eliminar si aplica
- administrar `Habilitados`

### Datos de un contratista

Cada contratista considera:

- nombre
- RUT
- archivo de nómina

### Habilitados

La vista `Habilitados` permite:

- revisar los trabajadores asociados a un contratista
- buscar dentro del listado
- marcar o desmarcar personas
- usar `Seleccionar todos`
- guardar cambios en bloque

### Uso recomendado

Usa este módulo para controlar qué empresa existe en el sistema y qué trabajadores de esa empresa quedan activos para operación.

## 4.5 Cuentas

Este módulo administra accesos al sistema.

Desde `Cuentas` puedes:

- revisar el listado de usuarios
- buscar cuentas
- crear una nueva cuenta
- editar una cuenta existente
- revisar el detalle de una cuenta
- eliminar una cuenta si tienes permiso

### Qué administra una cuenta

Cada cuenta permite definir:

- usuario
- contraseña
- nombre
- apellido
- correo
- empresa o contratista asociado
- nivel
- permisos por módulo
- permisos por predio

### Permisos

La cuenta usa una matriz de acceso para decidir qué puede ver o hacer el usuario.

En esta sección se pueden configurar, entre otros:

- permisos de visualización
- permisos de creación
- permisos de edición
- permisos de eliminación
- acceso a módulos
- acceso por predio

### Cambio de contraseña

Desde la sección de seguridad de una cuenta se puede:

- ingresar la contraseña actual
- definir una nueva contraseña
- confirmar la nueva contraseña

### Uso recomendado

Usa `Cuentas` para abrir accesos nuevos, corregir permisos y mantener controlado qué puede hacer cada perfil.

## 4.6 Personas

`Personas` es un módulo administrativo complementario, accesible desde el sistema y también desde `Casino`.

Desde `Personas` puedes:

- revisar el listado de personas
- buscar por RUT, nombre o centro de costo
- crear una nueva persona
- editar registros
- revisar el detalle
- eliminar si corresponde
- cargar personas desde archivo

### Datos manejados en Personas

Cada registro considera:

- RUT
- nombre
- centro de costo 1
- centro de costo 2
- glosa de costo
- glosa de puesto
- ubicación
- instalación

### Carga masiva

La vista `Cargar personas` permite importar o actualizar registros desde archivo.

Según la interfaz actual, la carga espera datos como:

- centro de costo
- ubicación
- instalación
- RUT
- nombre completo
- glosa de puesto

### Uso recomendado

Usa `Personas` cuando necesites mantener la base administrativa que alimenta otros procesos, especialmente `Casino`.

## 4.7 Casino

`Casino` es un módulo operativo separado del resto, con identidad visual propia.

Desde `Casino` puedes:

- revisar consumos del día
- filtrar por fecha
- buscar por RUT, persona, servicio o ubicación
- exportar por rango de fechas
- abrir el módulo `Personas`
- revisar el detalle de un registro
- eliminar registros si el permiso existe

### Panel de consulta diaria

Permite cambiar el día visible del módulo.

### Panel de informe

Permite:

- definir fecha de inicio
- definir fecha de fin
- elegir `Excel` o `PDF`
- generar el informe

### Tabla de actividad

Muestra:

- RUT
- nombre
- fecha
- hora
- servicio
- ubicación
- foto
- acciones

### Uso recomendado

Usa `Casino` para revisión rápida de consumo diario, búsqueda operativa y descarga de respaldos por periodo.

## 4.8 Registros del día

Este flujo se abre desde `RR.HH.` y sirve para revisar actividad consolidada del día.

Desde `Registros del día` puedes:

- cambiar la fecha
- buscar por nombre, campo o contratista
- revisar la tabla de actividad
- abrir fotos cuando existan

La tabla actual muestra:

- fecha
- nombre
- hora
- campo
- contratista
- foto

## 4.9 Registros por trabajador

Desde la ficha o tabla de `RR.HH.` puedes abrir los registros de una persona específica.

Ahí puedes:

- revisar el historial de marcaciones
- crear un nuevo registro manual
- editar registros
- ver el detalle
- eliminar registros si el permiso lo permite

### Datos de un registro

Cada registro considera:

- fecha y hora
- campo
- nombre del trabajador
- foto opcional

## 5. Recomendaciones de uso para administrador

## 5.1 Mantención básica diaria

Como rutina de administración, conviene revisar:

1. `RR.HH.` para fichas nuevas o cambios pendientes.
2. `Registros del día` para actividad reciente.
3. `Casino` para revisar consumo y errores de captura.
4. `Cuentas` para accesos y permisos.

## 5.2 Mantención estructural

En una revisión más administrativa conviene mantener al día:

- `Predios`
- `Contratistas`
- `Personas`
- permisos de `Cuentas`

## 5.3 Antes de crear usuarios nuevos

Antes de abrir una cuenta, verifica:

- qué empresa quedará asociada
- qué predios debe ver
- qué módulos necesita usar
- si requiere permisos de edición o solo consulta

## 5.4 Antes de generar informes

Antes de descargar un informe, revisa:

- predio correcto
- contratista correcto
- fechas correctas
- formato correcto

## 6. Buenas prácticas

- Usa los buscadores antes de crear un registro nuevo para evitar duplicados.
- Revisa el detalle antes de editar o eliminar.
- En `Habilitados`, guarda los cambios después de una selección masiva.
- En `Cuentas`, no entregues más permisos de los necesarios.
- En `Casino`, usa la foto ampliada cuando necesites validar evidencia.

## 7. Qué revisar si algo “no se ve bien”

Si la interfaz no refleja un cambio reciente:

- recarga el navegador de forma completa
- vuelve a entrar al módulo
- revisa el tema visual seleccionado

Esto es especialmente importante cuando se cambian estilos, temas o componentes visuales.

## 8. Resumen rápido por módulo

- `Predios`: estructura física y responsables.
- `RR.HH.`: fichas de trabajadores y documentos.
- `Informes`: reportes por predio, contratista, fechas, horas y asistencias.
- `Contratistas`: empresas y habilitación masiva de personal.
- `Cuentas`: usuarios, accesos y permisos.
- `Personas`: base administrativa complementaria.
- `Casino`: consumos, búsqueda rápida, exportación y evidencia.
- `Registros`: historial y marcaciones por persona o por día.

## 9. Alcance de este manual

Este documento describe el comportamiento visible y operativo actual de la aplicación, según la versión revisada en navegador y los módulos activos expuestos hoy.

Si más adelante se agregan nuevos módulos, permisos o flujos, conviene actualizar este manual para que siga siendo útil como referencia real de administración.
