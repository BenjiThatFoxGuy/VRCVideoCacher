using Newtonsoft.Json;
using Serilog;
using VRCVideoCacher.YTDL;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace VRCVideoCacher;

public class ConfigManager
{
    public static readonly ConfigModel Config;
    private static readonly ILogger Log = Program.Logger.ForContext<ConfigManager>();
    private static readonly string configFilePath;

    static ConfigManager()
    {
        Log.Information("Loading config...");
        if (OperatingSystem.IsWindows())
            configFilePath = Path.Combine(Program.CurrentProcessPath, "Config.json");
        else
            configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "VRCVideoCacher/Config.json");
        Log.Debug("Using config file path: {ConfigFilePath}", configFilePath);

        Directory.CreateDirectory(Path.GetDirectoryName(configFilePath) ?? throw new Exception("Failed to get config folder path"));
        if (!File.Exists(configFilePath))
        {
            Config = new ConfigModel();
            FirstRun();
        }
        else
        {
            Config = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(configFilePath)) ?? new ConfigModel();
        }
        if (Config.ytdlWebServerURL.EndsWith('/'))
            Config.ytdlWebServerURL = Config.ytdlWebServerURL.TrimEnd('/');
        
        Log.Information("Loaded config.");
        TrySaveConfig();
    }

    private static void TrySaveConfig()
    {
        var newConfig = JsonConvert.SerializeObject(Config, Formatting.Indented);
        var oldConfig = File.Exists(configFilePath) ? File.ReadAllText(configFilePath) : string.Empty;
        if (newConfig == oldConfig)
            return;
        
        Log.Information("Config changed, saving...");
        File.WriteAllText(configFilePath, JsonConvert.SerializeObject(Config, Formatting.Indented));
        Log.Information("Config saved.");
    }
    
    public static string GetYtdlArgs()
    {
        // If override is set and not empty, use it instead of additional args
        if (!string.IsNullOrEmpty(Config.ytdlArgsOverride))
            return Config.ytdlArgsOverride;
        
        // Otherwise use additional args (original behavior)
        return Config.ytdlAdditionalArgs;
    }

    private static bool GetUserConfirmation(string prompt, bool defaultValue)
    {
        var defaultOption = defaultValue ? "Y/n" : "y/N";
        var message = $"{prompt} ({defaultOption}):";
        message = message.TrimStart();
        Log.Information(message);
        var input = Console.ReadLine();
        return string.IsNullOrEmpty(input) ? defaultValue : input.Equals("y", StringComparison.CurrentCultureIgnoreCase);
    }

    private static void FirstRun()
    {
        Log.Information("It appears this is your first time running VRCVideoCacher. Let's create a basic config file.");

        Config.CacheYouTube = GetUserConfirmation("Would you like to cache/download Youtube videos?", true);
        if (Config.CacheYouTube)
        {
            var maxResolution = GetUserConfirmation("Would you like to cache/download Youtube videos in 4k?", true);
            Config.CacheYouTubeMaxResolution = maxResolution ? 2160 : 1080;
        }

        var vrDancingPyPyChoice = GetUserConfirmation("Would you like to cache/download VRDancing & PyPyDance videos?", true);
        Config.CacheVRDancing = vrDancingPyPyChoice;
        Config.CachePyPyDance = vrDancingPyPyChoice;

        Log.Information("Would you like to use the companion extension to fetch youtube cookies? (This will fix bot errors, requires installation of the extension)");
        Log.Information("Extension can be found here: https://github.com/clienthax/VRCVideoCacherBrowserExtension");
        Config.ytdlUseCookies = GetUserConfirmation("", true);

        if (OperatingSystem.IsWindows() && GetUserConfirmation("Would you like to add VRCVideoCacher to VRCX auto start?", true))
        {
            AutoStartShortcut.CreateShortcut();
        }

        if (YtdlManager.GlobalYtdlConfigExists() && GetUserConfirmation(@"Would you like to delete global YT-DLP config in %AppData%\yt-dlp\config?", true))
        {
            YtdlManager.DeleteGlobalYtdlConfig();
        }
    }
}

// ReSharper disable InconsistentNaming
public class ConfigModel
{
    public string ytdlWebServerURL = "http://localhost:9696";
    public string ytdlPath = OperatingSystem.IsWindows() ? "Utils\\yt-dlp.exe" : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VRCVideoCacher/Utils/yt-dlp");
    public bool ytdlUseCookies = true;
    public bool ytdlAutoUpdate = true;
    public string ytdlAdditionalArgs = string.Empty;
    public string ytdlArgsOverride = string.Empty;
    public string ytdlDubLanguage = "en";
    public int ytdlDelay = 0;
    public string avproOverride = "false";
    public string CachedAssetPath = "CachedAssets";
    public string[] BlockedUrls = new[] { "https://na2.vrdancing.club/sampleurl.mp4" };
    public bool CacheYouTube = true;
    public int CacheYouTubeMaxResolution = 1080;
    public int CacheYouTubeMaxLength = 180;
    public float CacheMaxSizeInGb = 32;
    public bool CachePyPyDance = false;
    public bool CacheVRDancing = false;

    public bool AutoUpdate = true;
    public string[] PreCacheUrls = [];
}
// ReSharper restore InconsistentNaming