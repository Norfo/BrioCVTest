using System.IO;

namespace BrioTest
{
    /// <summary>
    /// This class needs to select the data source.
    /// </summary>
    public class SourceFabric
    {
        public ISourceable GetSourceObject(string path)
        {
            SourceType sType = GetSourceTypeByExtension(path);

            switch (sType)
            {
                case SourceType.Image:
                    return new ImageSource(path);
                case SourceType.Video:
                    return new VideoSource(path);
                case SourceType.Default:
                default:
                    return null;
            }
        }

        private SourceType GetSourceTypeByExtension(string path)
        {
            string ext = Path.GetExtension(path);

            switch (ext)
            {
                case ".jpg":
                case ".png":
                case ".bmp":
                    return SourceType.Image;
                case ".mpg":
                case ".mp4":
                    return SourceType.Video;
                default:
                    return SourceType.Default;
            }
        }
    }
}
