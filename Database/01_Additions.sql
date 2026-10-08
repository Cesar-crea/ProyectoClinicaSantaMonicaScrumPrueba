USE DB_SantaMonica;
GO
IF OBJECT_ID(N'dbo.Historial_Usuario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Historial_Usuario (
        Id_Historial BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Historial_Usuario PRIMARY KEY,
        Id_Usuario BIGINT NOT NULL,
        Accion NVARCHAR(50) NOT NULL,
        Modulo NVARCHAR(80) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        Fecha DATETIME2 NOT NULL CONSTRAINT DF_Historial_Usuario_Fecha DEFAULT GETDATE(),
        Ip NVARCHAR(45) NULL
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Historial_Usuario_Usuario_Fecha' AND object_id=OBJECT_ID('dbo.Historial_Usuario'))
    CREATE INDEX IX_Historial_Usuario_Usuario_Fecha ON dbo.Historial_Usuario(Id_Usuario, Fecha);
GO
-- Reglas de integridad recomendadas. Ejecutar solo si no existen ya en la BD.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Paciente_Dni' AND object_id=OBJECT_ID('dbo.Paciente'))
    CREATE UNIQUE INDEX UX_Paciente_Dni ON dbo.Paciente(Dni);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Medico_Dni' AND object_id=OBJECT_ID('dbo.Medico'))
    CREATE UNIQUE INDEX UX_Medico_Dni ON dbo.Medico(Dni);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Medico_CMP' AND object_id=OBJECT_ID('dbo.Medico'))
    CREATE UNIQUE INDEX UX_Medico_CMP ON dbo.Medico(Nro_Colegiatura);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Cita_Medico_Fecha_Hora_Activa' AND object_id=OBJECT_ID('dbo.Cita'))
    CREATE UNIQUE INDEX UX_Cita_Medico_Fecha_Hora_Activa ON dbo.Cita(Id_Medico, Fecha, Hora) WHERE Estado <> 'CANCELADO';
GO
