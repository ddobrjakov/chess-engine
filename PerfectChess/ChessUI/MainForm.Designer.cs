namespace PerfectChess
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.LogTextBox = new PerfectChess.MovesTextBox();
            this.UndoButton = new System.Windows.Forms.Button();
            this.NewGameButton = new System.Windows.Forms.Button();
            this.FlipButton = new System.Windows.Forms.Button();
            this.MaterialLabel1 = new System.Windows.Forms.Label();
            this.MaterialLabel2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LogTextBox
            // 
            this.LogTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LogTextBox.Location = new System.Drawing.Point(809, 150);
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.Size = new System.Drawing.Size(461, 461);
            this.LogTextBox.TabIndex = 0;
            this.LogTextBox.Text = "";
            // 
            // UndoButton
            // 
            this.UndoButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.UndoButton.Location = new System.Drawing.Point(809, 55);
            this.UndoButton.Name = "UndoButton";
            this.UndoButton.Size = new System.Drawing.Size(205, 55);
            this.UndoButton.TabIndex = 1;
            this.UndoButton.Text = "Undo Move";
            this.UndoButton.UseVisualStyleBackColor = true;
            this.UndoButton.Click += new System.EventHandler(this.UndoButton_Click);
            this.UndoButton.MouseEnter += new System.EventHandler(this.UndoButton_MouseEnter);
            this.UndoButton.MouseLeave += new System.EventHandler(this.UndoButton_MouseLeave);
            // 
            // NewGameButton
            // 
            this.NewGameButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.NewGameButton.Location = new System.Drawing.Point(1052, 55);
            this.NewGameButton.Name = "NewGameButton";
            this.NewGameButton.Size = new System.Drawing.Size(218, 55);
            this.NewGameButton.TabIndex = 2;
            this.NewGameButton.Text = "New Game";
            this.NewGameButton.UseVisualStyleBackColor = true;
            this.NewGameButton.Click += new System.EventHandler(this.NewGameButton_Click);
            this.NewGameButton.MouseEnter += new System.EventHandler(this.NewGameButton_MouseEnter);
            this.NewGameButton.MouseLeave += new System.EventHandler(this.NewGameButton_MouseLeave);
            // 
            // FlipButton
            // 
            this.FlipButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FlipButton.Location = new System.Drawing.Point(63, 674);
            this.FlipButton.Name = "FlipButton";
            this.FlipButton.Size = new System.Drawing.Size(143, 33);
            this.FlipButton.TabIndex = 3;
            this.FlipButton.Text = "Flip the board";
            this.FlipButton.UseVisualStyleBackColor = true;
            this.FlipButton.Click += new System.EventHandler(this.FlipButton_Click);
            this.FlipButton.MouseEnter += new System.EventHandler(this.FlipButton_MouseEnter);
            this.FlipButton.MouseLeave += new System.EventHandler(this.FlipButton_MouseLeave);
            // 
            // MaterialLabel1
            // 
            this.MaterialLabel1.AutoSize = true;
            this.MaterialLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.MaterialLabel1.ForeColor = System.Drawing.Color.White;
            this.MaterialLabel1.Location = new System.Drawing.Point(604, 674);
            this.MaterialLabel1.Name = "MaterialLabel1";
            this.MaterialLabel1.Size = new System.Drawing.Size(23, 25);
            this.MaterialLabel1.TabIndex = 4;
            this.MaterialLabel1.Text = "0";
            // 
            // MaterialLabel2
            // 
            this.MaterialLabel2.AutoSize = true;
            this.MaterialLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.MaterialLabel2.ForeColor = System.Drawing.Color.White;
            this.MaterialLabel2.Location = new System.Drawing.Point(604, 25);
            this.MaterialLabel2.Name = "MaterialLabel2";
            this.MaterialLabel2.Size = new System.Drawing.Size(23, 25);
            this.MaterialLabel2.TabIndex = 5;
            this.MaterialLabel2.Text = "0";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1560, 744);
            this.Controls.Add(this.MaterialLabel2);
            this.Controls.Add(this.MaterialLabel1);
            this.Controls.Add(this.FlipButton);
            this.Controls.Add(this.NewGameButton);
            this.Controls.Add(this.UndoButton);
            this.Controls.Add(this.LogTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Chess Engine";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button UndoButton;
        private System.Windows.Forms.Button NewGameButton;
        private System.Windows.Forms.Button FlipButton;
        private System.Windows.Forms.Label MaterialLabel1;
        private System.Windows.Forms.Label MaterialLabel2;
        private MovesTextBox LogTextBox;
    }
}

