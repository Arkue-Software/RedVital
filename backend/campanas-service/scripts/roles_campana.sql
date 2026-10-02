-- RedVital — Servicio de Campañas
-- T-312.1. Mismo patrón de tres roles que roles_identidad.sql.

SELECT format('CREATE ROLE campana_propietario WITH LOGIN PASSWORD %L NOSUPERUSER', :'password_propietario')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'campana_propietario')
\gexec
GRANT ALL PRIVILEGES ON DATABASE db_campana TO campana_propietario;
GRANT ALL PRIVILEGES ON SCHEMA public TO campana_propietario;

SELECT format('CREATE ROLE campana_servicio WITH LOGIN PASSWORD %L NOSUPERUSER NOCREATEDB NOCREATEROLE', :'password_servicio')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'campana_servicio')
\gexec
GRANT CONNECT ON DATABASE db_campana TO campana_servicio;
GRANT USAGE ON SCHEMA public TO campana_servicio;

-- Entidades mutables. Sin borrado físico, igual que en el resto del sistema.
SELECT format('GRANT SELECT, INSERT, UPDATE ON TABLE %s TO campana_servicio', nombre)
FROM unnest(ARRAY['public.campania', 'public.reserva_cupo']) AS tablas(nombre)
WHERE to_regclass(nombre) IS NOT NULL
\gexec

-- Serie de auditoría: solo anexado, aplicado a nivel de motor.
SELECT 'GRANT SELECT, INSERT ON TABLE public.registro_auditoria_camp TO campana_servicio'
WHERE to_regclass('public.registro_auditoria_camp') IS NOT NULL
\gexec
SELECT 'REVOKE UPDATE, DELETE ON TABLE public.registro_auditoria_camp FROM campana_servicio'
WHERE to_regclass('public.registro_auditoria_camp') IS NOT NULL
\gexec

GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO campana_servicio;
