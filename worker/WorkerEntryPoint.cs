using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using lib;
using lib.db;

namespace worker;

public static class WorkerEntryPoint
{
    public static void Main()
    {
        var releaseTag = EnvironmentVariables.Get("PEGOVKA_RELEASE_TAG");
        var shardingToken = EnvironmentVariables.Get("PEGOVKA_SHARDING_TOKEN");
        var timeoutMinutes = EnvironmentVariables.TryGet("PEGOVKA_TIMEOUT_MINUTES", int.Parse, 15);
        Console.Out.WriteLine($"Worker '{releaseTag}' is processing shard: {shardingToken}");

        // определить задачу по shardingToken
        // решить задачу
        // сохранить решение задачи в БД
    }
}
