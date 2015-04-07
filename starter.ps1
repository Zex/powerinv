
function dummy()
{
Get-ExecutionPolicy -list
#Set-ExecutionPolicy RemoteSigned
$pocketknife = New-Object Object

Add-Member -memberType NoteProperty -name Color -value Red -inputObject $pocketknife
Get-Member -memberType NoteProperty -name Color -inputObject $pocketknife 

$listing = Get-ChildItem c:\

Dir variable:listing
Write-Host -NoNewline "variable:listing => "
Test-Path variable:listing
Write-Host -NoNewline "Test-Path variable:johndoe => "
Test-Path variable:johndoe

#Get-Variable z_username -Scope Global
#New-Variable z_username -value Marilyn -Description '
# The Marilyn
#'

#Push-Location
#Write-Host -NoNewline "Username => "
#$z_username
#Pop-Location
}

function utils_new_mount_point($dest)
{
    if (!(Test-Path $dest))
    {
        New-Item -ItemType directory $dest|%{$_.Attributes="hidden"}
    }   
}

function utils_mount($label, $destbase)
{     
    $label_filter = "Label='"+$label+"'"

    Get-WmiObject -Class win32_volume -Filter $label_filter|ForEach-Object -Process {
        
        $dest = "$destbase"+"\"+$_.Label
        utils_new_mount_point $dest
        $_.AddMountPoint($dest)        
    }

    return $dest
}

function utils_umount($label, $destbase)
{ 
    $label_filter = "Label='"+$label+"'"
    
    Get-WmiObject -Class win32_volume -Filter $label_filter|ForEach-Object -Process {
        
        $dest = "$destbase"+"\"+$_.Label

        mountvol $dest /d
        $_.Dismount($TRUE, $FALSE)
        Remove-Item -path $dest -Force -Recurse
    }
}

function begin_with($label, $destdir)
{
#Get-WmiObject -Class win32_volume|Where-Object {$_.Label -eq $label}|ForEach-Object -Process {$_.AddMountPoint("C:\ps\"+$_.Label)}
#Start-Process powershell -Verb runAs -ArgumentList "-file .\attr.ps1"
}

#begin_with 'hello' 'mountpoint'

#Get-Content function:mount

function identify($name)
{
    if ($name -like "*bird*")
    {
        "$name can fly"
    }
    elseif ($name -like "*cat*")
    {
        "$name needs fish"
    }
    else
    {
        "No idea what it is -_-!"
    }
        
}

for ($c = 3; $c; $c--)
{
    identify "blue bird"
}

foreach ($c in "luckydog", "funny cat")
{
    identify $c
}


