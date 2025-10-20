open cmd in the LAF.DataAccess directory and run the following:

dotnet ef dbcontext scaffold "Server=.;Database=LAF;Trusted_Connection=true;TrustServerCertificate=true;" --project LAF.DataAccess.csproj  Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --context-dir Data --context LAFDbContext --force