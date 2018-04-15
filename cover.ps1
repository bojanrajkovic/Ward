nuget install -OutputDirectory packages -Version 4.6.519 OpenCover
nuget install -OutputDirectory packages -Version 3.1.2 ReportGenerator

$dotnetPath=(Join-Path $env:ProgramFiles (Join-Path dotnet dotnet.exe))
.\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe -register:user -target:"$dotnetPath" `
    -targetargs:"test" -output:"coverage.xml" -oldstyle `
    -filter:"+[Ward*]* -[Ward.DnsClient]Asn1.* -[Ward.*.Tests]*"

.\packages\ReportGenerator.3.1.2\tools\ReportGenerator.exe -reports:coverage.xml -targetdir:coverage

Start-Process coverage\index.htm
