using System;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Wav2H
{
    class Program
    {
        static string Do<T>(string[] args, int index, string filter) where T : FileDialog, new()
        {
            string value = null;
            if(index < args.Length)
            {
                value = args[index];
            }
            else
            {
                using(var dialog = new T())
                {
                    dialog.Filter = filter + "|All files|*.*";
                    dialog.AddExtension = true;
                    var result = dialog.ShowDialog();
                    if(result == DialogResult.OK)
                    {
                        value = dialog.FileName;
                    }
                }
            }

            return value;
        }

        [STAThread]
        static void Main(string[] args)
        {
            string inFileName = Do<OpenFileDialog>(args, 0, "WAV Files|*.wav"),
                outFileName = Do<SaveFileDialog>(args, 1, "C Header Files | *.h");

            if(inFileName != null && outFileName != null)
            {
                var sb = new StringBuilder("unsigned char data[] = {");
                var hasRead = false;
                using(var fs = new FileStream(inFileName, FileMode.Open))
                {
                    var buffer = new byte[255];
                    int size;
                    while((size = fs.Read(buffer, 0, 255)) > 0)
                    {
                        if(hasRead)
                        {
                            sb.Append(",");
                        }
                        hasRead = true;
                        sb.Append(String.Join(",", buffer));
                    }
                }
                sb.Append("};");
                File.WriteAllText(outFileName, sb.ToString());
            }
        }
    }
}
