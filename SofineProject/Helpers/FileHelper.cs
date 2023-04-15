namespace SofineProject.Helpers
{
    public class FileHelper
    {
        public async static void DeleteFile(string fileName, IWebHostEnvironment env, params string[] folders)
        {
            string filePath = Path.Combine(env.WebRootPath);

            foreach (string folder in folders)
            {
                filePath = Path.Combine(filePath, folder);

            }
            filePath = Path.Combine(filePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
