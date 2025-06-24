namespace EventManagerWebRazorPage.Services.Other
{
    public interface IPathProvider
    {
        string MapPath(string path);
    }
    public class PathProvider : IPathProvider
    {
        private IWebHostEnvironment _hostEnvironment;
        public PathProvider(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        string IPathProvider.MapPath(string path)
        {
            var newPath = Path.Combine(_hostEnvironment.WebRootPath, path);
            if(!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            return newPath;
        }
    }
}
