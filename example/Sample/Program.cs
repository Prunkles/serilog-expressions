﻿using System;
using Serilog;
using Serilog.Debugging;
using Serilog.Templates;

namespace Sample
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        public static void Main()
        {
            SelfLog.Enable(Console.Error);
            
            using var log = new LoggerConfiguration()
                .Enrich.WithProperty("AppId", 10)
                .Enrich.WithComputed("FirstItem", "Items[0]")
                .Enrich.WithComputed("SourceContext", "coalesce(Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1), '<no source>')")
                .Filter.ByIncludingOnly("@l = 'Information' and AppId is not null and Items[?] like 'C%'")
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3} ({SourceContext})] {Message:lj} (first item is {FirstItem}){NewLine}{Exception}")
                .WriteTo.Console(new ExpressionTemplate(
                    "[{@t:HH:mm:ss} {@l:u3} ({SourceContext})] {@m} (first item is {Items[0]})\n{@x}"))
                    .CreateLogger();

            log.ForContext<Program>().Information("Cart contains {@Items}", new[] { "Tea", "Coffee" });
            log.Warning("Cart contains {@Items}", new[] { "Tea", "Coffee" });
            log.Information("Cart contains {@Items}", new[] { "Apricots" });
            log.Information("Cart for {Name} contains {@Items}", Environment.UserName, new[] { "Peanuts", "Chocolate" });
        }
    }
}
