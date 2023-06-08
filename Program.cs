namespace dumpbin_compare
{
  internal class Program
  {
    static void Main(string[] args)
    {
      string workingDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
      System.IO.Directory.SetCurrentDirectory(workingDir);

      string msDevtoolsDirectory = @"C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Tools\MSVC\14.35.32215\bin\Hostx64\x64\";
      string dumpbin = msDevtoolsDirectory + "dumpbin.exe";
      string undname = msDevtoolsDirectory + "undname.exe";

      // run dumpbin on opennurbs in Rhino 7
      string opennurbs7 = @"C:\Program Files\Rhino 7\System\opennurbs.dll";
      var proc7 = System.Diagnostics.Process.Start(dumpbin, $"/exports /out:v7dump.txt \"{opennurbs7}\"");
      string opennurbs8 = @"C:\Program Files\Rhino 8 WIP\System\opennurbs.dll";
      var proc8 = System.Diagnostics.Process.Start(dumpbin, $"/exports /out:v8dump.txt \"{opennurbs8}\"");

      // put all of the exported functions for v8 in a set and make sure that all of the v7
      // exports still exist in v8
      proc8.WaitForExit();
      string[] v8Lines = File.ReadAllLines("v8dump.txt");
      var v8Functions = new SortedSet<string>();
      foreach(var line in v8Lines)
      {
        if(line.Length > 27)
        {
          v8Functions.Add(line.Substring(27));
        }
      }

      proc7.WaitForExit();
      string[] v7Lines = File.ReadAllLines("v7dump.txt");
      var missing = new List<string>();
      foreach(var line in v7Lines)
      {
        if (line.Length > 27)
        {
          if (v8Functions.Contains(line.Substring(27)))
            continue;

          missing.Add(line);
        }
      }

      System.IO.File.WriteAllLines(@"missing.txt", missing);
      var p = new System.Diagnostics.Process();
      p.StartInfo.UseShellExecute = false;
      p.StartInfo.RedirectStandardOutput = true;
      p.StartInfo.FileName = undname;
      p.StartInfo.Arguments = "missing.txt";
      p.Start();
      string output = p.StandardOutput.ReadToEnd();
      p.WaitForExit();
      var lines = output.Split('\n');
      missing.Clear();
      foreach (var line in lines)
      {
        if (line.Length > 27)
          missing.Add(line.Substring(26).Trim());
      }
      System.IO.File.WriteAllLines(@"missing.txt", missing);
    }
  }
}