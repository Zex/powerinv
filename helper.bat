# helper.bat - Create partition from system partition
# Author: Zex <top_zlych@yahoo.com>
# 
# Run as administrator
# > powershell -File <path-to-helper.ps1>

powershell -File c:\part-helper\mount-customize.ps1
powershell -File c:\part-helper\mount-syspart.ps1

