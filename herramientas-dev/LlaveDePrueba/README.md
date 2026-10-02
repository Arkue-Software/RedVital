# Llave de prueba para tokens firmados — T-303.1 / T-303.2

## Qué es esto

Un par de llaves RSA de 3072 bits, generado localmente, que sirve para emitir
tokens JWT de prueba con las ocho reivindicaciones de ADR-008, **sin esperar**
a que el Servicio de Identidad emita tokens reales (T-304.3). Con esto,
cualquiera del equipo puede probar su propio componente contra un token válido
de U3 (operador), U4 (administrador de banco) o un token de servicio.

## Por qué existe aparte de la emisión real

El Servicio de Identidad todavía no emite tokens — depende de T-304.2, T-304.3
y de que el esquema de T-304.1 esté migrado. Mientras tanto, Damián y Tomás
necesitan tokens válidos para avanzar en sus propias tareas. Esta utilidad les
da eso sin bloquearlos.

## Cómo se genera

```bash
cd herramientas-dev/LlaveDePrueba
dotnet run -- generar-llave
```

Esto crea `llaves/dev-signing-key.private.pem` y `llaves/dev-signing-key.public.pem`
dentro de esta misma carpeta.

## Cómo se emite un token de prueba

```bash
# Token de operador de banco (U3)
dotnet run -- emitir-token --perfil operador

# Token de administrador de banco (U4)
dotnet run -- emitir-token --perfil admin_banco

# Token de servicio, para procesos sin usuario
dotnet run -- emitir-token --perfil servicio --cliente gateway
dotnet run -- emitir-token --perfil servicio --cliente donacion
dotnet run -- emitir-token --perfil servicio --cliente notificaciones
```

Cada comando imprime el token por salida estándar. Úsalo directamente en la
cabecera `Authorization: Bearer <token>` de cualquier llamada de prueba.

## Dónde se carga (T-303.2)

La llave **pública** (nunca la privada) se carga en el perfil de configuración
de **Desarrollo** de cada servicio que valida tokens, y en la configuración de
la suite de pruebas automatizadas. Concretamente:

| Dónde | Archivo | Qué se pone |
|---|---|---|
| Cada host .NET, en Desarrollo | `appsettings.Development.json` del proyecto ejecutable | Ruta a `dev-signing-key.public.pem`, bajo la clave `Jwt:LlavePublicaDesarrollo` |
| Host de Donación, en Desarrollo | Configuración de desarrollo del host | Misma ruta, nunca una copia de la llave |
| Suite de pruebas | Configuración de pruebas | Se referencia la llave local, nunca se copia ni se versiona |

En este repositorio, los hosts de Identidad y Campañas ya cargan esta
configuración en sus respectivos `appsettings.Development.json`. Desde la
carpeta del proyecto host, la ruta relativa a la llave es
`../../../../herramientas-dev/LlaveDePrueba/llaves/dev-signing-key.public.pem`.
La suite automatizada todavía no tiene un proyecto de pruebas que pueda
consumirla.

**No copies el contenido de la llave dentro de ningún archivo de
configuración.** Siempre se referencia por ruta de archivo, igual que se hace
con cualquier secreto en este proyecto — así, si el formato de la llave
cambia, se actualiza en un solo lugar.

Ejemplo de lo que debe quedar en `appsettings.Development.json` de cada
servicio .NET:

```json
{
  "Jwt": {
    "LlavePublicaDesarrollo": "../../../../herramientas-dev/LlaveDePrueba/llaves/dev-signing-key.public.pem",
    "Emisor": "https://identidad.redvital.local",
    "Audiencia": "redvital"
  }
}
```

## Advertencia que hay que dejar visible donde sea que se documente esto

> **Esta llave NO es secreta.** Cualquiera que obtenga el archivo privado
> local puede firmar tokens con ella. Los archivos de llave se excluyen de Git;
> son exclusivos de Desarrollo y de pruebas automatizadas. **La configuración
> de QA debe rechazar explícitamente cualquier token firmado con esta llave**:
> comprobarlo corresponde a T-303.3.

## Qué falta para que esto deje de ser necesario

Cuando T-304.3 (emisión real de tokens por el Servicio de Identidad) esté
terminada, esta utilidad sigue siendo útil para pruebas automatizadas y para
desarrollo local — no se reemplaza, convive con la emisión real. Lo único que
cambia es que QA y Producción usan la llave real, publicada por el propio
Servicio de Identidad vía JWKS, nunca esta.
