-- ============================================================
--  KURS — Migración: agregar columna NIVEL a CLIENTES
--  Ejecutar como KURS_USER sobre la BD ya existente
-- ============================================================

-- 1. Agregar columna con default 2 (Cliente)
ALTER TABLE CLIENTES ADD (
    NIVEL NUMBER(1) DEFAULT 2 NOT NULL
    CONSTRAINT CK_CLI_NIVEL CHECK (NIVEL BETWEEN 1 AND 4)
);

COMMENT ON COLUMN CLIENTES.NIVEL IS '1=Visitante, 2=Cliente, 3=Colaborador, 4=Administrador';

-- 2. Establecer nivel 4 (Administrador) a tu cuenta
UPDATE CLIENTES SET NIVEL = 4 WHERE EMAIL = 'rodripontevillarroel@gmail.com';
COMMIT;

-- 3. Verificar
SELECT ID, NOMBRE, EMAIL, NIVEL FROM CLIENTES ORDER BY ID;
