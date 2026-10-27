USE [BaranggayInformationSystem];
GO

ALTER TABLE dbo.residents
ADD is_archived BIT NOT NULL
    CONSTRAINT df_residents_is_archived DEFAULT 0;
GO