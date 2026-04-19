-- ============================================================
--  KURS — Script de inicialización Oracle
--  Ejecutar como SYSDBA o con privilegios CREATE USER
-- ============================================================

-- 1. Crear usuario/esquema (ajusta la contraseña)
-- ---------------------------------------------------------------
CREATE USER KURS_USER IDENTIFIED BY "TuPasswordSegura123"
    DEFAULT TABLESPACE USERS
    TEMPORARY TABLESPACE TEMP;

GRANT CONNECT, RESOURCE TO KURS_USER;
GRANT CREATE SEQUENCE TO KURS_USER;
GRANT UNLIMITED TABLESPACE TO KURS_USER;


-- ============================================================
--  Conectar como KURS_USER y ejecutar el resto
-- ============================================================

-- 2. Secuencias para PKs (usadas por EF Core HiLo)
-- ---------------------------------------------------------------
CREATE SEQUENCE SEQ_CLIENTES
    START WITH 1
    INCREMENT BY 10
    NOCACHE
    NOCYCLE;

CREATE SEQUENCE SEQ_MENSAJES
    START WITH 1
    INCREMENT BY 10
    NOCACHE
    NOCYCLE;


-- 3. Tabla CLIENTES
-- ---------------------------------------------------------------
CREATE TABLE CLIENTES (
    ID              NUMBER(19)      NOT NULL,
    NOMBRE          VARCHAR2(200)   NOT NULL,
    EMAIL           VARCHAR2(320)   NOT NULL,
    EMPRESA         VARCHAR2(200),
    TELEFONO        VARCHAR2(30),
    PASSWORD_HASH   VARCHAR2(500),
    FECHA_REGISTRO  TIMESTAMP(3)    DEFAULT SYSTIMESTAMP NOT NULL,
    ACTIVO          NUMBER(1)       DEFAULT 1 NOT NULL,
    NIVEL           NUMBER(1)       DEFAULT 2 NOT NULL,
    CONSTRAINT PK_CLIENTES     PRIMARY KEY (ID),
    CONSTRAINT UQ_CLI_EMAIL    UNIQUE (EMAIL),
    CONSTRAINT CK_CLI_ACTIVO   CHECK (ACTIVO IN (0, 1)),
    CONSTRAINT CK_CLI_NIVEL    CHECK (NIVEL BETWEEN 1 AND 4)
);

COMMENT ON TABLE  CLIENTES         IS 'Clientes registrados en la plataforma KURS';
COMMENT ON COLUMN CLIENTES.ACTIVO  IS '1 = activo, 0 = baja lógica';
COMMENT ON COLUMN CLIENTES.NIVEL   IS '1=Visitante, 2=Cliente, 3=Colaborador, 4=Administrador';


-- 4. Tabla MENSAJES_CONTACTO
-- ---------------------------------------------------------------
CREATE TABLE MENSAJES_CONTACTO (
    ID          NUMBER(19)      NOT NULL,
    NOMBRE      VARCHAR2(200)   NOT NULL,
    EMAIL       VARCHAR2(320)   NOT NULL,
    ASUNTO      VARCHAR2(300)   NOT NULL,
    MENSAJE     VARCHAR2(4000)  NOT NULL,
    FECHA_ENVIO TIMESTAMP(3)    DEFAULT SYSTIMESTAMP NOT NULL,
    LEIDO       NUMBER(1)       DEFAULT 0 NOT NULL,
    CONSTRAINT PK_MENSAJES   PRIMARY KEY (ID),
    CONSTRAINT CK_MSG_LEIDO  CHECK (LEIDO IN (0, 1))
);

COMMENT ON TABLE  MENSAJES_CONTACTO        IS 'Mensajes enviados desde el formulario de contacto';
COMMENT ON COLUMN MENSAJES_CONTACTO.LEIDO  IS '0 = no leído, 1 = leído por el equipo KURS';


-- ============================================================
--  Verificar
-- ============================================================
SELECT TABLE_NAME FROM USER_TABLES ORDER BY 1;
SELECT SEQUENCE_NAME FROM USER_SEQUENCES ORDER BY 1;
