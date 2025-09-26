-- Тестовый SQL запрос для демонстрации парсера
DECLARE @StartDate DATE = '2024-01-01'
DECLARE @EndDate DATE = '2024-12-31'
DECLARE @ManagerId INT = NULL
DECLARE @MinAmount DECIMAL(10,2) = 1000.00
DECLARE @Активен BIT = 1
DECLARE @CustomerName NVARCHAR(100) = 'Тестовый клиент'

SELECT 
    'Sample Data' as Info,
    @StartDate as StartDate,
    @EndDate as EndDate,
    @ManagerId as ManagerId,
    @MinAmount as MinAmount,
    @Активен as IsActive,
    @CustomerName as CustomerName
ORDER BY Info