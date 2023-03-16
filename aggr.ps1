$folders = @(
    ".",
    "Services"
)

$aggregateFile = "aggregate.cs"
$separator = ">>> BREAK"
$lineCounter = 0
$breakLines = 300

# Remove the aggregate file if it exists
if (Test-Path $aggregateFile) {
    Remove-Item $aggregateFile
}

foreach ($folder in $folders) {
    $csFiles = Get-ChildItem -Path $folder -Filter *.cs -File

    foreach ($csFile in $csFiles) {
        $fileLines = Get-Content $csFile.FullName

        foreach ($fileLine in $fileLines) {
            # Trim whitespaces
            $trimmedLine = $fileLine.Trim()

            # Discard lines starting with "using" and ending with ";"
            if (-not ($trimmedLine.StartsWith("using") -and $trimmedLine.EndsWith(";"))) {
                # Replace newlines with spaces
                $spaceLine = $trimmedLine -replace '\r?\n', ' '
                Add-Content -Path $aggregateFile -Value $spaceLine
                $lineCounter++

                if ($lineCounter -eq $breakLines) {
                    Add-Content -Path $aggregateFile -Value $separator
                    $lineCounter = 0
                }
            }
        }
    }
}
