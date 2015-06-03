# mount-customize.ps1 - Mount customize partition to diretory
# powershell -File <path-to-mount-customize.ps1>
#---------------------------------------------------------------------------
. c:\part-helper\common.ps1

$disk_nr, $part_nr = create_customize_mount_dp

diskpart /s $customize_premount_dp
Add-PartitionAccessPath -DiskNumber $disk_nr -PartitionNumber $part_nr -AccessPath $customize_mp  -ErrorAction Ignore 
