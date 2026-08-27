# Scans all .cs files for encoding corruption (literal ? in string literals
# that appear in CJK data contexts). Run before committing.
# Usage:  powershell -File check-encoding.ps1

$root = Split-Path $MyInvocation.MyCommand.Path -Parent | Split-Path -Parent
$found = $false

Get-ChildItem -Path $root -Recurse -Filter '*.cs' |
	Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\|\\.vs\\' } |
	ForEach-Object {
		$path = $_.FullName
		$bytes = [System.IO.File]::ReadAllBytes($path)
		$content = [System.Text.Encoding]::UTF8.GetString($bytes)
		$lines = $content -split "`r`n"

		for ($i = 0; $i -lt $lines.Count; $i++) {
			$line = $lines[$i]
			if ($line -match '^\s*//') { continue }
			if ($line -match '^\s*using\s') { continue }
			if ($line -match '_\s*=>\s*"') { continue }
			if ($line -match 'Description\s*=\s*') { continue }

			# Only flag ? inside quoted strings where the character after ?
			# is a CJK-data-context delimiter (comma, semicolon, closing bracket, pipe)
			# This avoids false positives from English questions, ternary
			# operators, null-coalescing, and nullable types.
			$lineBytes = [System.Text.Encoding]::UTF8.GetBytes($line)
			for ($j = 0; $j -lt $lineBytes.Length; $j++) {
				if ($lineBytes[$j] -ne 0x3F) { continue }

				# Must be inside a quoted string
				$before = $line.Substring(0, [Math]::Min($j, $line.Length))
				$quotes = ($before.ToCharArray() | Where-Object { $_ -eq [char]34 }).Count
				if ($quotes % 2 -ne 1) { continue }

				# Must be followed by a CJK-data-context character
				$after = $line.Substring([Math]::Min($j + 1, $line.Length))
				if ($after -match '^\s*[,;]|^\s*\]|^\s*\|') {
					$rel = $path.Replace($root + '\', '')
					Write-Host "SUSPECT: ${rel}:$($i + 1): $($line.Trim())" -ForegroundColor Yellow
					$found = $true
					break
				}
			}
		}
	}

if ($found) {
	Write-Host "Encoding corruption detected - review the lines above." -ForegroundColor Red
	exit 1
} else {
	Write-Host "No encoding corruption detected." -ForegroundColor Green
	exit 0
}