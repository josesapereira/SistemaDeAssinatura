IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SincronizarRegistrosAbility]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_SincronizarRegistrosAbility]
GO

CREATE PROCEDURE [dbo].[sp_SincronizarRegistrosAbility]
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[RegistroAbility]
    (
     [Id],
     [RE],
     [Nome],
     [CentroDeCusto],
     [Cargo],
     [CargoId],
     [Coordenador],
     [DatadeDemissao],
     [DatadeInclusao],
     [Departamento],
     [Gerente],
     [RECoordenador],
     [REGerente],
     [RESupervisor],
     [ReTelefonica],
     [Setor],
     [Supervisor]
    )
    SELECT 
     NEWID(),
     RB.Registro,
     RB.Nome,
     (ISNULL(CAST(RB.Depart AS VARCHAR(100)), '') 
       + ISNULL(CAST(RIGHT('00' + LTRIM(RTRIM(RB.Setor)), 2) AS VARCHAR(100)), '')
     ) AS CentroDeCusto,
     RB.Nome_Cargo,
     RB.cargo,
     '' AS Coordenador,
     RB.Demissao_Data,
     GETDATE() AS DatadeInclusao,
     RB.Nome_Depart,
     '' AS Gerente,
     '' AS RECoordenador,
     '' AS REGerente,
     '' AS RESupervisor,
     RB.Re_Telefonica,
     RB.Nome_Setor,
     '' AS Supervisor
    FROM 
        [172.30.5.3].[Ability].[dbo].RH_ABILITY RB
    LEFT JOIN 
        [RegistroAbility] RA
            ON RA.RE = RB.Registro
    WHERE 
        RB.Condicao_Func = 'A'
        AND RA.RE IS NULL;

    RETURN @@ROWCOUNT;
END
GO

