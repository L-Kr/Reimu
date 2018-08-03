using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace test
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new Form1());
            }
            catch(Exception e)
            {
                FileStream error_file = new FileStream("error.txt", FileMode.Create);
                StreamWriter error_write = new StreamWriter(error_file);
                error_write.WriteLine(e);
                error_write.WriteLine();
                error_write.Close();
                error_file.Close();
            }
        }

    }
}