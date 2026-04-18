$logPath = Join-Path $PSScriptRoot "security.log"
$timestamp = (Get-Date).ToUniversalTime().ToString("o")
$payload = [Console]::In.ReadToEnd()

$toolName = "unknown"
$ok = $true
if (-not [string]::IsNullOrWhiteSpace($payload)) {
    try {
        $obj = $payload | ConvertFrom-Json -ErrorAction Stop
        if ($obj.toolName) {
            $toolName = [string]$obj.toolName
        } elseif ($obj.tool -and $obj.tool.name) {
            $toolName = [string]$obj.tool.name
        }
    } catch {
        $ok = $false
    }
}

$entry = [ordered]@{
    type = "preToolUse"
    timestamp = $timestamp
    toolName = $toolName
    parsed = $ok
    payload = $payload
}

$entry | ConvertTo-Json -Depth 10 -Compress | Add-Content -Path $logPath
