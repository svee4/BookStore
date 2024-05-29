# Book store project

This repo contains a sample book store project

## Structure
### src
Contains the web api project is structed in a VSA-fashion.
The `Features` folder contains the singular feature, `Books`, where you will find controllers for book-actions.
The `Infra` folder contains only some startup code for now.
In the database folder, you will find the also simple EF core database.

### tests
Contains tests for the api

### Other files
- .editorconfig - my personal projects' editorconfig
- Directory.build.props - my personal projects' Directory.build.props
- test.ps1 - runs a quick test against the api
- resetdb.ps1 - removes and recreates the database

## Commits
The commit you are looking at is the Immediate.Handlers commit.
The code includes the app implemented with Immediate.Handlers, Immediate.Apis and Immediate.Validations.

## Running the project
1. Make sure you have .NET 8 installed
2. If books.db is not present in the project directory: install `dotnet-ef` and run `dotnet ef database update`
3. Run `dotnet run`
4. Navigate to http://localhost:9000/ to browse the Swagger UI
