using OpenQA.Selenium;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PruebasUIBased.Infrastructure
{
    public static class ScreenshotHelper
    {
        public static string TakeScreenshot(IWebDriver driver, string scenarioTitle, string status)
        {
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string folderPath = Path.Combine(basePath, "Screenshots");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string cleanTitle = Regex.Replace(scenarioTitle, "[^a-zA-Z0-9-_]", "_");
                if (cleanTitle.Length > 50) cleanTitle = cleanTitle.Substring(0, 50);
                string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{status}_{cleanTitle}_{timeStamp}.png";
                string fullPath = Path.Combine(folderPath, fileName);
                ITakesScreenshot camera = (ITakesScreenshot)driver;
                Screenshot screenshot = camera.GetScreenshot();
                screenshot.SaveAsFile(fullPath);

                Console.WriteLine($"📸 Screenshot guardado en: {fullPath}");
                return fullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se pudo tomar el screenshot: {ex.Message}");
                return null;
            }
        }
    }
}