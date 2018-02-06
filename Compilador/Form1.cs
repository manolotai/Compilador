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
        private string PathProyect;
        public __FrmMain()
        {
            InitializeComponent();
            PathProyect = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var listFiles = new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles("Text.cs");
            if(listFiles.Count() == 0) {
                using (var writeStrm = new StreamWriter(PathProyect + @"\Text.cs", false, Encoding.ASCII)) {
                    writeStrm.Write("//Archivo Nuevo " + (char)10);
                }
                using (var readStrm = new StreamReader((PathProyect + @"\Text.cs")))
                    __RTxtCsFile.Text = readStrm.ReadToEnd();
            } else {
                using (var readStrm = new StreamReader(PathProyect + @"\Text.cs"))
                    __RTxtCsFile.Text = readStrm.ReadToEnd();
            }
            
        }

        private void __BtnCompilar_Click(object sender, EventArgs e)
        {
            using (var writeStrm = new StreamWriter(PathProyect + @"\Text.cs", false, Encoding.ASCII))
                writeStrm.Write(__RTxtCsFile.Text);
            using (var readStrm = new StreamReader((PathProyect + @"\Text.cs"))) {
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
