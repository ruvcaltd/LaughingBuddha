Project structure is as follows:

LAF.Dtos
- Contains Data Transfer Objects (DTOs) used for communication between different layers of the application

LAF.DataAccess
- Responsible for database interactions using Entity Framework Core
- Contains models and DbContext for the LAF database

LAF.Services
- Contains business logic and services that interact with the DataAccess layer
- Responsible for mapping between DTOs and data models - use manual mapper classes only, no automapper or similar libraries. Mappers should be in a separate folder named "Mappers".

LAF.WebApi
- Exposes RESTful APIs for client applications to interact with the LAF system

LAF.Tests
- Contains unit tests for the services in LAF.Services
- Uses nUnit as the testing framework
- For mocking dependencies, use Moq library



Rules
1. Separation of Concerns
   - Each project should have a clear responsibility and should not overlap with others.
   - DTOs should only be used for data transfer and not contain business logic.
2. Dependency Management
   - LAF.WebApi can depend on LAF.Services and LAF.Dtos.
   - LAF.Services can depend on LAF.DataAccess and LAF.Dtos.
   - LAF.DataAccess should not depend on any other project.
3. Naming Conventions
   - Projects should follow PascalCase naming conventions.
   - Classes, methods, and properties should also follow PascalCase.
4. Code Quality
   - All code should adhere to SOLID principles.
   - Unit tests should be written for all service methods in LAF.Services.
   
