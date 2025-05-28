using System.Text;

namespace NIMSAP_Server;

// Логгер
public static class Logger
{
    private static string logPath;
    private static StreamWriter sw;

    // Инициализация логгера
    public static void Initialise(string name)
    {
        for (int num = 1; num <= 100; num++)
        {
            try
            {
                File.Create(name + num + ".txt").Close();
                logPath = name + num + ".txt";
                sw = new StreamWriter(logPath);
                break;
            }
            catch (Exception e)
            {
                // Console.WriteLine(e.Message);
            }
        }
    }
    
    // Запись в логи
    public static async void Log(string message, Exception error = null)
    {
        message = $"<{DateTime.Now.TimeOfDay}> {message}";
        Console.WriteLine(message);
        await sw.WriteLineAsync(message);
        if (error != null)
        {
            message = $"Источник: {error.StackTrace}\nОшибка: {error.Message}";
            await sw.WriteLineAsync(message);
        }
        sw.Flush();
    }
}