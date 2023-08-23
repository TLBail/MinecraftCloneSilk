using ChromeTracing.NET;

namespace UnitTest;

public class LoggerTest
{
    [Test]
    public void testLogger() {
        Directory.SetCurrentDirectory("./../../../../");
        ChromeTrace.Init();

        using (ChromeTrace.Profile("Test"))
        {
            Random rand = new Random();
            int sleep = 1000 + rand.Next(1000);

            Console.WriteLine("Night night!");

            System.Threading.Thread.Sleep(sleep);

            Console.WriteLine($"Woke up after {sleep} ms");
        }
        ChromeTrace.Dispose();
    }
}