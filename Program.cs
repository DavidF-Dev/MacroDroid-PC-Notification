
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;

// Download assets
string logoMessagesFilePath = Path.Combine(Environment.CurrentDirectory, "logo_messages.png");
DownloadFileAsync(logoMessagesFilePath, "https://ssl.gstatic.com/android-messages-web/images/2022.3/1x/messages_2022_96dp.png");

// Listen for broadcasts
const int port = 5000;
using UdpClient client = new(port);
IPEndPoint endPoint = new(IPAddress.Any, port);
while (true)
{
    // Receive data
    byte[] data = client.Receive(ref endPoint);
    Console.WriteLine($"Received broadcast message from client {endPoint}");
    Console.Write("Decoded data is: ");
    Console.WriteLine(Encoding.ASCII.GetString(data));
    Console.WriteLine();

    // Parse data
    string json = Encoding.ASCII.GetString(data);
    NotificationData notification = JsonConvert.DeserializeObject<NotificationData>(json);
    
    // Write to history file
    string fileName = Path.Combine(Environment.CurrentDirectory, notification.App + "_" + notification.Title + ".txt");
    bool newLine = File.Exists(fileName);
    using (StreamWriter sw = File.AppendText(fileName))
    {
        sw.Write($"{(newLine ? '\n' : "")}[{DateTime.Now:g}]\n{notification.Text}");
    }
    
    // Display notification
    ToastContentBuilder builder = new();
    builder.AddAppLogoOverride(new Uri(logoMessagesFilePath));
    builder.AddText(notification.App);
    builder.AddText(notification.Title);
    builder.AddText(notification.Text);
    builder.Show();
}

static async void DownloadFileAsync(string fileName, string url, bool replace = false)
{
    try
    {
        if (!replace && File.Exists(fileName))
        {
            return;
        }
            
        using HttpClient httpClient = new();
        await using Stream s = await httpClient.GetStreamAsync(url);
        await using FileStream fs = new(fileName!, FileMode.OpenOrCreate);
        await s.CopyToAsync(fs);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

internal readonly struct NotificationData
{
    [JsonProperty("not_app")]
    public readonly string App;
    
    [JsonProperty("not_title")]
    public readonly string Title;
    
    [JsonProperty("not_text")]
    public readonly string Text;

    [JsonProperty("encrypted")]
    public readonly bool Encrypted;
}

