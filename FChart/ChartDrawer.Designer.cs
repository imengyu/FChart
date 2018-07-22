namespace FChart
{
    partial class ChartDrawer
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.richTextBoxEditing = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(446, 468);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(302, 34);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // richTextBoxEditing
            // 
            this.richTextBoxEditing.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxEditing.Location = new System.Drawing.Point(12, 13);
            this.richTextBoxEditing.Name = "richTextBoxEditing";
            this.richTextBoxEditing.Size = new System.Drawing.Size(181, 26);
            this.richTextBoxEditing.TabIndex = 1;
            this.richTextBoxEditing.Text = "";
            this.richTextBoxEditing.Visible = false;
            // 
            // ChartDrawer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.Controls.Add(this.richTextBoxEditing);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.Name = "ChartDrawer";
            this.Size = new System.Drawing.Size(761, 515);
            this.Load += new System.EventHandler(this.ChartDrawer_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChartDrawer_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ChartDrawer_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ChartDrawer_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ChartDrawer_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ChartDrawer_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richTextBoxEditing;
    }
}
