@echo off
set redis="signalr3.southeastasia.cloudapp.azure.com:6379,ssl=False,abortConnect=False"
set sendSize=2048
set totalChannel=14000
set connectionCount=2
dotnet run --connectionString "%redis%" --sendSize %sendSize% --channelCount %totalChannel% --connectionCount %connectionCount%
