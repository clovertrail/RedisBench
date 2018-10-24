#!/bin/bash
redisConnection="xxx.com:6379,ssl=False,abortConnect=False"
sendSize=2048
totalChannel=14000
connectionCount=2
dotnet run --connectionString "$redisConnection" --sendSize $sendSize --channelCount $totalChannel --connectionCount $connectionCount
