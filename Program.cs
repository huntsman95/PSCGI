using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Text;

namespace PSCGI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string defaultHeader = "Content type: text/html" + Environment.NewLine + Environment.NewLine;
            string psargs = "";
            string psargsErr = "";
            try
            {
                psargs = Environment.GetEnvironmentVariable("QUERY_STRING");
                psargs = Uri.UnescapeDataString(psargs);
                psargs = psargs.Replace("\"", "\"\"");
                string[] psargarray = psargs.Split('&');
                for (int i = 0; i < psargarray.Length; i++)
                {
                    psargarray[i] = Regex.Replace(psargarray[i], @"(\w+)=(.*)", "-$1 \"$2\"");
                }
                psargs = String.Join(" ", psargarray);
            }
            catch(Exception ex)
            {
                psargsErr = ex.Message;
            }

            if (Environment.GetEnvironmentVariable("REQUEST_METHOD") == "POST") //experimental POST data
            {
                System.IO.Stream s = Console.OpenStandardInput();
                System.IO.BinaryReader br = new System.IO.BinaryReader(s);
                string Length = Environment.GetEnvironmentVariable("CONTENT_LENGTH");
                int Size = Int32.Parse(Length);
                byte[] Data = new byte[Size];
                br.Read(Data, 0, Size);
                // *** don’t close the reader!
                Environment.SetEnvironmentVariable("POST_DATA", System.Text.Encoding.Default.GetString(Data, 0, Size));
            }

            /// EXECUTE SCRIPT SECTION

            string OutputBuffer = "";

            using (PowerShell PowerShellInst = PowerShell.Create())
            {
                PowerShellInst.AddScript(System.IO.File.ReadAllText(args[0]));
                Collection<PSObject> results = PowerShellInst.Invoke();

                // close the runspace

                StringBuilder stringBuilder = new StringBuilder();
                foreach (PSObject obj in results)
                {
                    stringBuilder.AppendLine(obj.ToString());
                }

                OutputBuffer = stringBuilder.ToString();
            }

            /// END EXEC SCRIPT SECTION

            //Process cmd = new Process();
            //cmd.StartInfo.FileName = "powershell.exe";
            //cmd.StartInfo.Arguments = "-noprofile -file \"" + args[0] + "\" " + psargs;
            //cmd.StartInfo.RedirectStandardInput = true;
            //cmd.StartInfo.RedirectStandardOutput = true;
            //cmd.StartInfo.RedirectStandardError = true;
            //cmd.StartInfo.CreateNoWindow = true;
            //cmd.StartInfo.UseShellExecute = false;
            //cmd.Start();
            //
            //string OutputBuffer = "";
            //string ErrorBuffer = "";
            //
            //if(psargsErr != "")
            //{
            //    ErrorBuffer += psargsErr + "\n";
            //}
            //
            //while (!cmd.HasExited)
            //{
            //    OutputBuffer += cmd.StandardOutput.ReadToEnd();
            //    ErrorBuffer += cmd.StandardError.ReadToEnd();
            //}

                //OutputBuffer = OutputBuffer.Trim('\r', '\n');
                //
                //if (!(Regex.Match(OutputBuffer, "Content-type:.*").Success))
                //{
                //    OutputBuffer = defaultHeader + OutputBuffer;
                //}
                //
                ////if (ErrorBuffer != "")
                ////{
                ////    Console.WriteLine("Content-type: text/plain" + Environment.NewLine);
                ////    Console.WriteLine("The PowerShell script terminated with the following error:" + Environment.NewLine);
                ////    Console.Write(ErrorBuffer);
                ////}
                //else
                //{
                    Console.Write(OutputBuffer);
                //}
        }
    }
}
