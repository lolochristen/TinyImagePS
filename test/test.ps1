Import-Module .\TinyImage.psd1

$testimagepath = "..\..\..\..\..\test"

#$ApiKey = "tbd"
#Set-TinyImageApiKey $ApiKey

Get-ChildItem "$testimagepath\*.*" -exclude *.ps1 | Compress-TinyImage -Verbose | Get-TinyImage ".\"

Compress-TinyImage "$testimagepath\panda.jpg" ".\"

Compress-TinyImage "$testimagepath\*.*" ".\" -Verbose -Force

Compress-TinyImage "$testimagepath\panda.jpg" -Verbose | ft
Compress-TinyImage "$testimagepath\panda.jpg" -Verbose | fl

Compress-TinyImage "$testimagepath\panda.jpg" -Destination pandas2.jpg -Force -Verbose

Compress-TinyImage "$testimagepath\panda.jpg" ".\pandasmall.jpg" -Force -Verbose
Compress-TinyImage "$testimagepath\panda.jpg" -Verbose | Get-TinyImage ".\pandasmall1.jpg" -Force -Verbose

Compress-TinyImage "$testimagepath\panda.jpg" -Verbose | Get-TinyImage ".\pandasmall2.jpg" -ResizeMode Fit -Width 300 -Height 300 -Force -Verbose

Copy "$testimagepath\panda.jpg" ".\panda2.jpg"
Compress-TinyImage ".\panda2.jpg" -Replace -Verbose

Compress-TinyImage "$testimagepath\monkey.png" ".\monkey.png" -Force -Verbose

Compress-TinyImage "$testimagepath\monkey.png" -Verbose | Get-TinyImage ".\monkey2.png" -ResizeMode Scale -Width 300 -Force -Verbose

Read-Host -Prompt "Press any key to continue"