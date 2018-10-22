@echo off
set redisConnection="XXX,ssl=False,abortConnect=False"
set sendSize=2048
set totalChannel=14000
set connectionCount=2
dotnet run --connectionString "%redis%" --sendSize %sendSize% --channelCount %totalChannel% --connectionCount %connectionCount%
