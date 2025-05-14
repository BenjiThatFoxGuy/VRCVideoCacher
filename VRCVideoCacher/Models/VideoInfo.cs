// ReSharper disable InconsistentNaming
namespace VRCVideoCacher.Models;

public enum UrlType
{
    YouTube,
    PyPyDance,
    VRDancing,
    SoundCloud,
    PornHub,
    Other
}

public enum DownloadFormat
{
    MP4,
    Webm,
    opus,
    ogg,
    m4a,
    mp3
}

public class VideoInfo
{
    public required string VideoUrl;
    public required string VideoId;
    public required UrlType UrlType;
    public required DownloadFormat DownloadFormat;
}
