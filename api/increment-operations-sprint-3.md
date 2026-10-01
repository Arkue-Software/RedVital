# Operaciones del incremento — Sprint 3

**Proyecto:** RedVital  
**Sprint:** 3  
**Historia:** HU-301  
**Tarea:** T-301.1  
**Responsable:** Sara   
**Fuente:** Documento de Diseño (DD) V2.0  

---

## 1. Propósito

Este documento extrae del DD V2.0 las operaciones que forman parte del incremento funcional del Sprint 3.

Su objetivo es definir la superficie de API que posteriormente será formalizada mediante contratos OpenAPI para los servicios de:

- Identidad.
- Donación.
- Campañas.

Para cada operación se identifican:

- método y ruta;
- propósito;
- acceso;
- errores relevantes;
- degradación o comportamiento ante indisponibilidad.

Este documento no sustituye al DD V2.0. El DD continúa siendo la fuente normativa del diseño.

---

## 2. Convenciones comunes

### 2.1 Rutas

Las operaciones expuestas a la aplicación utilizan rutas versionadas:

`/v1/...`

La aplicación web accede mediante:

`/api/v1/...`

El API Gateway elimina el prefijo `/api` antes de enviar la petición al servicio correspondiente.

### 2.2 Autenticación

Las operaciones protegidas utilizan:

`Authorization: Bearer <token>`

El gateway valida el token y el servicio destino vuelve a validarlo.

Las operaciones públicas no requieren token.

### 2.3 Correlación

Toda petición y respuesta utiliza:

`X-Correlacion-Id`

Si el cliente no proporciona el identificador, el gateway lo genera.

### 2.4 Idempotencia

Las operaciones marcadas como idempotentes requieren:

`Idempotency-Key`

Un reenvío con la misma clave debe devolver el resultado original sin crear nuevamente el recurso.

### 2.5 Plazos entre servicios

Las llamadas entre servicios tienen un plazo máximo de 3 segundos.

Cuando se supera el plazo, el consumidor aplica la degradación declarada y no inventa valores por defecto.

### 2.6 Errores

Los errores siguen RFC 9457 e incluyen:

- `tipo`
- `titulo`
- `estado`
- `detalle`
- `correlacion_id`

Los mensajes de error no contienen datos personales, clínicos ni contenido sensible del recurso.

---

## 3. Catálogo de errores

| HTTP | Tipo | Uso |
|---|---|---|
| 400 | `peticion-invalida` | Esquema o valor incorrecto |
| 401 | `sesion-invalida` | Token o credencial inválida |
| 403 | `operacion-no-permitida` | Rol sin autorización |
| 403 | `fuera-de-jurisdiccion` | Ámbito solicitado fuera de la jurisdicción |
| 404 | `no-encontrado` | Recurso inexistente dentro del alcance |
| 409 | `conflicto-de-estado` | El estado actual no permite la operación |
| 413 | — | Cuerpo superior a 1 MiB |
| 422 | `regla-de-negocio` | Violación de una regla del dominio |
| 429 | `demasiadas-peticiones` | Límite de tasa superado |
| 503 | `servicio-no-disponible` | Dependencia requerida no disponible |

---

# 4. Servicio de Identidad

## ID-01 — Inicio de sesión

**Operación**

`POST /v1/sesiones`

**Propósito**

Iniciar una sesión de usuario.

**Acceso**

Público.

**Comportamiento**

Devuelve un token de acceso y establece una cookie de renovación.

La cookie debe ser:

- `HttpOnly`
- `Secure`
- `SameSite=Strict`
- limitada a `/api/v1/sesiones`

El fallo de autenticación no distingue entre correo inexistente y credencial incorrecta.

**Errores relevantes**

- 400
- 401
- 413
- 429

**Degradación**

No se declara degradación funcional específica. Ante un fallo de autenticación, la operación falla de forma cerrada.

---

## ID-02 — Renovación de sesión

**Operación**

`POST /v1/sesiones/renovacion`

**Propósito**

Renovar el token de acceso.

**Acceso**

Público con cookie de renovación.

**Comportamiento**

La renovación rota el secreto de renovación.

La reutilización de un secreto ya utilizado provoca la revocación de la sesión.

**Errores relevantes**

- 400
- 401
- 429

**Degradación**

Si la renovación no puede validarse, no se emite un nuevo token.

---

## ID-03 — Término de sesión

**Operación**

`DELETE /v1/sesiones/actual`

**Propósito**

Cerrar la sesión actual.

**Acceso**

Usuario autenticado.

**Comportamiento**

Revoca la sesión de renovación.

El token de acceso previamente emitido permanece válido hasta su expiración.

**Errores relevantes**

- 401
- 403
- 429

**Degradación**

No se declara degradación específica.

---

## ID-04 — Publicación de claves públicas

**Operación**

`GET /.well-known/jwks.json`

**Propósito**

Publicar las claves públicas utilizadas para validar tokens.

**Acceso**

Componentes de la red de aplicación.

No se enruta mediante el API Gateway.

**Respuesta**

Conjunto de claves conforme a RFC 7517 identificadas mediante `kid`.

**Consumo**

- caché de 10 minutos;
- nueva consulta ante un `kid` desconocido;
- máximo una consulta cada 30 segundos.

**Degradación**

Si un componente no dispone de las claves necesarias para validar un token, rechaza las peticiones que requieren sesión.

El comportamiento es de fallo cerrado.

---

# 5. Servicio de Donación

## DON-01 — Búsqueda de donante

**Operación**

`POST /v1/donantes/busqueda`

**Propósito**

Identificar al donante presente en la institución.

**Acceso**

U3 — Operador de banco.

**Entrada**

El documento se envía en el cuerpo y nunca en la URL.

**Respuesta**

- identificador;
- nombre;
- elegibilidad.

**Errores relevantes**

- 400
- 401
- 403
- 404
- 413
- 429

**Degradación**

No se declara dependencia externa.

---

## DON-02 — Registro de donación

**Operación**

`POST /v1/donaciones`

**Propósito**

Registrar una donación.

**Acceso**

U3 — Operador de banco.

**Idempotencia**

Requiere:

`Idempotency-Key`

**Comportamiento**

Crea en una sola transacción:

- la donación;
- las unidades correspondientes;
- el evento inicial de cada unidad.

Las unidades nacen en estado:

`captada`

La institución se obtiene del token.

**Dependencia externa**

Servicio de Campañas.

**Degradación**

Si Campañas no responde en 3 segundos:

1. la donación se registra;
2. `campania_id` queda en `null`;
3. la interfaz informa que la campaña no está disponible;
4. no se asigna ningún valor por defecto.

**Errores relevantes**

- 400
- 401
- 403
- 404
- 422
- 429

---

## DON-03 — Ingreso a tamizaje

**Operación**

`POST /v1/unidades/{id}/ingreso-tamizaje`

**Propósito**

Registrar el ingreso de una unidad al proceso de tamizaje.

**Acceso**

U3.

**Transición**

`fraccionada -> en_tamizaje`

**Errores relevantes**

- 400
- 401
- 403
- 404
- 409
- 429

---

## DON-04 — Registro de tamizaje

**Operación**

`POST /v1/unidades/{id}/tamizaje`

**Propósito**

Registrar el veredicto de tamizaje.

**Acceso**

U3.

**Entrada**

El cuerpo contiene únicamente:

`apta: boolean`

No admite campos adicionales.

**Transiciones**

`en_tamizaje -> disponible`

o

`en_tamizaje -> no_apta`

**Errores relevantes**

- 400
- 401
- 403
- 404
- 409
- 429

---

## DON-05 — Consulta de existencias

**Operación**

`GET /v1/inventario`

**Propósito**

Consultar existencias disponibles.

**Acceso**

- U3
- U4
- U5
- U6

**Comportamiento**

El inventario se calcula como proyección sobre las unidades.

Solo se contabilizan unidades cuyo estado tenga:

`cuenta_disponible = true`

El resultado se filtra por jurisdicción.

**Errores relevantes**

- 400
- 401
- 403
- 429

---

## DON-06 — Despacho

**Operación**

`POST /v1/unidades/{id}/despacho`

**Propósito**

Registrar el despacho de una unidad.

**Acceso**

U3.

Solo la institución custodia puede realizar la operación.

**Entrada**

Requiere:

`confirmacion`

**Transición**

`reservada -> despachada`

**Errores relevantes**

- 400
- 401
- 403
- 404
- 409
- 422
- 429

---

## DON-07 — Disposición final

**Operación**

`POST /v1/unidades/{id}/disposicion-final`

**Propósito**

Registrar la disposición final de una unidad.

**Acceso**

U3.

**Entrada**

Requiere:

`confirmacion`

**Transiciones**

`no_apta -> desechada`

`vencida -> desechada`

**Errores relevantes**

- 400
- 401
- 403
- 404
- 409
- 422
- 429

---

## DON-08 — Eventos de unidad

**Operación**

`GET /v1/unidades/{id}/eventos`

**Propósito**

Consultar la trazabilidad completa de una unidad.

**Acceso**

U3 a U7.

**Respuesta**

Historial con:

- actor;
- marca temporal;
- estado;
- observación de catálogo.

**Errores relevantes**

- 400
- 401
- 403
- 404
- 429

---

# 6. Servicio de Campañas

## CAM-01 — Consulta de campañas

**Operación**

`GET /v1/campanias`

**Propósito**

Consultar campañas visibles.

**Acceso**

Público y U2 a U6.

**Comportamiento**

Para público y donantes:

- solo campañas publicadas;
- filtradas por municipio o departamento.

Para roles institucionales:

- campañas de su jurisdicción;
- en los estados permitidos.

**Errores relevantes**

- 400
- 401
- 403
- 429

---

## CAM-02 — Detalle de campaña

**Operación**

`GET /v1/campanias/{id}`

**Propósito**

Consultar el detalle de una campaña y permitir a Donación confirmar la campaña asociada.

**Acceso**

U2 a U6 y Servicio de Donación.

**Respuesta**

- `id`
- `nombre`
- `sede`
- `inicia_en`
- `termina_en`
- `institucion_id`
- `territorio_ruta`
- `estado`

**Errores relevantes**

- 404
- 503

**Degradación**

Si Campañas no responde durante el registro de una donación:

- la donación continúa;
- `campania_id` queda en `null`;
- no se inventa una campaña.

---

## CAM-03 — Publicación de campaña

**Operación**

`POST /v1/campanias/{id}/publicacion`

**Propósito**

Publicar una campaña.

**Acceso**

U4 — Administrador de banco.

**Comportamiento**

La acción queda auditada.

**Errores relevantes**

- 400
- 401
- 403
- 404
- 409
- 429

---

## CAM-04 — Cierre de campaña

**Operación**

`POST /v1/campanias/{id}/cierre`

**Propósito**

Cerrar una campaña.

**Acceso**

U4 — Administrador de banco.

**Errores relevantes**

- 400
- 401
- 403
- 404
- 409
- 429

---

# 7. Operaciones que pasan a OpenAPI

## Identidad

- `POST /v1/sesiones`
- `POST /v1/sesiones/renovacion`
- `DELETE /v1/sesiones/actual`
- `GET /.well-known/jwks.json`

## Donación

- `POST /v1/donantes/busqueda`
- `POST /v1/donaciones`
- `POST /v1/unidades/{id}/ingreso-tamizaje`
- `POST /v1/unidades/{id}/tamizaje`
- `GET /v1/inventario`
- `POST /v1/unidades/{id}/despacho`
- `POST /v1/unidades/{id}/disposicion-final`
- `GET /v1/unidades/{id}/eventos`

## Campañas

- `GET /v1/campanias`
- `GET /v1/campanias/{id}`
- `POST /v1/campanias/{id}/publicacion`
- `POST /v1/campanias/{id}/cierre`

---

# 8. Resultado de T-301.1

Con esta extracción quedan definidos los contratos que sirven como entrada para:

- T-301.2 — OpenAPI de Identidad.
- T-301.3 — OpenAPI de Donación.
- T-301.4 — OpenAPI de Campañas.
- T-301.5 — Publicación y validación de los contratos.