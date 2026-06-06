using System;
using System.IO;
using System.Text.RegularExpressions;

class Program {
    static void Main() {
        string dir = @"e:\SWP\SealHackathonBackEnd\SealHackathon.API\Controllers";
        foreach(var file in Directory.GetFiles(dir, "*.cs")) {
            string text = File.ReadAllText(file);
            if (!text.Contains("SealHackathon.Domain.Constants")) {
                text = text.Replace("using Microsoft.AspNetCore.Mvc;", "using Microsoft.AspNetCore.Mvc;\nusing SealHackathon.Domain.Constants;");
            }
            text = text.Replace("[Authorize(Roles = "Coordinator")]", "[Authorize(Roles = RoleConstants.Coordinator)]");
            text = text.Replace("[Authorize(Roles = "Judge")]", "[Authorize(Roles = RoleConstants.Judge)]");
            text = text.Replace("[Authorize(Roles = "Coordinator,Judge")]", "[Authorize(Roles = RoleConstants.Coordinator + "," + RoleConstants.Judge)]");
            text = text.Replace("[Authorize(Roles = "Judge,Coordinator")]", "[Authorize(Roles = RoleConstants.Judge + "," + RoleConstants.Coordinator)]");
            File.WriteAllText(file, text);
        }
    }
}
