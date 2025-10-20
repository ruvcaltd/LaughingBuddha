CREATE VIEW [dbo].[vw_ActiveCounterparties] AS
SELECT 
    Id,
    CounterpartyCode,
    CounterpartyName,
    CounterpartyType,
    LegalEntityIdentifier,
    CountryCode,
    Region,
    CreditRating,
    CreditLimit
FROM Counterparty
WHERE IsActive = 1;
