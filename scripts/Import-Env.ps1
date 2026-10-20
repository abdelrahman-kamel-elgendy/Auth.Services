param(
    [Parameter(Mandatory = $true)]
    [string] $Path
)

$resolvedPath = Resolve-Path -LiteralPath $Path -ErrorAction Stop
$lineNumber = 0

foreach ($line in Get-Content -LiteralPath $resolvedPath -Encoding UTF8) {
    $lineNumber++
    $trimmedLine = $line.Trim()

    if ([string]::IsNullOrWhiteSpace($trimmedLine) -or $trimmedLine.StartsWith("#")) {
        continue
    }

    $separatorIndex = $trimmedLine.IndexOf("=")
    if ($separatorIndex -lt 1) {
        throw "Invalid env-file entry at ${resolvedPath}:$lineNumber. Expected KEY=VALUE."
    }

    $name = $trimmedLine.Substring(0, $separatorIndex).Trim()
    $value = $trimmedLine.Substring($separatorIndex + 1).Trim()

    if ($name -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
        throw "Invalid environment variable name at ${resolvedPath}:$lineNumber."
    }

    if ($value.Length -ge 2 -and
        (($value[0] -eq '"' -and $value[$value.Length - 1] -eq '"') -or
         ($value[0] -eq "'" -and $value[$value.Length - 1] -eq "'"))) {
        $value = $value.Substring(1, $value.Length - 2)
    }

    if (-not [string]::IsNullOrWhiteSpace($value)) {
        [Environment]::SetEnvironmentVariable($name, $value, "Process")
    }
}

Write-Host "Loaded non-empty settings from $resolvedPath into this PowerShell process."
Write-Host "Secret values were not printed. Blank values are ignored."
