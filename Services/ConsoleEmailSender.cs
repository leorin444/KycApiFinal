public class ConsoleEmailSender : IEmailSender
{
    public Task SendAsync(string to, string subject, string body)
    {
        Console.WriteLine("=== EMAIL SEND (DEV) ===");
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {body}");
        return Task.CompletedTask;
    }
}
