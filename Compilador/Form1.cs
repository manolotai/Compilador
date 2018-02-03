using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Compilador.Analizadores.Lexico;

namespace Compilador {
    public partial class __FrmMain : Form {
        public __FrmMain()
        {
            InitializeComponent();
            using ( StreamReader readStrm = new StreamReader(@"C: \Users\Manolo\Desktop\lexico.txt"))
                __RTxtCsFile.Text = readStrm.ReadToEnd();
        }

        private void __BtnCompilar_Click(object sender, EventArgs e)
        {
            var path0 = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent;
            string path = Directory.GetCurrentDirectory();
            Console.WriteLine(path0.FullName);
            using (StreamWriter writeStrm = new StreamWriter(@"C: \Users\Manolo\Desktop\lexico.txt", false, Encoding.ASCII))
                writeStrm.Write(__RTxtCsFile.Text);
            using (StreamReader readStrm = new StreamReader(@"C: \Users\Manolo\Desktop\lexico.txt")) {
                Lexico test = new Lexico(readStrm);
                Token testToken;
                while ((testToken = test.NextToken()).Valor != "") {
                    Console.WriteLine(testToken.Valor);
                    Console.WriteLine((Lexico.IDTokens)testToken.ID);
                }
            }
        }
    }
}
