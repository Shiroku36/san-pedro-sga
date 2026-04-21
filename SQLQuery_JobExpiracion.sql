-- =============================================
-- Job diario: Deshabilitar trabajadores expirados
-- Ejecutar en SSMS conectado a 45.71.46.198
-- =============================================
USE Sgajcp;
GO

-- 1. Ejecutar una vez para deshabilitar los que ya expiraron
UPDATE dbo.Trabajador
SET Habilitado = 0
WHERE Expiración IS NOT NULL
  AND Expiración < CAST(GETDATE() AS date)
  AND Habilitado = 1;
GO

-- 2. Crear el Job de SQL Server Agent que se ejecute diario a las 00:01
USE msdb;
GO

-- Eliminar job si ya existe
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'Deshabilitar_Trabajadores_Expirados')
BEGIN
    EXEC sp_delete_job @job_name = N'Deshabilitar_Trabajadores_Expirados';
END
GO

-- Crear el job
EXEC sp_add_job
    @job_name = N'Deshabilitar_Trabajadores_Expirados',
    @enabled = 1,
    @description = N'Deshabilita trabajadores cuya fecha de expiración ya pasó';
GO

-- Agregar el paso (step)
EXEC sp_add_jobstep
    @job_name = N'Deshabilitar_Trabajadores_Expirados',
    @step_name = N'Actualizar_Habilitado',
    @subsystem = N'TSQL',
    @command = N'
        UPDATE dbo.Trabajador
        SET Habilitado = 0
        WHERE Expiración IS NOT NULL
          AND Expiración < CAST(GETDATE() AS date)
          AND Habilitado = 1;',
    @database_name = N'Sgajcp';
GO

-- Programar ejecución diaria a las 00:01
EXEC sp_add_jobschedule
    @job_name = N'Deshabilitar_Trabajadores_Expirados',
    @name = N'Diario_00_01',
    @freq_type = 4,          -- Diario
    @freq_interval = 1,      -- Cada 1 día
    @active_start_time = 100; -- 00:01:00
GO

-- Asignar al servidor local
EXEC sp_add_jobserver
    @job_name = N'Deshabilitar_Trabajadores_Expirados',
    @server_name = N'(local)';
GO

PRINT 'Job creado exitosamente. Se ejecutará cada día a las 00:01.';
