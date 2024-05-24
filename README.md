# Book store project

This repo contains a sample book store project for a technical assignment

## Structure
The web api project is structed in a VSA-fashion.
The `Features` folder contains the singular feature, `Books`, where you will find controllers for book-actions.
In this commit, the controllers are single top-level files in the feature folder, because they are very simple.
For more complex features, you would make subfolders for each sub-feature.

In the database folder, you will find the also simple EF core database.

### Other files
- .editorconfig - my personal projects' editorconfig
- Directory.build.props - my personal projects' Directory.build.props

## Commits
The commit you are looking at is the initial MVP commit. This commit has the minimal working sample.
The code includes the bare minimum error handling and features.
Further commits improve the code.

## Running the project
1. Make sure you have .NET 8 installed
2. If books.db is not present in the project directory: install `dotnet-ef` and run `dotnet ef database update`
3. Run `dotnet run`
4. Navigate to http://localhost:9000/ to browse the Swagger UI
