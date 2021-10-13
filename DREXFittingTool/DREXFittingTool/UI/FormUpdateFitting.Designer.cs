
namespace DREXFittingTool.UI
{
    partial class FormUpdateFitting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.radFitting = new System.Windows.Forms.RadioButton();
            this.radFrameHeight = new System.Windows.Forms.RadioButton();
            this.lblRow = new System.Windows.Forms.Label();
            this.lblCol = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.cbxView = new System.Windows.Forms.ComboBox();
            this.lblView = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // radFitting
            // 
            this.radFitting.AutoSize = true;
            this.radFitting.Location = new System.Drawing.Point(13, 13);
            this.radFitting.Name = "radFitting";
            this.radFitting.Size = new System.Drawing.Size(97, 17);
            this.radFitting.TabIndex = 0;
            this.radFitting.TabStop = true;
            this.radFitting.Text = "継手符号昇順";
            this.radFitting.UseVisualStyleBackColor = true;
            // 
            // radFrameHeight
            // 
            this.radFrameHeight.AutoSize = true;
            this.radFrameHeight.Location = new System.Drawing.Point(12, 45);
            this.radFrameHeight.Name = "radFrameHeight";
            this.radFrameHeight.Size = new System.Drawing.Size(108, 17);
            this.radFrameHeight.TabIndex = 1;
            this.radFrameHeight.TabStop = true;
            this.radFrameHeight.Text = "梁せいのサイズ順";
            this.radFrameHeight.UseVisualStyleBackColor = true;
            // 
            // lblRow
            // 
            this.lblRow.AutoSize = true;
            this.lblRow.Location = new System.Drawing.Point(12, 111);
            this.lblRow.Name = "lblRow";
            this.lblRow.Size = new System.Drawing.Size(19, 13);
            this.lblRow.TabIndex = 2;
            this.lblRow.Text = "幅";
            // 
            // lblCol
            // 
            this.lblCol.AutoSize = true;
            this.lblCol.Location = new System.Drawing.Point(12, 135);
            this.lblCol.Name = "lblCol";
            this.lblCol.Size = new System.Drawing.Size(27, 13);
            this.lblCol.TabIndex = 3;
            this.lblCol.Text = "高さ";
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(107, 108);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(169, 20);
            this.txtWidth.TabIndex = 4;
            this.txtWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtRow_KeyPress);
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(107, 132);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(169, 20);
            this.txtHeight.TabIndex = 5;
            this.txtHeight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCol_KeyPress);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(197, 169);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 25);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "キャンセル";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(111, 169);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 25);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // cbxView
            // 
            this.cbxView.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxView.FormattingEnabled = true;
            this.cbxView.Location = new System.Drawing.Point(107, 81);
            this.cbxView.Name = "cbxView";
            this.cbxView.Size = new System.Drawing.Size(169, 21);
            this.cbxView.TabIndex = 8;
            // 
            // lblView
            // 
            this.lblView.AutoSize = true;
            this.lblView.Location = new System.Drawing.Point(12, 84);
            this.lblView.Name = "lblView";
            this.lblView.Size = new System.Drawing.Size(91, 13);
            this.lblView.TabIndex = 9;
            this.lblView.Text = "製図ビューの更新";
            // 
            // FormUpdateFitting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(289, 207);
            this.Controls.Add(this.lblView);
            this.Controls.Add(this.cbxView);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.txtWidth);
            this.Controls.Add(this.lblCol);
            this.Controls.Add(this.lblRow);
            this.Controls.Add(this.radFrameHeight);
            this.Controls.Add(this.radFitting);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormUpdateFitting";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DREXFittingTool";
            this.Load += new System.EventHandler(this.FormFitting_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radFitting;
        private System.Windows.Forms.RadioButton radFrameHeight;
        private System.Windows.Forms.Label lblRow;
        private System.Windows.Forms.Label lblCol;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ComboBox cbxView;
        private System.Windows.Forms.Label lblView;
    }
}