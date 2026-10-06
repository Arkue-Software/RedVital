# RedVital

RedVital es una plataforma para coordinar el ciclo de vida de la donacion de
sangre, la trazabilidad de unidades, campañas y la operacion de bancos de sangre
en Colombia.

Este repositorio centraliza los contratos OpenAPI, documentacion de entrada y
configuracion compartida. El frontend y los servicios se mantienen en
repositorios propios bajo la organizacion [Arkue-Software](https://github.com/Arkue-Software).

## Repositorios

| Componente | Repositorio |
|---|---|
| Frontend | [frontend](https://github.com/Arkue-Software/frontend) |
| Servicio de Identidad | [identity-service](https://github.com/Arkue-Software/identity-service) |
| Servicio de Campañas | [campaign-service](https://github.com/Arkue-Software/campaign-service) |
| Servicio de Donación | [donation-service](https://github.com/Arkue-Software/donation-service) |
| Servicio Institucional | [Institutional-network-service](https://github.com/Arkue-Software/Institutional-network-service) |
| API Gateway | [api-gateway-apisix](https://github.com/Arkue-Software/api-gateway-apisix) |
| Bases y migraciones | [databases](https://github.com/Arkue-Software/databases) |
| Documentacion | [red-vital-documentacion](https://github.com/Arkue-Software/red-vital-documentacion) |

## Persistencia

Cada servicio con datos persistentes usa una instancia PostgreSQL independiente:
Identidad (`db_identidad`), Institucional (`db_institucional`), Campañas
(`db_campana`) y Donación (`db_donacion`). Las migraciones disponibles en esta
integracion cubren Identidad y Campañas; las bases de Donación e Institucional
se aprovisionan vacias, sin inventar tablas o roles pendientes de sus equipos.
Notificaciones no requiere una base de negocio propia.

El Compose canonico, los roles y las migraciones viven en el repositorio
`databases`; no uses una base compartida para ejecutar los servicios.

## Contratos y documentacion

Los contratos publicados estan en [`api/`](api/). Las decisiones
arquitectonicas, requisitos, operacion y evidencia de pruebas se mantienen en
el repositorio de documentacion enlazado arriba.

## Desarrollo local

Consulta el README de cada servicio y ejecuta el Compose de
`Arkue-Software/databases` para provisionar las bases aisladas.
