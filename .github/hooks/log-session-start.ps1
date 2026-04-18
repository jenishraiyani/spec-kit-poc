$logPath = Join-Path $PSScriptRoot "security.log"
$timestamp = (Get-Date).ToUniversalTime().ToString("o")
$payload = [Console]::In.ReadToEnd()

$sessionId = $null
$ok = $true
if (-not [string]::IsNullOrWhiteSpace($payload)) {
    try {
        $obj = $payload | ConvertFrom-Json -ErrorAction Stop
        if ($obj.sessionId) {
            $sessionId = [string]$obj.sessionId
        } elseif ($obj.session -and $obj.session.id) {
            $sessionId = [string]$obj.session.id
        }
    } catch {
        $ok = $false
    }
}

$entry = [ordered]@{
    type = "sessionStart"
    timestamp = $timestamp
    sessionId = $sessionId
    parsed = $ok
    payload = $payload
}

$entry | ConvertTo-Json -Depth 10 -Compress | Add-Content -Path $logPath
