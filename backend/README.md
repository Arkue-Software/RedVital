# Backend

El backend de RedVital se mantiene en repositorios de servicios independientes;
esta carpeta conserva solo la guia de entrada. No contiene un backend Spring
Boot monolitico que deba ejecutarse desde este repositorio.

| Servicio | Repositorio |
|---|---|
| Identidad | [identity-service](https://github.com/Arkue-Software/identity-service) |
| Campañas | [campaign-service](https://github.com/Arkue-Software/campaign-service) |
| Donación | [donation-service](https://github.com/Arkue-Software/donation-service) |
| Institucional | [Institutional-network-service](https://github.com/Arkue-Software/Institutional-network-service) |
| API Gateway | [api-gateway-apisix](https://github.com/Arkue-Software/api-gateway-apisix) |
| Bases y migraciones | [databases](https://github.com/Arkue-Software/databases) |

La base de datos local se levanta desde el repositorio `databases`. Cada servicio
con persistencia tiene una instancia independiente; las bases de Donación e
Institucional permanecen vacías hasta que sus equipos aprueben sus esquemas.
