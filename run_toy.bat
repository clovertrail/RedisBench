@echo off
set redisConnection="xxx:6379,ssl=False,abortConnect=False"
set sendSize=2048
set totalChannel=14000
set connectionCount=2
dotnet run --connectionString "%redisConnection%" --sendSize %sendSize% --channelCount %totalChannel% --connectionCount %connectionCount%
