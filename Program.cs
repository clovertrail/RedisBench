using CommandLine;
using System;

namespace RedisClient
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isInputValid = true;
            var argsOption = new ArgsOption();
            var result = Parser.Default.ParseArguments<ArgsOption>(args)
                .WithParsed(options => argsOption = options)
                .WithNotParsed(error => {
                    isInputValid = false;
                    Console.WriteLine($"Input is invalid for {error}");
                });
            if (!isInputValid)
            {
                return;
            }
            RedisBench rb = new RedisBench(argsOption.ConnectionString,
                argsOption.ChannelCount, argsOption.SendSize);
            rb.StartBench();
            Console.WriteLine("Press any key to stop...");
            Console.ReadLine();
            rb.StopBench();
        }
    }
}
