namespace BotSupport;

public class PercentCalculator
{
    private const double Weekend = 22;
    private const double Travel  = 4.7;
    private const double Fund  = 15;
    private const double Payments  = 16.5;
    private const double Credits  = 6.5;

    public string PrintResult(double value)
    {
        var weekend = CalculatePercent(value, Weekend);
        var travel = CalculatePercent(value, Travel);
        var fund = CalculatePercent(value, Fund);
        var payments = CalculatePercent(value, Payments);
        var credits = CalculatePercent(value, Credits);
        var last = value - weekend - travel - fund - payments - credits;
        return $"На отпуск: {weekend} => {Weekend}%\n" +
               $"На проезд: {travel} => {Travel}%\n" +
               $"НЗ: {fund} => {Fund}%\n" +
               $"На ком. платежи: {payments} => {Payments}%\n" +
               $"На кредит: {credits} => {Credits}%\n" +
               $"\n" +
               $"Маше: {weekend}\n"+
               $"Alfa остаток: {Math.Truncate(last * 100) / 100}\n" +
               $"На сбер => {fund + payments + credits}\n" +
               $"На ВТБ => {travel}";
    }
    
    private double CalculatePercent(double value, double percent)
    {
        return value * percent / 100;
    }
}