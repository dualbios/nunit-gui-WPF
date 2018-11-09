set arg1=%1
nuget pack -build -Properties Configuration=%arg1%
start /d "bin\"%arg1% "ss" "%programfiles%\7-zip\7z.exe" a ..\..\NUnit3GUIWPF.zip *.*