using BotSupport;

Console.WriteLine("Start!");

const string TOKEN = "6120355824:AAETsxL1WdjalNHJlM3bw7rg739aggEp-4Y";

var dataBase = new DataBase();
dataBase.Open();
var bot = new CreateBot(dataBase.CurrentSqlConnection);
bot.CreateBotMethod(TOKEN);

Console.ReadLine();
Console.WriteLine("End!");