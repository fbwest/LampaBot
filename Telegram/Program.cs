using Serilog;
using Telegram;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
    
Log.Information("LampaBot.Back running at: {Time}", DateTimeOffset.Now);

var bot = new TgBot();
await bot.RunAsync();