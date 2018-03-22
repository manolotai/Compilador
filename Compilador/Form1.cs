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
using Compilador.Analizadores.Sintaxis;
using System.Diagnostics;

namespace Compilador {
    public partial class __FrmMain : Form {
        private string _PathProyect;
        private string _InitPath;
        private DataTable _TablaTokens;
        private DataTable _TablaAtributos;

        public __FrmMain()
        {
            InitializeComponent();
            _InitPath = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            _PathProyect = _InitPath;
            var listFiles = new DirectoryInfo(_PathProyect).GetFiles("Text.cs");

            if(listFiles.Count() == 0) {
                using (var writeStrm = new StreamWriter(_PathProyect + @"\Text.cs", false, Encoding.ASCII)) {
                    writeStrm.Write("//Archivo Nuevo " + (char)10);
                }
                using (var readStrm = new StreamReader((_PathProyect + @"\Text.cs")))
                    __TxtRCsFile.Text = readStrm.ReadToEnd();
            } else {
                using (var readStrm = new StreamReader(_PathProyect + @"\Text.cs"))
                    __TxtRCsFile.Text = readStrm.ReadToEnd();
            }
            _PathProyect += @"\Text.cs";
            _TablaTokens = new DataTable();
            _TablaAtributos = new DataTable();
            
            _TablaTokens.Columns.Add("Clasificacion", typeof(string));
            _TablaTokens.Columns.Add("Valor", typeof(string));
            __DataGVTokens.DataSource = _TablaTokens;

            _TablaAtributos.Columns.Add("Nombre", typeof(string));
            _TablaAtributos.Columns.Add("Valor", typeof(string));
            _TablaAtributos.Columns.Add("Tipo", typeof(string));
            _TablaAtributos.Columns.Add("Accesor", typeof(string));
            __DataGVAtributos.DataSource = _TablaAtributos;
        }

        private void __BtnCompilar_Click(object sender, EventArgs e)
        {
            Console.Clear();
            __TxtRConsola.Text = "";
            _TablaTokens.Clear();
            _TablaAtributos.Clear();
            using (var writeStrm = new StreamWriter(_PathProyect, false, Encoding.ASCII))
                writeStrm.Write(__TxtRCsFile.Text);
            using (var readStrm = new StreamReader(_PathProyect)) {
                try {
                    Sintaxis test = new Sintaxis(readStrm);
                    test.AnalisisSintactico();

                    foreach (var item in test.OutPut) {
                        __TxtRConsola.Text += item;
                    }

                    foreach (var token in test.LogTokens) {
                        _TablaTokens.Rows.Add(token.ID, token.Valor);
                    }

                    foreach (var atrib in test.LogAtributos) {
                        _TablaAtributos.Rows.Add(atrib.Nombre, "" + atrib.Valor,
                            atrib.TipoDato, atrib.Acceso);
                    }
                } catch (InvalidDataException exc) {
                    __TxtRConsola.Text = "!!! " + exc.Message + "\n";
                } catch (NullReferenceException exc) {
                    __TxtRConsola.Text = "!!! " + exc.Message + "\n";
                } catch (Exception exc) {
                    __TxtRConsola.Text = "!!! " + exc.Message + "\n";
                }
            }
        }

        private void __BtnChangeCs_Click(object sender, EventArgs e)
        {
            __TxtRConsola.Text = "";
            try {
                using (var browser = new OpenFileDialog()) {
                    browser.Title = "Elija un archivo .cs";
                    browser.InitialDirectory = _PathProyect;
                    if (browser.ShowDialog() == DialogResult.OK) {
                        if (browser.FileName.Split('.')[1] == "cs") {
                            _PathProyect = browser.FileName;
                            using (var strmRead = new StreamReader(_PathProyect)) {
                                __TxtRCsFile.Text = strmRead.ReadToEnd();
                            }
                        }
                        else
                            throw new Exception("Archivo no admitido, debe tener extenson cs.");
                    }
                }
            } catch (Exception exc) {
                __TxtRConsola.Text = exc.Message;
            }
        }
    }
}
