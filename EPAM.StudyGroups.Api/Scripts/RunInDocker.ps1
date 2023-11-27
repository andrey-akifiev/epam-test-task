$sqlPass = "<YourStrong@Passw0rd>"
$sqlContainer = (docker ps -a | Select-String "sql1")

if( -not ([string]::IsNullOrEmpty($sqlContainer))){
	if([string]::IsNullOrEmpty((docker ps | Select-String "sql1")))
	{
		docker container start sql1
	}
	else 
	{ 
		# return
	}
}
else
{
	docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=$sqlPass" -p 1433:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2022-latest
}

$regex = [regex] "\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b"
$sqlIP = ($regex.Matches((docker inspect sql1 | Select-String '"IPAddress":')) | %{ $_.value })[0]
$newConnectionString = "Data Source=$sqlIP,1433; Database=StudyGroups; user id=sa; password=$sqlPass; MultipleActiveResultSets=true"

$appsettingsDevPath = ".\appsettings.Development.json"
$appsettingsDevOriginal = Get-Content $appsettingsDevPath -Raw
$appsettingsDevOriginal | Out-File -FilePath ($appsettingsDevPath+"_backup")
$appsettingsDevParsed = Get-Content $appsettingsDevPath -Raw | ConvertFrom-Json
$appsettingsDevParsed.ConnectionStrings.StudyGroupsContext = $newConnectionString
$appsettingsDevParsed | ConvertTo-Json -Depth 10 | Out-File $appsettingsDevPath

try{
	docker build -t be -f .\Dockerfile ..
	docker run -t -d -p 80:80 -e ASPNETCORE_ENVIRONMENT=Development be
}
finally {
	$appsettingsDevOriginal | Out-File $appsettingsDevPath
}