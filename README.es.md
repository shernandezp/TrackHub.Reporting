## Componentes y Recursos Utilizados

| Componente                | Descripción                                             | Documentación                                                                 |
|---------------------------|---------------------------------------------------------|-------------------------------------------------------------------------------|
| .NET Core                 | Plataforma de desarrollo para aplicaciones modernas     | [Documentación .NET Core](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |

# API de Reportes para TrackHub

## Características Clave

La API de Reportes para TrackHub es una API REST que proporciona una interfaz para que TrackHub Web acceda a los datos de reportes. Está diseñada para ser escalable y flexible, permitiendo la integración de reportes adicionales de diferentes fuentes dentro del sistema. Los reportes se generan en formato Excel.

## Reportes Disponibles

- **Reporte en Línea**: Lista las unidades junto con su ubicación actual. Se filtra en función de los grupos asignados al usuario. 
- **Reporte de Posiciones**: Proporciona un registro de las posiciones de las unidades en un período especificado.
- **Unidades en Geocercas**: Identifica las unidades que se encuentran actualmente dentro de una geocerca.

## Licencia

Este proyecto está bajo la Licencia Apache 2.0. Consulta el archivo [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para más información.