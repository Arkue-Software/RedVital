-- RedVital — Servicio de Identidad
-- T-304.1 (roles base) y T-330.4 (permisos sobre registro_auditoria_ident)
--
-- Tres roles por base, igual que el resto del sistema:
--   1. Superusuario   — solo lo usa el guion de inicialización. No lo usa ningún servicio.
--   2. Propietario    — solo lo usan los trabajos de migración, respaldo y restauración.
--   3. Servicio       — con el que corre el Servicio de Identidad en tiempo de ejecución.
--
-- Ejecutar este script con el rol superusuario, antes de la primera migración.

-- ============================================================
-- 1. Rol propietario: dueño del esquema, usado solo por migraciones
-- ============================================================
SELECT format('CREATE ROLE identidad_propietario WITH LOGIN PASSWORD %L NOSUPERUSER', :'password_propietario')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'identidad_propietario')
\gexec
GRANT ALL PRIVILEGES ON DATABASE db_identidad TO identidad_propietario;
GRANT ALL PRIVILEGES ON SCHEMA public TO identidad_propietario;

-- ============================================================
-- 2. Rol de servicio: con el que corre el servicio en ejecución
--    Sin permiso de definición de esquema (no puede crear, alterar ni
--    borrar tablas). Esto es lo que impide que un error de código, o un
--    atacante que comprometa el servicio, modifique la estructura de la base.
-- ============================================================
SELECT format('CREATE ROLE identidad_servicio WITH LOGIN PASSWORD %L NOSUPERUSER NOCREATEDB NOCREATEROLE', :'password_servicio')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'identidad_servicio')
\gexec
GRANT CONNECT ON DATABASE db_identidad TO identidad_servicio;
GRANT USAGE ON SCHEMA public TO identidad_servicio;

-- Entidades mutables: inserción, lectura y actualización, pero NUNCA borrado físico.
-- El DD es explícito: no existe borrado físico de entidades del dominio.
SELECT format('GRANT SELECT, INSERT, UPDATE ON TABLE %s TO identidad_servicio', nombre)
FROM unnest(ARRAY[
    'public.usuario',
    'public.usuario_jurisdiccion',
    'public.sesion',
    'public.credencial_servicio'
]) AS tablas(nombre)
WHERE to_regclass(nombre) IS NOT NULL
\gexec

-- Catálogo: solo lectura para el servicio. Los roles no se dan de alta desde
-- el código, se siembran con la migración (ver HasData en el DbContext).
SELECT 'GRANT SELECT ON TABLE public.rol TO identidad_servicio'
WHERE to_regclass('public.rol') IS NOT NULL
\gexec

-- ------------------------------------------------------------
-- registro_auditoria_ident (T-330.4): la diferencia de permisos que hace
-- "solo anexado" una propiedad del motor de base de datos, no una promesa
-- de que el código nunca lo va a intentar.
--
-- Nótese la ausencia deliberada de UPDATE y DELETE frente a las entidades
-- de arriba. Si alguien agrega sin querer un método que intente modificar
-- un registro de auditoría, esto lo rechaza en producción, no solo en
-- revisión de código.
-- ------------------------------------------------------------
SELECT 'GRANT SELECT, INSERT ON TABLE public.registro_auditoria_ident TO identidad_servicio'
WHERE to_regclass('public.registro_auditoria_ident') IS NOT NULL
\gexec
SELECT 'REVOKE UPDATE, DELETE ON TABLE public.registro_auditoria_ident FROM identidad_servicio'
WHERE to_regclass('public.registro_auditoria_ident') IS NOT NULL
\gexec

-- Secuencias: el servicio necesita poder usarlas para generar identificadores,
-- pero no crearlas ni borrarlas (eso lo hace la migración, con el rol propietario).
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO identidad_servicio;

-- ============================================================
-- Verificación rápida tras ejecutar este script (correr como identidad_servicio):
--
--   INSERT INTO registro_auditoria_ident (...) VALUES (...);  -- debe funcionar
--   UPDATE registro_auditoria_ident SET resultado = 'permitido' WHERE id = ...;  -- debe fallar
--   DELETE FROM registro_auditoria_ident WHERE id = ...;  -- debe fallar
--
-- Si cualquiera de las dos últimas no falla, el script no se aplicó correctamente
-- o alguien sobrescribió los permisos después. Esto es parte del criterio de
-- cierre de T-330.4, no un paso opcional.
-- ============================================================
