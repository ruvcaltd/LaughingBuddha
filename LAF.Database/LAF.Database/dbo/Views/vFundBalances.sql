   CREATE VIEW dbo.vFundBalances AS
    SELECT
        f.Id AS FundId,
        f.FundCode,
        f.FundName,
        f.CurrencyCode,
        ISNULL(SUM(cf.Amount), 0.0) AS AvailableBalance
    FROM dbo.Fund f
    LEFT JOIN dbo.CashAccount ca ON ca.FundId = f.Id
    LEFT JOIN dbo.Cashflow cf ON cf.CashAccountId = ca.Id
    GROUP BY f.Id, f.FundCode, f.FundName, f.CurrencyCode;
