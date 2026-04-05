# API de Reportes para TrackHub

## Características Principales

- **Generación de Reportes Excel**: Exportar datos operacionales en formato Excel para análisis fácil
- **Reportes de Posición en Vivo**: Listado en tiempo real de todos los transportadores con sus ubicaciones actuales
- **Registros Históricos de Posición**: Consultar historial de posiciones de transportadores en rangos de tiempo específicos
- **Reportes de Geocercas**: Identificar transportadores actualmente dentro de límites geográficos definidos
- **Reportes de Eventos de Geocercas**: Rastrear historial de entrada/salida de geocercas con marcas de tiempo, duraciones y coordenadas
- **Filtrado por Grupos**: Reportes filtrados automáticamente por los grupos asignados al usuario
- **Interfaz API REST**: Endpoints simples y directos para recuperación de reportes
- **Arquitectura Escalable**: Diseñada para integrar tipos de reportes adicionales y fuentes de datos

---

## Inicio Rápido

### Requisitos Previos

- .NET 10.0 SDK
- PostgreSQL 14+
- TrackHub Authority Server ejecutándose (para autenticación)
- TrackHub Manager y Router APIs (para acceso a datos)

### Instalación

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/shernandezp/TrackHub.Reporting.git
   cd TrackHub.Reporting
   ```

2. **Configurar las conexiones** en `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "ManagerConnection": "Host=localhost;Database=trackhub_manager;Username=postgres;Password=yourpassword"
     },
     "GraphQL": {
       "RouterEndpoint": "https://localhost:5001/graphql"
     }
   }
   ```

3. **Iniciar la aplicación**:
   ```bash
   dotnet run --project src/Web
   ```

4. **Acceder a la documentación de la API** en `https://localhost:5001/swagger`

### Ejemplo de Llamada a la API

```bash
# Obtener reporte en vivo de todos los transportadores
curl -X GET "https://localhost:5001/api/reports/live" \
  -H "Authorization: Bearer {tu_token}" \
  -o reporte_vivo.xlsx
```

---

## Componentes y Recursos Utilizados

| Componente                | Descripción                                             | Documentación                                                                 |
|---------------------------|---------------------------------------------------------|-------------------------------------------------------------------------------|
| .NET Core                 | Plataforma de desarrollo para aplicaciones modernas     | [Documentación .NET Core](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |

---

## Descripción General

## Características Clave

La API de Reportes para TrackHub es una API REST que proporciona una interfaz para que TrackHub Web acceda a los datos de reportes. Está diseñada para ser escalable y flexible, permitiendo la integración de reportes adicionales de diferentes fuentes dentro del sistema. Los reportes se generan en formato Excel.

## Reportes Disponibles

- **Reporte en Línea**: Lista las unidades junto con su ubicación actual. Se filtra en función de los grupos asignados al usuario. 
- **Reporte de Posiciones**: Proporciona un registro de las posiciones de las unidades en un período especificado.
- **Unidades en Geocercas**: Identifica las unidades que se encuentran actualmente dentro de una geocerca.
- **Eventos de Geocercas**: Lista los eventos de entrada/salida de geocercas dentro de un rango de fechas especificado. Incluye nombre del transportador, nombre de la geocerca, marcas de tiempo de entrada/salida, tiempo total y coordenadas. Soporta filtrado opcional por transportador.

## Licencia

Este proyecto está bajo la Licencia Apache 2.0. Consulta el archivo [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.