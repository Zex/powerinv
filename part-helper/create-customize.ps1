# create-customize.ps1 - Create customize partition in Windows
# powershell -File <path-to-create-customize.ps1>
#---------------------------------------------------------------------------
. c:\part-helper\common.ps1

$out_dp = "$outpath\create-customize.dp"

"rem Partition helper script"|Set-Content -Encoding UTF8 $out_dp
$drive_ltr = find_avail_space
create_customize_dp $drive_ltr
diskpart /s $out_dp