Import-Module .\TinyImagePS.psd1

$testimagepath = "..\..\..\..\..\test"

#$ApiKey = "tbd"
#Set-TinyImageApiKey $ApiKey

Compress-TinyImage "$testimagepath\panda.jpg" | ft
Compress-TinyImage "$testimagepath\panda.jpg" | fl
Read-Host -Prompt "Press any key to continue"

Compress-TinyImage "$testimagepath\panda.jpg" ".\pandasmall.jpg" -Force
Compress-TinyImage "$testimagepath\panda.jpg" | Get-TinyImage ".\pandasmall1.jpg" -Force

Compress-TinyImage "$testimagepath\panda.jpg" | Get-TinyImage ".\pandasmall2.jpg" -ResizeMode Fit -Width 300 -Height 300 -Force

Copy "$testimagepath\panda.jpg" ".\panda2.jpg"
Compress-TinyImage ".\panda2.jpg" -Replace

Compress-TinyImage "$testimagepath\monkey.png" ".\monkey.png" -Force

Compress-TinyImage "$testimagepath\monkey.png" | Get-TinyImage ".\monkey2.png" -ResizeMode Scale -Width 300 -Force

Read-Host -Prompt "Press any key to continue"