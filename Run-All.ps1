param(
    [switch]$WatchBackend,
    [switch]$WatchFrontend,
    [int]$BackendPort = 5099,
    [int]$FrontendPort = 5173
)

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$frontendDir = Join-Path $repoRoot 'ArchiTrackerFE'
$backendDir = Join-Path $repoRoot 'ArchiTrackerBE'
$backendUrl = "http://localhost:$BackendPort"
$frontendUrl = "http://localhost:$FrontendPort"

Write-Host "🚀 Starting backend on $backendUrl"

if ($WatchBackend) {
    $backendArgs = @('watch', '--project', (Join-Path $backendDir 'ArchiTrackerBE.csproj'), '--', 'run', '--urls', $backendUrl)
} else {
    $backendArgs = @('run', '--project', (Join-Path $backendDir 'ArchiTrackerBE.csproj'), '--urls', $backendUrl)
}

$backendInfo = Start-Process -FilePath 'dotnet' -ArgumentList $backendArgs -WorkingDirectory $backendDir -PassThru

Start-Sleep -Seconds 2
Write-Host "⚙️  Starting frontend on $frontendUrl"

$frontendArgs = @('dev', '--host', '0.0.0.0', '--port', $FrontendPort)
if ($WatchFrontend) {
    $frontendArgs = @('dev', '--host', '0.0.0.0', '--port', $FrontendPort)
}
$frontendInfo = Start-Process -FilePath 'yarn' -ArgumentList $frontendArgs -WorkingDirectory $frontendDir -PassThru

Write-Host "🌐 Opening browsers..."
Start-Sleep -Seconds 3
Start-Process $frontendUrl
Start-Process $backendUrl

Write-Host "Backend PID: $($backendInfo.Id) | Frontend PID: $($frontendInfo.Id)"
Write-Host "Press Ctrl+C to stop."

$null = $backendInfo.WaitForExit()
$null = $frontendInfo.WaitForExit()
