# UclePdf

Aplicación WPF (.NET 9) para cargar un Excel de Pedidos, aplicar datos manuales por registro (Especie, Sexo, Raza, Edad) con filtros por fecha y generar vista previa.

Características actuales:
- Carga de Excel Pedidos con ClosedXML.
- Filtro por fecha "Desde" en lectura para mayor rendimiento.
- Vista previa ordenada por fecha descendente.
- Edición por registro: Especie (Felino/Canino/Otros), Sexo (Hembra/Macho), Raza, Edad (cantidad + unidad).
- Opción "Sin selección" para dejar campos vacíos.
- Botón Confirmar selección y Limpiar (con confirmación).
- Spinner de carga (overlay) durante la lectura del Excel.

Requisitos:
- .NET 9 SDK

Ejecución:
1. Restaurar paquetes NuGet (VS lo hace automáticamente / `dotnet restore`).
2. Compilar y ejecutar el proyecto WPF `UclePdf`.

Pendientes futuros:
- Persistencia de cambios.
- Exportación a PDF/Excel con el layout final.
- Segundo y tercer Excel si el cliente lo requiere.
