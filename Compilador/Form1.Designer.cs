namespace Compilador {
    partial class __FrmMain {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.@__PnlMain = new System.Windows.Forms.Panel();
            this.@__BtnCompilar = new System.Windows.Forms.Button();
            this.@__RTxtCsFile = new System.Windows.Forms.RichTextBox();
            this.@__PnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // __PnlMain
            // 
            this.@__PnlMain.Controls.Add(this.@__BtnCompilar);
            this.@__PnlMain.Controls.Add(this.@__RTxtCsFile);
            this.@__PnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.@__PnlMain.Location = new System.Drawing.Point(0, 0);
            this.@__PnlMain.Margin = new System.Windows.Forms.Padding(0);
            this.@__PnlMain.Name = "__PnlMain";
            this.@__PnlMain.Size = new System.Drawing.Size(556, 261);
            this.@__PnlMain.TabIndex = 0;
            // 
            // __BtnCompilar
            // 
            this.@__BtnCompilar.Location = new System.Drawing.Point(221, 187);
            this.@__BtnCompilar.Name = "__BtnCompilar";
            this.@__BtnCompilar.Size = new System.Drawing.Size(83, 25);
            this.@__BtnCompilar.TabIndex = 1;
            this.@__BtnCompilar.Text = "Compilar";
            this.@__BtnCompilar.UseVisualStyleBackColor = true;
            this.@__BtnCompilar.Click += new System.EventHandler(this.@__BtnCompilar_Click);
            // 
            // __RTxtCsFile
            // 
            this.@__RTxtCsFile.Location = new System.Drawing.Point(12, 12);
            this.@__RTxtCsFile.Name = "__RTxtCsFile";
            this.@__RTxtCsFile.Size = new System.Drawing.Size(532, 169);
            this.@__RTxtCsFile.TabIndex = 0;
            this.@__RTxtCsFile.Text = "";
            // 
            // __FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(556, 261);
            this.Controls.Add(this.@__PnlMain);
            this.Name = "__FrmMain";
            this.Text = "Compilador - Angel Emmanuel Ruiz Alcaraz";
            this.@__PnlMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel __PnlMain;
        private System.Windows.Forms.RichTextBox __RTxtCsFile;
        private System.Windows.Forms.Button __BtnCompilar;
    }
}

