$veracodeapiid=$args[0]
$veracodeapikey=$args[1]
$versionName=$args[3]
$filename=$args[4]
$appci=$args[5]
$appName = "{0} {1}{2}" -f $appci, $args[2], "service"
$sandboxname = "{0} {1}{2}" -f $appci, $args[2], "service"




$applist=$(java -jar /home/tools/VeracodeJavaAPI.jar -vid $veracodeapiid -vkey $veracodeapikey -action getapplist)
$xmlapplist = [xml] $applist;
$app=$xmlapplist.applist.app | where-Object { $_.app_name -eq $appName } | Select-Object app_id
$appid = $app.app_id

if ($null -eq $appid) {
    Write-Host "`t Veracode App not found. Please have the Veracode app created as $appName"
    exit 1
} else {
    Write-Host "`t ===VeracodeAppName :" $appName;
    Write-Host "`t ===App :" $app;
    Write-Host "`t ===App ID : "$appid;
}

$response=$(java -jar /home/tools/VeracodeJavaAPI.jar -action uploadandscan -vid $veracodeapiid -vkey $veracodeapikey -appname $appName -createprofile false -filepath $filename -createsandbox true -sandboxname $sandboxname -version $versionName -autorecreate true)
$response