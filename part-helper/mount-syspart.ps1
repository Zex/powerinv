# mount-syspart.ps1 - Mount system partition to diretory
# powershell -File <path-to-mount-syspart.ps1>
#---------------------------------------------------------------------------
. c:\part-helper\common.ps1

$disk_nr, $part_nr = create_syspart_mount_dp

diskpart /s $syspart_premount_dp
Add-PartitionAccessPath -DiskNumber $disk_nr -PartitionNumber $part_nr -AccessPath $syspart_mp  -ErrorAction Ignore
