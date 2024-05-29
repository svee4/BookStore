
$prefix = "./src/BookStore"
if (Test-Path "$prefix/books.db") { Remove-Item "$prefix/books.db" }
if (Test-Path "$prefix/books.db-wal") { Remove-Item "$prefix/books.db-wal" }
if (Test-Path "$prefix/books.sb-shm") { Remove-Item "$prefix/books.db-shm" }
iex "cd $prefix && dotnet ef database update"
