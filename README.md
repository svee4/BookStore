# Book store project

Sample book store project

## Structure
### src
Contains the web api project structured in a VSA-fashion.
The `Features` folder contains the singular feature, `Books`, where you will find controllers for book-actions.
The `Infra` folder contains some startup code and middleware.
In the database folder, you will find the also simple EF core database models.

### tests
Contains tests for the api.

### Other files
- `.editorconfig` - my personal projects' editorconfig
- `Directory.build.props` - my personal projects' Directory.build.props
- `test.ps1` - runs a quick test against the api
- `resetdb.ps1` - removes and recreates the database

## Branches
The branch you are looking at is the Immediate.Handlers branch.
The code includes the app implemented with Immediate.Handlers, Immediate.Apis and Immediate.Validations.
For different implementations, check out other branches.

## Running the project
1. Make sure you have .NET 8 installed
2. Navigate to `./src/BookStore`
3. If `books.db` is not present in the project directory: run `dotnet tool install dotnet-ef` to install entity framework tools, and then run `dotnet ef database update` to create and migrate the database
4. Run `dotnet run` to run the application in a development environment
5. Navigate to `http://localhost:9000/` to browse the Swagger UI
