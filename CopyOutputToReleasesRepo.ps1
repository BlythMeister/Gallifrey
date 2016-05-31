Param(
  [Parameter(Position=0)]
  [alias("t")]
  [ValidateSet('alpha','beta','stable')]
  [System.String]$ReleaseType
)

if($ReleaseType -eq "")
{
	Write-Host "No Release Type Entered"
}
else
{
	Write-Host "Publishing $ReleaseType To Releases Repo"
	Copy-Item Output\$ReleaseType\ ..\Gallifrey.Releases\download\modern\ -recurse -force
	Write-Host "Published $ReleaseType"
}