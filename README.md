# TinyImagePS
A powershell module to compress/shrink/tinify images using https://tinypng.com/ and https://tinypng.com/

## Usage Exmples

Shrink in-place
``` ps
Set-TinyImageApiKey $ApiKey # from https://tinypng.com/developers
Compress-TinyImage ".\panda.jpg" -Replace  # replace file
```

Shrink and resize
``` ps
Compress-TinyImage ".\panda.jpg" | Get-TinyImage ".\pandasmall.jpg" -ResizeMode Fit -Width 300 -Height 300 -Force
```

## Install

Install it form the Powershell Gallery:
```
Install-Module -Name TinyImagePS -AllowPrerelease
```