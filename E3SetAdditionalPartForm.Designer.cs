using System.Windows.Forms;

namespace E3SetAdditionalPart
{
    partial class E3SetAdditionalPartForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

                #region Код, автоматически созданный конструктором форм Windows

                /// <summary>
                /// Требуемый метод для поддержки конструктора — не изменяйте 
                /// содержимое этого метода с помощью редактора кода.
                /// </summary>
                private void InitializeComponent()
        {
            this.buttonOk = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbNameDevice = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSynchr = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageAdditionalParts = new System.Windows.Forms.TabPage();
            this.additionalPartsUserControl = new E3SetAdditionalPart.PartsUserControl();
            this.tabPageReplacementParts = new System.Windows.Forms.TabPage();
            this.replacementPartsUserControl = new E3SetAdditionalPart.PartsUserControl();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageAdditionalParts.SuspendLayout();
            this.tabPageReplacementParts.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(954, 4);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(86, 40);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "Сохранить и выйти";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.buttonCancel);
            this.panel2.Controls.Add(this.buttonOk);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 506);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1141, 47);
            this.panel2.TabIndex = 1;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(1053, 12);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(77, 25);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Выйти";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbNameDevice);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.buttonSynchr);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1141, 54);
            this.panel1.TabIndex = 1;
            // 
            // tbNameDevice
            // 
            this.tbNameDevice.Location = new System.Drawing.Point(336, 17);
            this.tbNameDevice.Name = "tbNameDevice";
            this.tbNameDevice.ReadOnly = true;
            this.tbNameDevice.Size = new System.Drawing.Size(500, 20);
            this.tbNameDevice.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(226, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Изделие в проекте";
            // 
            // buttonSynchr
            // 
            this.buttonSynchr.Location = new System.Drawing.Point(53, 13);
            this.buttonSynchr.Name = "buttonSynchr";
            this.buttonSynchr.Size = new System.Drawing.Size(99, 30);
            this.buttonSynchr.TabIndex = 0;
            this.buttonSynchr.Text = "Синхронизация";
            this.buttonSynchr.UseVisualStyleBackColor = true;
            this.buttonSynchr.Click += new System.EventHandler(this.buttonSynchr_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageAdditionalParts);
            this.tabControl1.Controls.Add(this.tabPageReplacementParts);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 54);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1141, 452);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPageAdditionalParts
            // 
            this.tabPageAdditionalParts.Controls.Add(this.additionalPartsUserControl);
            this.tabPageAdditionalParts.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdditionalParts.Name = "tabPageAdditionalParts";
            this.tabPageAdditionalParts.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAdditionalParts.Size = new System.Drawing.Size(1133, 426);
            this.tabPageAdditionalParts.TabIndex = 0;
            this.tabPageAdditionalParts.Text = "Дополнительные части";
            this.tabPageAdditionalParts.UseVisualStyleBackColor = true;
            // 
            // additionalPartsUserControl
            // 
            this.additionalPartsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.additionalPartsUserControl.Location = new System.Drawing.Point(3, 3);
            this.additionalPartsUserControl.Name = "additionalPartsUserControl";
            this.additionalPartsUserControl.ShowAmountColumn = true;
            this.additionalPartsUserControl.ShowPrimColumn = true;
            this.additionalPartsUserControl.Size = new System.Drawing.Size(1127, 420);
            this.additionalPartsUserControl.TabIndex = 0;
            // 
            // tabPageReplacementParts
            // 
            this.tabPageReplacementParts.Controls.Add(this.replacementPartsUserControl);
            this.tabPageReplacementParts.Location = new System.Drawing.Point(4, 22);
            this.tabPageReplacementParts.Name = "tabPageReplacementParts";
            this.tabPageReplacementParts.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageReplacementParts.Size = new System.Drawing.Size(1133, 426);
            this.tabPageReplacementParts.TabIndex = 1;
            this.tabPageReplacementParts.Text = "Допустимые замены";
            this.tabPageReplacementParts.UseVisualStyleBackColor = true;
            // 
            // replacementPartsUserControl
            // 
            this.replacementPartsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.replacementPartsUserControl.Location = new System.Drawing.Point(3, 3);
            this.replacementPartsUserControl.Name = "replacementPartsUserControl";
            this.replacementPartsUserControl.ShowAmountColumn = true;
            this.replacementPartsUserControl.ShowPrimColumn = true;
            this.replacementPartsUserControl.Size = new System.Drawing.Size(1127, 420);
            this.replacementPartsUserControl.TabIndex = 0;
            // 
            // E3SetAdditionalPartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1141, 553);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "E3SetAdditionalPartForm";
            this.Text = "Перечень дополнительных Частей и Замен (от 11.11.2026)";
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPageAdditionalParts.ResumeLayout(false);
            this.tabPageReplacementParts.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button buttonCancel;
        private Panel panel1;
        private Label label1;
        private Button buttonSynchr;
        private TabControl tabControl1;
        private TabPage tabPageAdditionalParts;
        private PartsUserControl additionalPartsUserControl;
        private TabPage tabPageReplacementParts;
        private PartsUserControl replacementPartsUserControl;
        private TextBox tbNameDevice;
    }
}

