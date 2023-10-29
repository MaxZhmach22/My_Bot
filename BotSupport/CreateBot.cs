using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotSupport;
using Telegram.Bot.Types.ReplyMarkups;


public class CreateBot
{
    private string _previousMessage = "";
    private const string WeatherApiKey = "aef5c8145ae6fd0125d2951b30a7e9f3";
    private CancellationTokenSource _cts = new ();
    private readonly Dictionary<long, bool> _calculateRequests = new();
    private readonly SqlConnection _sqlConnection;

    public CreateBot(SqlConnection sqlConnection)
    {
        _sqlConnection = sqlConnection;
    }
    
    public void CreateBotMethod(string token)
    {
        var botClient = new TelegramBotClient(token);
        botClient.StartReceiving(Update, Error);

    }

    private async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        CallbackQueryHandler(botClient, update);
        
        if (update.Message == null) return;
        
        _cts.Cancel();
        var message = update.Message;
        var photo = update.Message.Photo;
        _cts = new ();

        if (message.Text != null && photo == null)
        {
            MessageHandler(botClient, update, message);
            return;
        }
        
        if (photo != null)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Смешно!");
            return;
        }
    }

    private async void MessageHandler(ITelegramBotClient botClient, Update update, Message message)
    {
        if (message.Text == null) _previousMessage = "";

        var loweredMessage = message.Text.ToLower();
        
        if (CheckCalculateRequest(botClient, update).Result) return;
        

        if(KeyWords.Greeting.Contains(loweredMessage))
        {
            _previousMessage = message.Text;

            await botClient.SendTextMessageAsync(message.Chat.Id, "Приветствую!");
            
        }
        else if(KeyWords.DataBase.Contains(loweredMessage))
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();
            string query = $"select * from Bot";
            SqlCommand command = new SqlCommand(query, _sqlConnection);
            adapter.SelectCommand = command;
            adapter.Fill(table);
            await botClient.SendTextMessageAsync(message.Chat.Id, "Приветствую!");
        }
        else if(KeyWords.Meals.Contains(loweredMessage))
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                // first row
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Завтрак с сухофруктами", callbackData: Meals.BreakfastWithDriedFruits.ToString()),
                    InlineKeyboardButton.WithCallbackData(text: "НЕТ", callbackData: "Clear"),
                }
            });
            
            await botClient.SendTextMessageAsync(message.Chat.Id, "Посчитать?", cancellationToken: _cts.Token,  replyMarkup: inlineKeyboard);
        }
        
        else if(KeyWords.Weather.Contains(message.Text.ToLower()))
        {
            await WeatherRequest(botClient, message);
        }
        else if(KeyWords.Payment.Contains(loweredMessage))
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                // first row
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "ДА", callbackData: "Calculate"),
                    InlineKeyboardButton.WithCallbackData(text: "НЕТ", callbackData: "Clear"),
                }
            });
            
            await botClient.SendTextMessageAsync(message.Chat.Id, "Посчитать?", cancellationToken: _cts.Token,  replyMarkup: inlineKeyboard);
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Напиши что-нибудь внятное:" );
        }
    }

    private async Task<bool> CheckCalculateRequest(ITelegramBotClient botClient, Update update)
    {
        if (!_calculateRequests.ContainsKey(update.Message.Chat.Id)) return false;
        
        if (double.TryParse(update.Message.Text, out var value))
        {
            var calculator = new PercentCalculator();
            var result = calculator.PrintResult(value);
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, result);
            _calculateRequests.Remove(update.Message.Chat.Id);
            return true;
        }
        
        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Некоректные данные! Попробуйте еще раз.");
        _calculateRequests.Remove(update.Message.Chat.Id);
        return true;
    }

    private async void CallbackQueryHandler(ITelegramBotClient botClient, Update update)
    {
        if (update.CallbackQuery == null) return;

        switch (update.CallbackQuery.Data)
        {
            case "Calculate":
            {
                if (update.CallbackQuery.Message == null) return;
                
                var id = update.CallbackQuery.Message.Chat.Id;
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Введите сумму!");
                _calculateRequests.Add(id, true);
                break;
            }
            case "Clear":
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Очистил!");
                break;
            case "BreakfastWithDriedFruits":
                var meal = new BreakfastWithDriedFruits(2, 3);
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, meal.PrintResult());
                break;
        }
    }
    
    private async Task WeatherRequest(ITelegramBotClient botClient, Message message)
    {
        
        using (var httpClient = new HttpClient())
        {
            // Create a cancellation token that will trigger after 5 seconds
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            try {
                // Make the request with the cancellation token
                var response = await httpClient.GetAsync($"http://api.openweathermap.org/data/2.5/weather?id=498817&units=metric&lang=ru&APPID={WeatherApiKey}", cts.Token);
            
                // Check the response status code
                response.EnsureSuccessStatusCode();
                
                Console.WriteLine("Success!");
                // Read the response content as a string
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic weatherData = JsonConvert.DeserializeObject(responseContent);
                Console.WriteLine(weatherData);
                // Extract the temperature and description from the weather data
                string temperature = weatherData.main.temp;
                string fillLike = weatherData.main.feels_like;
                await botClient.SendTextMessageAsync(message.Chat.Id, 
                    $"Погода в Санк-Петербурге: {temperature.Split('.')[0]}, " +
                    $"ощущается как {fillLike.Split('.')[0]}, {weatherData.weather[0].description}.");
                
            } catch (TaskCanceledException) {
                Console.WriteLine("Request was cancelled due to timeout.");
                await botClient.SendTextMessageAsync(message.Chat.Id,$"Request timed out.");
            } catch (Exception ex) {
                Console.WriteLine($"Request failed: {ex.Message}");
                await botClient.SendTextMessageAsync(message.Chat.Id,$"Request failed: {ex.Message}");
            } finally {
                cts.Dispose();
            }
        }
    }
    
    private Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
        throw new NotImplementedException();
    }
}

class BreakfastWithDriedFruits
{
    private readonly Meals _meal;
    private const double Cereals = 50;
    private const double DriedFruits = 30;
    private const double Kefir = 150;
    private const double СottageСheese = 80;
    private const double Apples = 250;
    private const double Chicken = 300;
    private const double Cheese = 30;
    private const double Egg = 1;
    private const double RiceFlour = 25;
    private const double SourСream = 30;
    private const double Bulgur = 30;
    private const double Сucumbers = 100;
    private const double Tomato = 100;
    private const double OliveOil = 17;
    
    private readonly int _persons;
    private readonly int _days;
    
    public BreakfastWithDriedFruits(int persons, int days)
    {
        _persons = persons;
        _days = days;
    }
    
    private double Calculate(double value, int persons, int days)
    {
        return value * persons * days;
    }
    
    public string PrintResult()
    {
        var cereals = Calculate(Cereals, _persons, _days);
        var driedFruits = Calculate(DriedFruits, _persons, _days);
        var kefir = Calculate(Kefir, _persons, _days);
        var cottageСheese = Calculate(СottageСheese, _persons, _days);
        var apples = Calculate(Apples, _persons, _days);
        var chicken = Calculate(Chicken, _persons, _days);
        var cheese = Calculate(Cheese, _persons, _days);
        var egg = Calculate(Egg, _persons, _days);
        var riceFlour = Calculate(RiceFlour, _persons, _days);
        var sourСream = Calculate(SourСream, _persons, _days);
        var bulgur = Calculate(Bulgur, _persons, _days);
        var сucumbers = Calculate(Сucumbers, _persons, _days);
        var tomato = Calculate(Tomato, _persons, _days);
        var oliveOil = Calculate(OliveOil, _persons, _days);

        var result = $"Овсяные хлопья: {cereals}\n" +
                     $"Сухофрукты: {driedFruits}\n" +
                     $"Кефир: {kefir}\n" +
                     $"Творог: {cottageСheese}\n" +
                     $"Яблоки: {apples}\n" +
                     $"Курица: {chicken}\n" +
                     $"Сыр: {cheese}\n" +
                     $"Яйца: {egg}\n" +
                     $"Рисовая мука: {riceFlour}\n" +
                     $"Сметана: {sourСream}\n" +
                     $"Булгур: {bulgur}\n" +
                     $"Огурцы: {сucumbers}\n" +
                     $"Помидоры: {tomato}\n" +
                     $"Оливковое масло: {oliveOil}\n";

        return result;
    }
}

internal enum Meals
{
    BreakfastWithDriedFruits = 0,
}