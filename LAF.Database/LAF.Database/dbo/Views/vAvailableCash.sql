
    CREATE VIEW dbo.vAvailableCash AS
    SELECT
        ca.Id AS CashAccountId,
        ca.AccountName,
        ca.CurrencyCode,
        ca.FundId,
        f.FundCode,
        f.FundName,
        SUM(cf.Amount) AS AvailableBalance
    FROM dbo.CashAccount ca
    LEFT JOIN dbo.Cashflow cf ON ca.Id = cf.CashAccountId
    LEFT JOIN dbo.Fund f ON ca.FundId = f.Id
    GROUP BY ca.Id, ca.AccountName, ca.CurrencyCode, ca.FundId, f.FundCode, f.FundName;
