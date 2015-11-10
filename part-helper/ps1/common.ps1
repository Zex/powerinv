# common.ps1 - Mount customize partition to diretory
# Author: Zex <top_zlynch@yahoo.com>
#
# OPTIONAL SETTING:
#  powershell -Command {Set-ExecutionPolicy RemoteSigned}
#
# powershell -ExecutionPolicy Bypass -File <path-to-common-definitions>
#---------------------------------------------------------------------------
# Common definitions
$customize_lb = "CUSTOMIZE"
$windows_lb = "Windows"
$basic_data_gpt_type = "ebd0a0a2-b9e5-4433-87c0-68b6b72699c7"
$outpath = "c:\part-helper\out"
$out_dp = "$outpath\out.dp"
# CUSTOMIZE attributes
$customize_min_sz = 1024 # MB
$customize_premount_dp = "$outpath\premount-customize.dp"
$customize_postmount_dp = "$outpath\postmount-customize.dp"
$customize_gpt_attr = "0x8000000000000001" # hidden and required
$customize_gpt_type = "80800910-1234-9876-4381-5510798332f5"
$customize_mp = "c:\customize"
$customize_umount_ps = "$outpath\umount-customize.ps1"
# SYSPART attributes
$syspart_premount_dp = "$outpath\premount-syspart.dp"
$syspart_postmount_dp = "$outpath\postmount-syspart.dp"
$syspart_gpt_type = "c12a7328-f81f-11d2-ba4b-00a0c93ec93b"
$syspart_mp = "c:\syspart"
$syspart_umount_ps = "$outpath\umount-syspart.ps1"

function find_avail_space()
{
	$vols = get-volume
	
	foreach ($p in $vols)
	{
		if ("Fixed".Equals($p.DriveType, 5))
		{
#		if ($windows_lb.Equals($p.FileSystemLabel))
#		{
			if ($p.SizeRemaining -gt $customize_min_sz*1024*1024)
			{
				return $p.DriveLetter
			}
		}
	}
}

# -1, the disk doesn't supported
# 1, the disk doesn't have enough free space for customize, need shrink from other partition 
# 0, the disk has enough free space for customize
function disk_check($disk_nr)
{
	$disk = get-disk -Number $disk_nr
	
	if (!"GPT".Equals($disk.PartitionStyle))
	{
		return -1
	}
		
	$avail = ($p.Size-$p.AllocatedSize)/1024/1024
		
	if ($avail -gt $customize_min_sz)
	{
		return 0
	}
	else
	{
		return 1
	}
}

function create_customize_dp($drive_ltr)
{
	$parts = get-partition
	
	foreach ($p in $parts)
	{
		if ($p.DriveLetter.Equals($drive_ltr, 5))
		{
			$disk_nr = $p.DiskNumber
			if ($disk_nr.Lengh -eq 0) { break }
				
			$part_nr = $p.PartitionNumber
			if ($part_nr.Lengh -eq 0) { break }
				
			"select disk $disk_nr" |Add-Content -Encoding UTF8 $out_dp
			"select part $part_nr" |Add-Content -Encoding UTF8 $out_dp
			
			$need_shrink = 1#disk_check $disk_nr
			
			if (-1 -eq $need_shrink)
			{
				echo "-1 -eq $need_shrink"
				return
			}
			
			if (1 -eq $need_shrink)
			{
				echo "shrink desired=$customize_min_sz" |Add-Content -Encoding UTF8 $out_dp
			}
			
			"create part primary size=$customize_min_sz" |Add-Content -Encoding UTF8 $out_dp
			"format quick fs=fat32 label=CUSTOMIZE" |Add-Content -Encoding UTF8 $out_dp
			"gpt attributes=$customize_gpt_attr" |Add-Content -Encoding UTF8 $out_dp
            "set id=$customize_gpt_type override" |Add-Content -Encoding UTF8 $out_dp
			break;
		}
	}
}

function create_customize_mount_dp()
{
	$parts = get-partition
	
	mkdir -Path $customize_mp -Force |Out-Null
	
	foreach ($p in $parts)
	{
		if ("{$customize_gpt_type}".Equals($p.GptType, 5)) # System.StringComparison.OrdinalIgnoreCase
		{
			$disk_nr = $p.DiskNumber
			if ($disk_nr.Lengh -eq 0) { break }
				
			$part_nr = $p.PartitionNumber
			if ($part_nr.Lengh -eq 0) { break }
			
			# create customize premount diskpart script
			"rem Partitions helper script"|Set-Content -Encoding UTF8 $customize_premount_dp
			"select disk $disk_nr" |Add-Content -Encoding UTF8 $customize_premount_dp
			"select part $part_nr" |Add-Content -Encoding UTF8 $customize_premount_dp
			"set id=$basic_data_gpt_type override" |Add-Content -Encoding UTF8 $customize_premount_dp		
			
			# create customize postmount diskpart script
			"rem Partitions helper script"|Set-Content -Encoding UTF8 $customize_postmount_dp
			"select disk $disk_nr" |Add-Content -Encoding UTF8 $customize_postmount_dp
			"select part $part_nr" |Add-Content -Encoding UTF8 $customize_postmount_dp
			"set id=$customize_gpt_type override" |Add-Content -Encoding UTF8 $customize_postmount_dp			
			
			# create umount customize ps1
			"# mount-customize.ps1 - Mount customize partition to diretory" |Set-Content -Encoding UTF8 $customize_umount_ps
			"# powershell -File $customize_umount_ps" |Add-Content -Encoding UTF8 $customize_umount_ps
			"#---------------------------------------------------------------------------"  |Add-Content -Encoding UTF8 $customize_umount_ps
			". c:\part-helper\ps1\common.ps1" |Add-Content -Encoding UTF8 $customize_umount_ps
			"Remove-PartitionAccessPath -DiskNumber $disk_nr -PartitionNumber $part_nr -AccessPath $customize_mp -ErrorAction Ignore" |Add-Content -Encoding UTF8 $customize_umount_ps
			"diskpart /s $customize_postmount_dp" |Add-Content -Encoding UTF8 $customize_umount_ps
			
			return ($disk_nr, $part_nr)
		}
	}
}

function create_syspart_mount_dp()
{
	$parts = get-partition
	
	mkdir -Path $syspart_mp -Force |Out-Path
	
	foreach ($p in $parts)
	{
		if ("{$syspart_gpt_type}".Equals($p.GptType, 5)) # System.OrdinalIgnoreCase.OrdinalIgnoreCase
		{
			$disk_nr = $p.DiskNumber
			if ($disk_nr.Lengh -eq 0) { break }
				
			$part_nr = $p.PartitionNumber
			if ($part_nr.Lengh -eq 0) { break }
			
			# create SYSPART premount diskpart script
			"rem syspart helper script"|Set-Content -Encoding UTF8 $syspart_premount_dp
			"select disk $disk_nr" |Add-Content -Encoding UTF8 $syspart_premount_dp
			"select part $part_nr" |Add-Content -Encoding UTF8 $syspart_premount_dp
			"set id=$basic_data_gpt_type override" |Add-Content -Encoding UTF8 $syspart_premount_dp

			# create SYSPART postmount diskpart script
			"rem syspart helper script"|Set-Content -Encoding UTF8 $syspart_postmount_dp
			"select disk $disk_nr" |Add-Content -Encoding UTF8 $syspart_postmount_dp
			"select part $part_nr" |Add-Content -Encoding UTF8 $syspart_postmount_dp
			"set id=$syspart_gpt_type override" |Add-Content -Encoding UTF8 $syspart_postmount_dp			

			# create umount SYSPART ps1
			"# mount-syspart.ps1 - Mount system partition to diretory" |Set-Content -Encoding UTF8 $syspart_umount_ps
			"# powershell -File $syspart_umount_ps" |Add-Content -Encoding UTF8 $syspart_umount_ps
			"#---------------------------------------------------------------------------" |Add-Content -Encoding UTF8 $syspart_umount_ps
			". c:\part-helper\ps1\common.ps1" |Add-Content -Encoding UTF8 $syspart_umount_ps
			"Remove-PartitionAccessPath -DiskNumber $disk_nr -PartitionNumber $part_nr -AccessPath $syspart_mp -ErrorAction Ignore" |Add-Content -Encoding UTF8 $syspart_umount_ps
			"diskpart /s $syspart_postmount_dp" |Add-Content -Encoding UTF8 $syspart_umount_ps
			
			return ($disk_nr, $part_nr)
		}
	}
}

mkdir -Path $outpath -Force |Out-Null 
