using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;

namespace ConnectAnnouncer;

public class PluginConfig
{
    [Description("Chat message format. {NAME} = player, {LOCATION} = geo location.")]
    public string MessageFormat { get; set; } = "[green]{NAME}[default] Shemogviertda | [lightred]{LOCATION}";

    [Description("Default text when location cannot be resolved.")]
    public string UnknownLocationText { get; set; } = "Ucnobi";

    [Description("Text shown for localhost or private LAN IPs.")]
    public string LocalLocationText { get; set; } = "Lokaluri";
}

[PluginMetadata(Id = "ConnectAnnouncer", Version = "1.0.0", Name = "Connect Announcer", Author = "xaboo", Description = "High-performance connection announcer with GeoIP")]
public partial class ConnectAnnouncer : BasePlugin
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(3)
    };

    private static readonly ConcurrentDictionary<string, string> GeoCache = new();
    private CancellationTokenSource? _cts;
    private bool _configInitialized;

    public PluginConfig Config { get; private set; } = new();

    public ConnectAnnouncer(ISwiftlyCore core) : base(core)
    {
    }

    public override void ConfigureSharedInterface(IInterfaceManager interfaceManager)
    {
    }

    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
    }

    public override void Load(bool hotReload)
    {
        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
        catch { }

        _cts = new CancellationTokenSource();

        try
        {
            // უსაფრთხო ინიციალიზაცია: ეშვება მხოლოდ ერთხელ პროცესის განმავლობაში
            if (!_configInitialized)
            {
                Core.Configuration
                    .InitializeJsonWithModel<PluginConfig>("config.jsonc", "Main")
                    .Configure(b => b.AddJsonFile(Core.Configuration.GetConfigPath("config.jsonc"), optional: true, reloadOnChange: true));
                
                _configInitialized = true;
            }

            Config = Core.Configuration.Manager.GetSection("Main").Get<PluginConfig>() ?? new PluginConfig();
            Core.Logger.LogInformation($"[ConnectAnnouncer] Config path: {Core.Configuration.GetConfigPath("config.jsonc")}");
        }
        catch (Exception ex)
        {
            Core.Logger.LogError($"[ConnectAnnouncer] Config Init Error: {ex.Message}");
        }

        Core.Event.OnClientPutInServer += OnPutInServer;

        Core.Logger.LogInformation("[ConnectAnnouncer] Successfully loaded by xaboo!");
    }

    public override void Unload()
    {
        Core.Event.OnClientPutInServer -= OnPutInServer;

        try
        {
            _cts?.Cancel();
        }
        catch { }

        Core.Logger.LogInformation("[ConnectAnnouncer] Unloaded.");
    }

    private void OnPutInServer(IOnClientPutInServerEvent e)
    {
        try
        {
            int playerId = e.PlayerId;
            var player = Core.PlayerManager.GetPlayer(playerId);
            
            if (player == null || player.IsFakeClient)
                return;

            string rawName = string.IsNullOrWhiteSpace(player.Name) ? "Player" : player.Name;
            string playerName = rawName
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "");

            string ipAddress = player.IPAddress;

            Core.Logger.LogInformation($"[ConnectAnnouncer] Joiner: {playerName} (IP: {ipAddress})");

            string messageFormat = Config.MessageFormat;
            string unknownText = Config.UnknownLocationText;
            string localText = Config.LocalLocationText;

            var token = _cts?.Token ?? CancellationToken.None;

            _ = Task.Run(async () =>
            {
                try
                {
                    if (token.IsCancellationRequested) return;

                    string location = await GetLocationAsync(ipAddress, unknownText, localText, token);
                    string sanitizedLocation = location.Replace("{", "").Replace("}", "");

                    string formattedMessage = messageFormat
                        .Replace("{NAME}", playerName)
                        .Replace("{LOCATION}", sanitizedLocation);

                    if (token.IsCancellationRequested) return;

                    Core.Scheduler.NextTick(() =>
                    {
                        if (token.IsCancellationRequested) return;

                        var p = Core.PlayerManager.GetPlayer(playerId);
                        if (p == null || p.Name != rawName) return;

                        Core.PlayerManager.SendChat(formattedMessage);
                    });
                }
                catch (OperationCanceledException) { }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    Core.Logger.LogError($"[ConnectAnnouncer] Task Error: {ex.Message}");
                }
            }, token);
        }
        catch (Exception ex)
        {
            Core.Logger.LogError($"[ConnectAnnouncer] PutInServer Error: {ex.Message}");
        }
    }

    private async Task<string> GetLocationAsync(string? rawIp, string unknownText, string localText, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(rawIp))
            return unknownText;

        string cleanIp;

        if (IPAddress.TryParse(rawIp, out var parsedIp))
        {
            if (parsedIp.IsIPv4MappedToIPv6)
                parsedIp = parsedIp.MapToIPv4();

            if (parsedIp.AddressFamily != AddressFamily.InterNetwork)
                return unknownText;

            cleanIp = parsedIp.ToString();
        }
        else
        {
            cleanIp = rawIp.Split(':')[0].Trim();
        }

        if (IsPrivateOrLocalIp(cleanIp))
            return localText;

        if (GeoCache.TryGetValue(cleanIp, out var cachedLocation))
        {
            return cachedLocation;
        }

        try
        {
            string url = $"http://ip-api.com/json/{cleanIp}?fields=status,message,country,city";
            var response = await HttpClient.GetFromJsonAsync<GeoIpResponse>(url, token);

            if (response != null)
            {
                if (response.Status == "success")
                {
                    string country = response.Country ?? unknownText;
                    string city = response.City ?? "";

                    string resultLocation = !string.IsNullOrEmpty(city) 
                        ? $"{country}, {city}" 
                        : country;

                    if (GeoCache.Count < 2000)
                    {
                        GeoCache[cleanIp] = resultLocation;
                    }

                    return resultLocation;
                }
                else
                {
                    Core.Logger.LogWarning($"[ConnectAnnouncer] GeoIP API returned status '{response.Status}': {response.Message ?? "Unknown"}");
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        catch (Exception ex)
        {
            Core.Logger.LogError($"[ConnectAnnouncer] GeoIP Request Exception: {ex.Message}");
        }

        return unknownText;
    }

    private static bool IsPrivateOrLocalIp(string ipString)
    {
        if (!IPAddress.TryParse(ipString, out var ip))
            return false;

        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }

        if (IPAddress.IsLoopback(ip))
            return true;

        if (ip.AddressFamily != AddressFamily.InterNetwork)
            return false;

        byte[] bytes = ip.GetAddressBytes();
        
        if (bytes[0] == 10) return true;
        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
        if (bytes[0] == 192 && bytes[1] == 168) return true;
        if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127) return true;

        return false;
    }

    private class GeoIpResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
    }
}