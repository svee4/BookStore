
$port = 9000

function Make-Get {
	param (
		[string]$Path,
		[hashtable]$Parameters
	)

	$uri = "http://localhost:$port$Path"
	Write-Output $uri

	$params = @{}
	if ($Parameters) {
		$params["Body"] = $Parameters
	}

	Invoke-WebRequest `
		-Uri $uri `
		-Method GET `
		-Headers @{ "accept" = "application/json" } `
		-SkipHttpErrorCheck `
		@params
}

function Make-Request {
	param (
		[string]$Path,
		[string]$Method,
		[hashtable]$Body
	)

	$uri = "http://localhost:$port$Path"
	Write-Host $uri

	$params = @{}
	if ($Body) {
		$params.Body = ($Body | ConvertTo-Json -Depth 5)
	}

	Invoke-WebRequest `
		-Uri $uri `
		-Method $Method `
		-ContentType "application/json" `
		-Headers @{ "accept" = "application/json" } `
		-SkipHttpErrorCheck `
		@params
}

function Convert-ResponseToHashTable {
	param (
		[Parameter(ValueFromPipeline)]
		[object]$Response
	)

	$Response | ConvertFrom-Json -Depth 5 -AsHashTable
}

function Assert-PartialMatch {
	param (
		[Parameter(ValueFromPipeline)]
		[hashtable]$Actual,
		[hashtable]$Expected
	)

	function Compare-HashTables {
		param (
			[hashtable]$Actual,
			[hashtable]$Expected
		)
	}

	foreach ($key in $Expected.Keys) {

		if (!$Actual.ContainsKey($key)) {
			Write-Host "Actual hashtable did not contain expected key '$key'" `
				-ForegroundColor Red
			Write-Host "Expected:" -ForegroundColor Blue
			Write-Host ($Expected | ConvertTo-Json -Depth 5)
			Write-Host "Actual:" -ForegroundColor Blue
			Write-Host ($Actual | ConvertTo-Json -Depth 5)
			throw
		}

		if ($Expected[$key] -is [hashtable]) {
			if (!($Actual[$key] -is [hashtable])) {
				Write-Host "Actual hashtable type of key '$key' was not hashtable" `
					-ForegroundColor Red
				Write-Host "Expected:" -ForegroundColor Blue
				Write-Host ($Expected | ConvertTo-Json -Depth 5)
				Write-Host "Actual:" -ForegroundColor Blue
				Write-Host ($Actual | ConvertTo-Json -Depth 5)
				throw
			}

			Assert-PartialMatch -Actual $Actual[$key] -Expected $Expected[$key]
			continue
		}

		if ($Expected[$key] -is [array]) {
			if (!($Actual[$key] -is [array])) {
				Write-Host "Actual hashtable type of key '$key' was not array" `
					-ForegroundColor Red
				Write-Host "Expected:" -ForegroundColor Blue
				Write-Host ($Expected | ConvertTo-Json -Depth 5)
				Write-Host "Actual:" -ForegroundColor Blue
				Write-Host ($Actual | ConvertTo-Json -Depth 5)
				throw
			}

			$convExpected = Convert-ArrayToHashTable -Array $Expected[$key]
			$convActaual = Convert-ArrayToHashTable -Array $Actual[$key]
			Assert-PartialMatch -Actual $convActual -Expected $convExpected
			continue
		}

		if (!$Expected[$key] -eq $Actual[$key]) {
			Write-Host "Actual hashtable value for '$key' was different from expected value" `
				-Foregroundcolor Red
			Write-Host "Expected:" -ForegroundColor Blue
			Write-Host $Expected[$key]
			Write-Host "Actual:" -ForegroundColor Blue
			Write-Host $Actual[$key]
			throw
		}
	}
}

function Assert-StatusCodeEquals {
	param (
		[Parameter(ValueFromPipeline)]
		[object]$Response,
		[int]$Expected
	)

	$status = $Response.StatusCode

	if ($status -ne $Expected) {
		Write-Host "Unexpected status code" -ForegroundColor Red
		Write-Host "Expected:" -ForegroundColor Blue
		Write-Host $Expected
		Write-Host "Actual:" -ForegroundColor Blue
		Write-Host $status
	}
}

function Convert-ArrayToHashTable {
	param (
		[array]$Array
	)

	$hashtable = @{}
	$index = 0
	foreach ($item in $Array) {
		$hashtable[$index] = $item
		$index++
	}
}

Make-Request -Path "/books" -Method "POST" -Body `
	@{
		title = "Harry Potter and the Philosophers stone"
		author = "J.K. Rowling"
		year = 1997
		publisher = "Bloomsbury (UK)"
		description = "A book about a wizard boy"
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected @{ id = 1 }


Make-Request -Path "/books" -Method "POST" -Body `
	@{
		title = "Old Testament"
		author = "Various"
		year = -165
		description = "A book about a wizard boy"
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected @{ id = 2 }


Make-Request -Path "/books" -Method "POST" -Body `
	@{
		title = "The Subtle Knife"
		author = "Philip Pullman"
		year = 1997
		publisher = "Scholastic Point"
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected @{ id = 3 }


Make-Request -Path "/books" -Method "POST" -Body `
	@{
		title = "Goosebumps: Beware, the Snowman"
		author = "R.L. Stine"
		year = 1997
		publisher = "Scholastic Point"
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected @{ id = 4 }


# missing title
Make-Request -Path "/books" -Method "POST" -Body `
	@{
		author = "Douglas Adams"
		year = 1979
		publisher = "Pan Books"
		description = "Originally a radio series"
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected @{ status = 400 }


# missing year
Make-Request -Path "/books" -Method "POST" -Body `
	@{
		title = "The Hitchhiker's Guide to the Galaxy"
		author = "Douglas Adams"
		publisher = "Pan Books"
		description = "Originally a radio series"
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected @{ status = 400 }


# empty author
Make-Request -Path "/books" -Method "POST" -Body `
	@{
		title = "The Hitchhiker's Guide to the Galaxy"
		author = ""
		year = 1979
		publisher = "Pan Books"
		description = "Originally a radio series"
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected @{ status = 400 }


# empty title
Make-Request -Path "/books" -Method "POST" -Body `
	@{
		title = ""
		author = "Douglas Adams"
		year = 1979
		publisher = "Pan Books"
		description = "Originally a radio series"
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected @{ status = 400 }


# non-integer year
Make-Request -Path "/books" -Method "POST" -Body `
	@{
		title = "The Hitchhiker's Guide to the Galaxy"
		author = "Douglas Adams"
		year = 1979.95
		publisher = "Pan Books"
		description = "Originally a radio series"
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected @{ status = 400 }


# bad year
Make-Request -Path "/books" -Method "POST" -Body `
	@{
		title = "The Hitchhiker's Guide to the Galaxy"
		author = "Douglas Adams"
		year = "asd"
		publisher = "Pan Books"
		description = "Originally a radio series"
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected @{ status = 400 }


# existing book
Make-Request -Path "/books" -Method "POST" -Body `
	@{
		title = "Harry Potter and the Philosophers stone"
		author = "J.K. Rowling"
		year = 1997
		publisher = "Bloomsbury (UK)"
		description = "A book about a wizard boy"
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected @{ status = 400 }


Make-Get -Path "/books"
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected `
	(Convert-ArrayToHashTable -Array `
		@(
			@{
				id = 1
				title = "Harry Potter and the Philosophers Stone"
				author =  "J.K. Rowling"
				year = 1997
				publisher = "Bloomsbury (UK)"
				description = "A book about a wizard boy"
			},
			@{
				id = 2
				title = "Old Testament"
				author =  "Various"
				year = -165
				publisher = $null
				description = "A holy book of Christianity and Jewish faith"
			},
			@{
				id = 3
				title = "The Subtle Knife"
				author =  "Philip Pullman"
				year = 1997
				publisher = "Scholastic Point"
				description = $null
			},
			@{
				id = 4
				title = "Goosebumps: Beware, the Snowman"
				author =  "R.L. Stine"
				year = 1997
				publisher = "Scholastic Point"
				description = $null
			}
		)
	)


# by author
Make-Get -Path "/books" -Parameters @{ author = "J.K. Rowling" }
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected `
	(Convert-ArrayToHashTable -Array `
		@(
			@{
				id = 1
				title = "Harry Potter and the Philosophers Stone"
				author =  "J.K. Rowling"
				year = 1997
				publisher = "Bloomsbury (UK)"
				description = "A book about a wizard boy"
			}
		)PartialMatch -Expected @{ status = 404 }
	)


# by year
Make-Get -Path "/books" -Parameters @{ year = 1997 }
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected `
	(Convert-ArrayToHashTable -Array `
		@(
			@{
				id = 1
				title = "Harry Potter and the Philosophers Stone"
				author =  "J.K. Rowling"
				year = 1997
				publisher = "Bloomsbury (UK)"
				description = "A book about a wizard boy"
			},
			@{
				id = 3
				title = "The Subtle Knife"
				author =  "Philip Pullman"
				year = 1997
				publisher = "Scholastic Point"
				description = $null
			},
			@{
				id = 4
				title = "Goosebumps: Beware, the Snowman"
				author =  "R.L. Stine"
				year = 1997
				publisher = "Scholastic Point"
				description = $null
			}
		)
	)



# by publisher (no books)
Make-Get -Path "/books" -Parameters @{ publisher = "Otava" }
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected `
	(Convert-ArrayToHashTable -Array `
		@()
	)


# by year and publisher
Make-Get -Path "/books" -Parameters @{
		publisher = "Scholastic point"
		year = 1997
	}
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected `
	(Convert-ArrayToHashTable -Array `
		@(
			@{
				id = 3
				title = "The Subtle Knife"
				author =  "Philip Pullman"
				year = 1997
				publisher = "Scholastic Point"
				description = $null
			},
			@{
				id = 4
				title = "Goosebumps: Beware, the Snowman"
				author =  "R.L. Stine"
				year = 1997
				publisher = "Scholastic Point"
				description = $null
			}
		)
	)


# get one book
Make-Get -Path "/books/1"
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected `
	@{
		id = 1
		title = "Harry Potter and the Philosophers Stone"
		author =  "J.K. Rowling"
		year = 1997
		publisher = "Bloomsbury (UK)"
		description = "A book about a wizard boy"
	}

# nonexistent book
Make-Get -Path "/books/0"
| Assert-StatusCodeEquals -Expected 404


# non integer id book
Make-Get -Path "/books/x"
| Assert-StatusCodeEquals -Expected 404



# non integer id book
Make-Get -Path "/books/1.5"
| Assert-StatusCodeEquals -Expected 404


# delete book
Make-Request -Path "/books/1" -Method "DELETE"
| Assert-StatusCodeEquals -Expected 204

# delete non integer id book
Make-Request -Path "/books/1.5" -Method "DELETE"
| Assert-StatusCodeEquals -Expected 404

# delete non integer id book
Make-Request -Path "/books/x" -Method "DELETE"
| Assert-StatusCodeEquals -Expected 404


Make-Get -Path "/books"
| Convert-ResponseToHashTable
| Assert-PartialMatch -Expected `
	(Convert-ArrayToHashTable -Array `
		@(
			@{
				id = 2
				title = "Old Testament"
				author =  "Various"
				year = -165
				publisher = $null
				description = "A holy book of Christianity and Jewish faith"
			},
			@{
				id = 3
				title = "The Subtle Knife"
				author =  "Philip Pullman"
				year = 1997
				publisher = "Scholastic Point"
				description = $null
			},
			@{
				id = 4
				title = "Goosebumps: Beware, the Snowman"
				author =  "R.L. Stine"
				year = 1997
				publisher = "Scholastic Point"
				description = $null
			}
		)
	)















