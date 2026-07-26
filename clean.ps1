[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$VendorSdks
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$GitRoot = Join-Path $Root '.git'
$VendorRoot = Join-Path $Root 'third_party'
$RemovedCount = 0

function Test-IsProtectedPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if ($fullPath.StartsWith($GitRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    if (-not $VendorSdks.IsPresent -and
        $fullPath.StartsWith($VendorRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    return $false
}

function Remove-GeneratedPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    if (Test-IsProtectedPath -Path $Path) {
        return
    }

    if ($PSCmdlet.ShouldProcess($Path, 'Remove generated file or directory')) {
        Remove-Item -LiteralPath $Path -Recurse -Force
        $script:RemovedCount++
        Write-Host "Removed: $Path" -ForegroundColor DarkGray
    }
}

# Root-level build, publish, IDE, and test output.
@(
    'build',
    'dist',
    'out',
    'logs',
    '.vs',
    'TestResults',
    'artifacts'
) | ForEach-Object {
    Remove-GeneratedPath -Path (Join-Path $Root $_)
}

# Project-local generated directories. Sort deepest paths first so deleting a
# parent never invalidates a child that is still waiting to be processed.
$generatedDirectoryNames = @('bin', 'obj', 'CMakeFiles', 'TestResults')
$generatedDirectories = @(
    Get-ChildItem -LiteralPath $Root -Directory -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object {
            $generatedDirectoryNames -contains $_.Name -and
            -not (Test-IsProtectedPath -Path $_.FullName)
        } |
        Sort-Object { $_.FullName.Length } -Descending
)

foreach ($directory in $generatedDirectories) {
    Remove-GeneratedPath -Path $directory.FullName
}

# CMake metadata and temporary editor/merge files.
$generatedFileNames = @(
    'CMakeCache.txt',
    'cmake_install.cmake',
    'CMakeUserPresets.json'
)
$temporaryExtensions = @(
    '.tmp',
    '.bak',
    '.backup',
    '.gpu-backup',
    '.orig',
    '.rej'
)

$generatedFiles = @(
    Get-ChildItem -LiteralPath $Root -File -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object {
            -not (Test-IsProtectedPath -Path $_.FullName) -and
            ($generatedFileNames -contains $_.Name -or
             $temporaryExtensions -contains $_.Extension)
        }
)

foreach ($file in $generatedFiles) {
    Remove-GeneratedPath -Path $file.FullName
}

if ($VendorSdks.IsPresent -and (Test-Path -LiteralPath $VendorRoot)) {
    Get-ChildItem -LiteralPath $VendorRoot -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -ne 'README.md' } |
        ForEach-Object { Remove-GeneratedPath -Path $_.FullName }
}

if ($RemovedCount -eq 0) {
    Write-Host 'Nothing to clean.' -ForegroundColor Green
}
else {
    Write-Host "Cleanup complete. Removed $RemovedCount item(s)." -ForegroundColor Green
}

if (-not $VendorSdks.IsPresent -and (Test-Path -LiteralPath $VendorRoot)) {
    Write-Host 'Vendor SDK checkouts were preserved. Use -VendorSdks to remove them.' -ForegroundColor Cyan
}
