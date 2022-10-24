using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace PSCGI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string defaultHeader = "Content type: text/html" + Environment.NewLine + Environment.NewLine;
            string psargs = "";
            try
            {
                psargs = HttpUtility.UrlDecode(Environment.GetEnvironmentVariable("QUERY_STRING"));
                psargs = psargs.Replace("\"", "\"\"");
                string[] psargarray = psargs.Split('&');
                for (int i = 0; i < psargarray.Length; i++)
                {
                    psargarray[i] = Regex.Replace(psargarray[i], @"(\w+)=(.*)", "-$1 \"$2\"");
                }
                psargs = String.Join(" ", psargarray);
            }
            catch
            {

            }

            Process cmd = new Process();
            cmd.StartInfo.FileName = "powershell.exe";
            cmd.StartInfo.Arguments = "-noprofile -file \"" + args[0] + "\" " + psargs;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            string OutputBuffer = "";
            string ErrorBuffer = "";

            while (!cmd.HasExited)
            {
                OutputBuffer += cmd.StandardOutput.ReadToEnd();
                ErrorBuffer += cmd.StandardError.ReadToEnd();
            }

            if (!(Regex.Match(OutputBuffer, "Content-type:.*").Success))
            {
                OutputBuffer = defaultHeader + OutputBuffer;
            }

            //cmd.WaitForExit();
            OutputBuffer = OutputBuffer.Trim('\r', '\n');
            if (ErrorBuffer != "")
            {
                Console.WriteLine("Content-type: text/plain" + Environment.NewLine);
                Console.WriteLine("The PowerShell script terminated with the following error:" + Environment.NewLine);
                Console.Write(ErrorBuffer);
            }
            else
            {
                Console.Write(OutputBuffer);
            }
        }
    }
}
