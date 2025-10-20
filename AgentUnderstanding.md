# LAF (Liquidity Allocation Framework) - Complete Project Understanding

## Project Overview

LAF is a **Multi-Fund Repo Trading & Cash Allocation Platform** designed for banks and investment managers to allocate and monitor daily fund cash across overnight repo trades while ensuring:

- No breach of pre-agreed liquidity limits with each counterparty
- Every fund's cash account starts and ends the day flat (via integration with Eagle)
- Full audit trail, regulatory transparency, and multi-fund visibility

## Architecture

### Solution Structure
```
LAF/
├── LAF.Server/           # .NET Core Web API Backend
│   ├── LAF.WebApi/      # RESTful API controllers
│   ├── LAF.Services/    # Business logic layer
│   ├── LAF.DataAccess/  # Entity Framework Core data layer
│   ├── LAF.Dtos/        # Data Transfer Objects
│   └── LAF.Tests/       # Unit tests (nUnit + Moq)
├── LAF.Database/        # SQL Server Database Project
└── LAF.Client/          # Angular Frontend
    └── web/             # Angular application
```

## Domain Model

### Core Entities

1. **Fund** - Legal funds managed by the firm
   - FundCode, FundName, CurrencyCode
   - IsActive status, audit fields

2. **RepoRate** - Pre-agreed repo terms with counterparties
   - CounterpartyId, EffectiveDate, RepoRate
   - TargetCircle (maximum liquidity in millions)
   - Used for compliance checking before trades

3. **RepoTrades** - Individual repo trades per fund
   - FundId, CounterpartyId, Direction (Borrow/Lend)
   - Notional, Rate, StartDate, EndDate, MaturityDate
   - Status, SecurityId, CollateralTypeId

4. **CashAccount** - Fund cash accounts for repo funding
   - AccountName, CurrencyCode, FundId

5. **Cashflow** - Actual cash movements
   - Linked to FundId and CashAccountId
   - Tracks inflows/outflows from settlements

### Database Views

- **vAvailableCash** - Real-time cash available per fund
- **vFundBalances** - Aggregated fund cash balances
- **vCounterpartyExposure** - Total exposure vs TargetCircle

## Business Rules

### TargetCircle Enforcement
Before booking a repo trade:
```sql
SUM(Notional) for (Counterparty, Date) + NewTrade.Notional <= TargetCircle * 1,000,000
```
- If exceeded → reject or require override authorization

### Fund Flatness
- End-of-day check ensures every fund's cash account = 0
- System proposes adjustments to rebalance

### Auditability
- Every insert/update logged with CreatedBy/ModifiedBy
- Full traceability for compliance

## Daily Workflow

1. **06:30 AM** - Eagle Feed Import (starting cash balances)
2. **07:00 AM** - RepoRate Load (daily rates and limits)
3. **08:00 AM** - Trader Allocation (allocate cash across funds)
4. **09:30 AM** - Trade Settlement (create cashflows)
5. **02:00 PM** - Adjustments (handle new opportunities)
6. **05:30 PM** - Reconciliation (ensure fund flatness)
7. **06:00 PM** - Eagle Posting (export closing entries)

## Technology Stack

### Backend (.NET Core)
- **Framework**: ASP.NET Core Web API
- **Data Access**: Entity Framework Core
- **Testing**: nUnit + Moq
- **Architecture**: Clean architecture with separation of concerns

### Database
- **Platform**: SQL Server
- **Project Type**: SQL Server Database Project (.sqlproj)
- **Key Features**: Views, constraints, indexes, audit columns

### Frontend (Angular)
- **Framework**: Angular (v20.3.6)
- **UI Components**: Ag-Grid Community/Enterprise
- **State Management**: NgRx Signals
- **Architecture**: Zoneless Angular application
- **API Client**: Auto-generated TypeScript client using NSwag

## Key Development Rules

### Server-Side Rules
1. **Separation of Concerns** - Clear project responsibilities
2. **Dependency Management** - Strict dependency hierarchy
3. **Manual Mapping** - Use manual mapper classes only (no AutoMapper)
4. **SOLID Principles** - All code must adhere to SOLID
5. **Unit Testing** - All service methods must have unit tests

### Client-Side Rules
1. **Ag-Grid** - Use Ag-Grid for data grids
2. **NgRx Signals** - Use for state management
3. **Zoneless** - Keep application zoneless
4. **TypeScript Client** - Auto-generate using NSwag from Swagger

## API Integration

### TypeScript Client Generation
```bash
# Install tool
dotnet tool install --global Nswag.ConsoleCore

# Generate client
nswag openapi2tsclient /input:https://localhost:7202/swagger/v1/swagger.json /output:src/app/api/client.ts
```

### Eagle Integration
- **Inbound**: Morning feed provides starting cash balances
- **Outbound**: Evening export posts end-of-day flat balances

## Critical Constraints

1. **Liquidity Limits**: TargetCircle enforcement prevents over-exposure
2. **Fund Flatness**: End-of-day reconciliation ensures zero balances
3. **Audit Trail**: Complete traceability for regulatory compliance
4. **Multi-Currency**: Support for different fund currencies
5. **Real-time Updates**: Live cash position tracking

## Development Workflow

1. Database changes via SQL Server Database Project
2. Backend development following clean architecture
3. Frontend development with Angular and Ag-Grid
4. API client regeneration when backend changes
5. Unit testing for all service methods
6. Integration with Eagle for daily operations

This platform provides comprehensive repo trading capabilities while maintaining strict regulatory compliance and operational efficiency.