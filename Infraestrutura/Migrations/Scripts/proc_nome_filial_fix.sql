USE [SistemaDeNotaFiscal]
GO
/****** Object:  StoredProcedure [dbo].[proc_nome_filial]    Script Date: 06/01/2026 20:17:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- FIX: Corrigido aliases duplicados - NomeUnidade agora tem aliases únicos
-- =============================================
ALTER PROCEDURE [dbo].[proc_nome_filial]
	-- Add the parameters for the stored procedure here
	@CNPJCliente varchar(100), 
	@CNPJFilial varchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    --[proc_nome_filial] ''
select distinct top 1
    COALESCE(T7.NOME, ITEM.NOME) as 'NomeUnidade',  -- FIX: Usa T7.NOME (filial) ou ITEM.NOME como fallback, mantém nome único
    CLIENTE_R.IDENTIFICADOR as 'CnpjCliente',
    FILIAL_R.IDENTIFICADOR as 'CnpjUnidade'
from
    [192.168.8.13].BDEnterprise_TI.dbo.PROJETO,
    [192.168.8.13].BDEnterprise_TI.dbo.ITEM,
    [192.168.8.13].BDEnterprise_TI.dbo.ITEM T1,
    [192.168.8.13].BDEnterprise_TI.dbo.ITEM T7,
    [192.168.8.13].BDEnterprise_TI.dbo.ITEM T8,
    [192.168.8.13].BDEnterprise_TI.dbo.CLIENTE_R,
    [192.168.8.13].BDEnterprise_TI.dbo.FILIAL_R
where (ITEM.EXCLUIDO = 0) AND
    PROJETO.OID = ITEM.OID AND
    PROJETO.RCLIENTE = T1.OID AND
    PROJETO.RFILIAL = T7.OID AND
    ITEM.RESCOPO = T8.OID and 
    CLIENTE_R.OID = PROJETO.RCLIENTE and
    FILIAL_R.OID = PROJETO.RFILIAL and
    replace(replace(replace(CLIENTE_R.IDENTIFICADOR,'-',''),'/',''),'.','') = @CNPJCliente and 
    replace(replace(replace(FILIAL_R.IDENTIFICADOR,'-',''),'/',''),'.','') = @CNPJFilial

END


