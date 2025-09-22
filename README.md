# UclePdf

Aplicaci�n WPF (.NET 9) para cargar un Excel de Pedidos, aplicar datos manuales por registro (Especie, Sexo, Raza, Edad) con filtros por fecha y generar vista previa.

Caracter�sticas actuales:
- Carga de Excel Pedidos con ClosedXML.
- Filtro por fecha "Desde" en lectura para mayor rendimiento.
- Vista previa ordenada por fecha descendente.
- Edici�n por registro: Especie (Felino/Canino/Otros), Sexo (Hembra/Macho), Raza, Edad (cantidad + unidad).
- Opci�n "Sin selecci�n" para dejar campos vac�os.
- Bot�n Confirmar selecci�n y Limpiar (con confirmaci�n).
- Spinner de carga (overlay) durante la lectura del Excel.

Requisitos:
- .NET 9 SDK

Ejecuci�n:
1. Restaurar paquetes NuGet (VS lo hace autom�ticamente / `dotnet restore`).
2. Compilar y ejecutar el proyecto WPF `UclePdf`.

Pendientes futuros:
- Persistencia de cambios.
- Exportaci�n a PDF/Excel con el layout final.
- Segundo y tercer Excel si el cliente lo requiere.
