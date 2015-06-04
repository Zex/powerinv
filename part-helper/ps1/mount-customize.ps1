# mount-customize.ps1 - Mount customize partition to diretory
# Author: Zex <top_zlynch@yahoo.com>
#
# powershell -ExecutionPolicy Bypass -File <path-to-mount-customize.ps1>
#---------------------------------------------------------------------------
. c:\part-helper\ps1\common.ps1

$disk_nr, $part_nr = create_customize_mount_dp

echo "disk_nr: $disk_nr"
echo "part_nr: $part_nr"

if ($disk_nr -ne $null -and $part_nr -ne $null)
{
    diskpart /s $customize_premount_dp
    Add-PartitionAccessPath -DiskNumber $disk_nr -PartitionNumber $part_nr -AccessPath $customize_mp 
}
