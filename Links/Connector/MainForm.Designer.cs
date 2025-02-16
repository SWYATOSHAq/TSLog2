namespace TSLog.Links.Connector
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            StartButton = new Button();
            StopButton = new Button();
            SetingsButton = new Button();
            ExitButton = new Button();
            SuspendLayout();
            // 
            // StartButton
            // 
            StartButton.Location = new Point(12, 12);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(100, 24);
            StartButton.TabIndex = 0;
            StartButton.Text = "Запуск";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            StartButton.KeyDown += MainForm_KeyDown;
            // 
            // StopButton
            // 
            StopButton.Location = new Point(12, 42);
            StopButton.Name = "StopButton";
            StopButton.Size = new Size(100, 24);
            StopButton.TabIndex = 1;
            StopButton.Text = "Остановить";
            StopButton.UseVisualStyleBackColor = true;
            StopButton.Click += StopButton_Click;
            StopButton.KeyDown += MainForm_KeyDown;
            // 
            // SetingsButton
            // 
            SetingsButton.Location = new Point(118, 12);
            SetingsButton.Name = "SetingsButton";
            SetingsButton.Size = new Size(100, 24);
            SetingsButton.TabIndex = 2;
            SetingsButton.Text = "Настройки";
            SetingsButton.UseVisualStyleBackColor = true;
            SetingsButton.Click += SetingsButton_Click;
            SetingsButton.KeyDown += MainForm_KeyDown;
            // 
            // ExitButton
            // 
            ExitButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ExitButton.Location = new Point(647, 404);
            ExitButton.Name = "ExitButton";
            ExitButton.Size = new Size(100, 24);
            ExitButton.TabIndex = 3;
            ExitButton.Text = "Выход";
            ExitButton.UseVisualStyleBackColor = true;
            ExitButton.Click += ExitButton_Click;
            ExitButton.KeyDown += MainForm_KeyDown;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(759, 440);
            Controls.Add(ExitButton);
            Controls.Add(SetingsButton);
            Controls.Add(StopButton);
            Controls.Add(StartButton);
            Font = new Font("Verdana", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Коннектор";
            Shown += MainForm_Shown;
            KeyDown += MainForm_KeyDown;
            ResumeLayout(false);
        }

        #endregion

        private Button StartButton;
        private Button StopButton;
        private Button SetingsButton;
        private Button ExitButton;
    }
}
