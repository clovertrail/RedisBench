# Evaluate the Redis's Pub/Sub performance
 
* Command

`dotnet run --connectionString "xxx" --sendSize 2048 --channelCount 10000`

or run the bat file after replacing 'redisConnection' with your own redis connection string

The output is as follows. It tells you the message distribution percentage of less than 1 second and greater than 1 second. We expect to see as many channels as possible with all messages' latency are less than 1s.

`
<=1s: count=2033971 percent=100.000%, >1s: count=0 percent=0.000%
<=1s: count=2050035 percent=100.000%, >1s: count=0 percent=0.000%
<=1s: count=2065891 percent=100.000%, >1s: count=0 percent=0.000%
<=1s: count=2082604 percent=100.000%, >1s: count=0 percent=0.000%
<=1s: count=2098141 percent=100.000%, >1s: count=0 percent=0.000%
<=1s: count=2114292 percent=100.000%, >1s: count=0 percent=0.000%
`
