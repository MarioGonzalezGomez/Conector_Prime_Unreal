param(
    [ValidateSet('P_Sumario','Text')]
    [string]$Mode = 'P_Sumario',

    [string]$TextValue = 'Hola PauPau'
)

$signal = switch ($Mode) {
    'P_Sumario' { 'P_Sumario' }
    'Text' { "CHG_TxtSumario_$TextValue" }
}

$client = New-Object System.Net.Sockets.TcpClient
$client.Connect('127.0.0.1', 8283)
$stream = $client.GetStream()
$writer = New-Object System.IO.StreamWriter($stream)
$writer.WriteLine($signal)
$writer.Flush()
$writer.Dispose()
$stream.Dispose()
$client.Dispose()
