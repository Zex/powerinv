rem ---------------------------------------------
rem Minimum UEFI boot image
rem
rem \EFI\Boot
rem          \bootx64.efi
rem          \BCD
rem \sources
rem          \boot.wim
rem \Boot
rem      \boot.sdi
rem ---------------------------------------------
rem
rem \Device\HarddiskVolume[nr] - The partition that contains boot image
rem
bcdedit -store BCD -create {ramdiskoptions} -d "Tiny Boot"
bcdedit -store BCD -set {ramdiskoptions} ramdisksdidevice partition=\Device\HarddiskVolume2
bcdedit -store BCD -set {ramdiskoptions} ramdisksdipath \Boot\boot.sdi
bcdedit -store BCD -set {default} device ramdisk=[\Device\HarddiskVolume2]\sources\boot.wim,{ramdiskoptions}
bcdedit -store BCD -set {default} osdevice ramdisk=[\Device\HarddiskVolume2]\sources\boot.wim,{ramdiskoptions} 


