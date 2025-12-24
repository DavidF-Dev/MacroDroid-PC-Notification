
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;

ToastNotificationManagerCompat.OnActivated += OnToastNotificationActivated;

// Create assets directory
string assetsDirPath = Path.Combine(Environment.CurrentDirectory, "assets");
if (!Directory.Exists(assetsDirPath))
{
    Directory.CreateDirectory(assetsDirPath);
}

// Download assets
string messagesAssetFilePath = Path.Combine(assetsDirPath, "logo_messages.png");
DownloadFileAsync(messagesAssetFilePath, "https://ssl.gstatic.com/android-messages-web/images/2022.3/1x/messages_2022_96dp.png");

// Re-create data directory
string dataDirPath = Path.Combine(Environment.CurrentDirectory, "data");
if (Directory.Exists(dataDirPath))
{
    Directory.Delete(dataDirPath, true);
}

Directory.CreateDirectory(dataDirPath);

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
    Console.WriteLine(Encoding.UTF8.GetString(data));
    Console.WriteLine();

    // Parse data
    string json = Encoding.UTF8.GetString(data);
    NotificationData notification = JsonConvert.DeserializeObject<NotificationData>(json);
    
    // Decrypt notification text if necessary
    string notificationText = notification.Text;
    if (notification.Encrypted)
    {
        // TODO
    }

    notificationText = notificationText.Trim();
    
    // Write to history file
    string dataFilePath = Path.Combine(dataDirPath, string.Join("_", (notification.App + "_" + notification.Title).Replace(' ', '_').Split(Path.GetInvalidFileNameChars())) + ".txt");
    bool newLine = File.Exists(dataFilePath);
    using (StreamWriter sw = File.AppendText(dataFilePath))
    {
        sw.Write($"{(newLine ? "\n\n" : "")}[{DateTime.Now:g}]\n{notificationText}");
    }
    
    // Display notification
    ToastContentBuilder builder = new();
    builder.AddArgument("not_text", notificationText);
    builder.AddArgument("file_path", dataFilePath);
    builder.AddAppLogoOverride(new Uri(messagesAssetFilePath));
    builder.AddText(notification.App);
    builder.AddText(notification.Title);
    builder.AddText(notificationText);
    builder.Show();
}

static void OnToastNotificationActivated(ToastNotificationActivatedEventArgsCompat e)
{
    Console.WriteLine("Activated: " + e.Argument);
    
    ToastArguments args = ToastArguments.Parse(e.Argument);
    string notificationText = args.Get("not_text");
    string filePath = args.Get("file_path");

    // Open the URL
    if (Uri.TryCreate(notificationText, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
    {
        Process.Start(new ProcessStartInfo(uri.ToString()) { Verb = "open", UseShellExecute = true});
    }
    
    // Open the history file
    else if (File.Exists(filePath) && Path.GetExtension(filePath) == ".txt")
    {
        Process.Start(new ProcessStartInfo(filePath) {Verb = "open", UseShellExecute = true});
    }
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

