using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        var controllersPath = @"C:\Users\vanshika.gupta\OneDrive - InTimeTec Visionsoft Pvt. Ltd.,\Desktop\Project_&_Resource_Management_Tool\PRMTool\PRMTool\WebAPI\Controllers";
        var files = Directory.GetFiles(controllersPath, "*.cs");

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            if (!content.Contains("[Authorize") && !file.EndsWith("UsersController.cs") && !file.EndsWith("AuthController.cs"))
            {
                var newContent = content.Replace("using Microsoft.AspNetCore.Mvc;", "using Microsoft.AspNetCore.Authorization;\r\nusing Microsoft.AspNetCore.Mvc;");
                newContent = newContent.Replace("[ApiController]", "[ApiController]\r\n[Authorize]");
                File.WriteAllText(file, newContent);
                Console.WriteLine("Authorized " + Path.GetFileName(file));
            }
        }
    }
}
