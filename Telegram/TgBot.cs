using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Telegram;

public class TgBot
{
    private Chat _botChat = null!;
    private Command _state = Command.None;
    
    public async Task RunAsync()
    {
        using var cts = new CancellationTokenSource();
        
        var bot = new TelegramBotClient("7324975235:AAFxAZpoA5av8hzNKnCLBz7qCJ3lraJ_sC8", cancellationToken: cts.Token);

        await bot.DropPendingUpdatesAsync(cts.Token);
        
        bot.StartReceiving(HandleUpdate, HandleError, null, cts.Token);

        var me = await bot.GetMeAsync(cts.Token);
        Log.Information("@{MeUsername} is running... Press Enter to terminate", me.Username);
        Console.ReadLine();
        await cts.CancelAsync(); // stop the bot
    }
    
    private async Task HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message?.Text is null) return; // we want only updates about new Text Message
        var msg = update.Message;

        _botChat = msg.Chat;
        
        Log.Information("Received message \'{MsgText}\' from {FromFirstName}",
            msg.Text, msg.From?.Username ?? msg.From?.FirstName);

        var respond = string.Empty;
        
        switch (_state)
        {
            case Command.None:
            {
                var isDecentCommand = Enum.TryParse<Command>(msg.Text.Split('/').Last(), true, out var command);
                if (!isDecentCommand) throw new Exception("Неверная команда");

                switch (command)
                {
                    case Command.Start:
                        respond = "Добро пожаловать в LampaBot\ud83c\udf52!";
                        break;
                    case Command.Calc:
                        respond = "Введите длительность фильма в минутах";
                        _state = Command.Calc;
                        break;
                    default:
                        throw new Exception("Неверная команда");
                }

                break;
            }
            case Command.Calc:
            {
                var success = int.TryParse(msg.Text, out var mins);
                if (!success) throw new Exception("Введено неверное число");

                var maxSize = (double)mins * 60 * 3 / (8 * 1024);

                respond = $"Максимальный размер фильма: {maxSize:f2} Гб";

                _state = Command.None;
                break;
            }
        }

        await bot.SendTextMessageAsync(msg.Chat, respond, cancellationToken: ct);
    }
    
    private async Task HandleError(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        Log.Error("{Message}", ex.Message);

        await bot.SendTextMessageAsync(_botChat, ex.Message, cancellationToken: ct);
    }
}