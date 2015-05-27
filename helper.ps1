# helper.ps1 - Create partition from system partition
#
# Author: Zex <top_zlych@yahoo.com>
#
$customize_lb = "CUSTOMIZE"
$windows_lb = "WINDOWS"
$customize_min_sz = 1024 # MB
$customize_dp = "D:\\customize-helper.dp"
$customize_gpt_attr = "0x8000000000000001" # hidden and required
$customize_gpt_type = "80800910-1234-9876-4381-5510798332f5"

function find_avail_space()
{
	$vols = get-volume
	
	foreach ($p in $vols)
	{
    	if ("Fixed".Equals($p.DriveType))
    	{
#   	if ($windows_lb.Equals($p.FileSystemLabel))
#   	{
    		if ($p.SizeRemaining -gt $customize_min_sz*1024*1024)
    		{
    			return $p.DriveLetter
    		}
    	}
	}
}

function create_customize_dp($drive_ltr)
{
	$parts=get-partition
	
	foreach ($p in $parts)
	{
		if ($p.DriveLetter.Equals($drive_ltr))
		{
			$disk_nr = $p.DiskNumber
			if ($disk_nr.Lengh -eq 0) { break }
				
			$part_nr = $p.PartitionNumber
			if ($part_nr.Lengh -eq 0) { break }
				
			"select disk $disk_nr" |Add-Content -Encoding UTF8 $customize_dp
			"select part $part_nr" |Add-Content -Encoding UTF8 $customize_dp
			
			$need_shrink = disk_check $disk_nr

			if (-1 -eq $need_shrink)
			{
				return
			}
			
			if (1 -eq $need_shrink)
			{
				"shrink desired=$customize_min_sz" |Add-Content -Encoding UTF8 $customize_dp
			}
			
			"create part primary size=$customize_min_sz" |Add-Content -Encoding UTF8 $customize_dp
			"format quick fs=fat32 label=CUSTOMIZE" |Add-Content -Encoding UTF8 $customize_dp
			"gpt attributes=$customize_gpt_attr" |Add-Content -Encoding UTF8 $customize_dp
			"set id=$customize_gpt_type" |Add-Content -Encoding UTF8 $customize_dp
			break;
		}
	}
}

# -1, the disk type isn't GPT
# 1, the disk doesn't have enough free space for customize, need shrink from other partition 
# 0, the disk has enough free space for customize
function disk_check($disk_nr)
{
	$disk = get-disk -Number $disk_nr
	
	if ($disk.PartitionType -ne "GPT")
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

"rem partition helper script"|Set-Content -Encoding UTF8 $customize_dp
$drive_ltr = find_avail_space
create_customize_dp $drive_ltr
diskpart /s $customize_dp

