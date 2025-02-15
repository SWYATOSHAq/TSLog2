namespace TSLog.Links.Connector
{
    partial class SettingsForm
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
            SaveButton = new Button();
            CancelButton = new Button();
            DbSettingsGroupBox = new GroupBox();
            DbPasswordTextBox = new TextBox();
            DbPasswordLabel = new Label();
            DbLoginTextBox = new TextBox();
            DbLoginLabel = new Label();
            DbDataSourceTextBox = new TextBox();
            DbDataSourceLabel = new Label();
            WmsSettingsGroupBox = new GroupBox();
            DiadocSettingsGroupBox = new GroupBox();
            CommonSettingsGroupBox = new GroupBox();
            IntervalTextBox = new TextBox();
            IntervalLabel = new Label();
            DbSettingsGroupBox.SuspendLayout();
            CommonSettingsGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // SaveButton
            // 
            SaveButton.DialogResult = DialogResult.OK;
            SaveButton.Location = new Point(406, 405);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(100, 24);
            SaveButton.TabIndex = 0;
            SaveButton.Text = "Сохранить";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            SaveButton.KeyDown += SettingsForm_KeyDown;
            // 
            // CancelButton
            // 
            CancelButton.DialogResult = DialogResult.Cancel;
            CancelButton.Location = new Point(512, 405);
            CancelButton.Name = "CancelButton";
            CancelButton.Size = new Size(100, 24);
            CancelButton.TabIndex = 1;
            CancelButton.Text = "Отмена";
            CancelButton.UseVisualStyleBackColor = true;
            CancelButton.Click += CancelButton_Click;
            CancelButton.KeyDown += SettingsForm_KeyDown;
            // 
            // DbSettingsGroupBox
            // 
            DbSettingsGroupBox.Controls.Add(DbPasswordTextBox);
            DbSettingsGroupBox.Controls.Add(DbPasswordLabel);
            DbSettingsGroupBox.Controls.Add(DbLoginTextBox);
            DbSettingsGroupBox.Controls.Add(DbLoginLabel);
            DbSettingsGroupBox.Controls.Add(DbDataSourceTextBox);
            DbSettingsGroupBox.Controls.Add(DbDataSourceLabel);
            DbSettingsGroupBox.Location = new Point(12, 12);
            DbSettingsGroupBox.Name = "DbSettingsGroupBox";
            DbSettingsGroupBox.Size = new Size(295, 113);
            DbSettingsGroupBox.TabIndex = 2;
            DbSettingsGroupBox.TabStop = false;
            DbSettingsGroupBox.Text = "Подключение к БД";
            // 
            // DbPasswordTextBox
            // 
            DbPasswordTextBox.Location = new Point(113, 77);
            DbPasswordTextBox.Name = "DbPasswordTextBox";
            DbPasswordTextBox.PasswordChar = '*';
            DbPasswordTextBox.Size = new Size(160, 22);
            DbPasswordTextBox.TabIndex = 5;
            DbPasswordTextBox.TextChanged += TextBox_TextChanged;
            DbPasswordTextBox.KeyDown += SettingsForm_KeyDown;
            // 
            // DbPasswordLabel
            // 
            DbPasswordLabel.AutoSize = true;
            DbPasswordLabel.Location = new Point(48, 80);
            DbPasswordLabel.Name = "DbPasswordLabel";
            DbPasswordLabel.Size = new Size(59, 14);
            DbPasswordLabel.TabIndex = 4;
            DbPasswordLabel.Text = "Пароль:";
            // 
            // DbLoginTextBox
            // 
            DbLoginTextBox.CharacterCasing = CharacterCasing.Lower;
            DbLoginTextBox.Location = new Point(113, 49);
            DbLoginTextBox.Name = "DbLoginTextBox";
            DbLoginTextBox.Size = new Size(160, 22);
            DbLoginTextBox.TabIndex = 3;
            DbLoginTextBox.TextChanged += TextBox_TextChanged;
            DbLoginTextBox.KeyDown += SettingsForm_KeyDown;
            // 
            // DbLoginLabel
            // 
            DbLoginLabel.AutoSize = true;
            DbLoginLabel.Location = new Point(6, 52);
            DbLoginLabel.Name = "DbLoginLabel";
            DbLoginLabel.Size = new Size(101, 14);
            DbLoginLabel.TabIndex = 2;
            DbLoginLabel.Text = "Пользователь:";
            // 
            // DbDataSourceTextBox
            // 
            DbDataSourceTextBox.CharacterCasing = CharacterCasing.Lower;
            DbDataSourceTextBox.Location = new Point(113, 21);
            DbDataSourceTextBox.Name = "DbDataSourceTextBox";
            DbDataSourceTextBox.Size = new Size(160, 22);
            DbDataSourceTextBox.TabIndex = 1;
            DbDataSourceTextBox.TextChanged += TextBox_TextChanged;
            DbDataSourceTextBox.KeyDown += SettingsForm_KeyDown;
            // 
            // DbDataSourceLabel
            // 
            DbDataSourceLabel.AutoSize = true;
            DbDataSourceLabel.Location = new Point(78, 24);
            DbDataSourceLabel.Name = "DbDataSourceLabel";
            DbDataSourceLabel.Size = new Size(29, 14);
            DbDataSourceLabel.TabIndex = 0;
            DbDataSourceLabel.Text = "БД:";
            // 
            // WmsSettingsGroupBox
            // 
            WmsSettingsGroupBox.Location = new Point(317, 12);
            WmsSettingsGroupBox.Name = "WmsSettingsGroupBox";
            WmsSettingsGroupBox.Size = new Size(295, 113);
            WmsSettingsGroupBox.TabIndex = 3;
            WmsSettingsGroupBox.TabStop = false;
            WmsSettingsGroupBox.Text = "Подключение к WMS";
            // 
            // DiadocSettingsGroupBox
            // 
            DiadocSettingsGroupBox.Location = new Point(12, 131);
            DiadocSettingsGroupBox.Name = "DiadocSettingsGroupBox";
            DiadocSettingsGroupBox.Size = new Size(295, 198);
            DiadocSettingsGroupBox.TabIndex = 4;
            DiadocSettingsGroupBox.TabStop = false;
            DiadocSettingsGroupBox.Text = "Настройки Диадок";
            // 
            // CommonSettingsGroupBox
            // 
            CommonSettingsGroupBox.Controls.Add(IntervalTextBox);
            CommonSettingsGroupBox.Controls.Add(IntervalLabel);
            CommonSettingsGroupBox.Location = new Point(317, 131);
            CommonSettingsGroupBox.Name = "CommonSettingsGroupBox";
            CommonSettingsGroupBox.Size = new Size(295, 198);
            CommonSettingsGroupBox.TabIndex = 5;
            CommonSettingsGroupBox.TabStop = false;
            CommonSettingsGroupBox.Text = "Общие настройки";
            // 
            // IntervalTextBox
            // 
            IntervalTextBox.Location = new Point(89, 21);
            IntervalTextBox.Name = "IntervalTextBox";
            IntervalTextBox.Size = new Size(100, 22);
            IntervalTextBox.TabIndex = 1;
            IntervalTextBox.TextChanged += TextBox_TextChanged;
            IntervalTextBox.KeyPress += IntervalTextBox_KeyPress;
            // 
            // IntervalLabel
            // 
            IntervalLabel.AutoSize = true;
            IntervalLabel.Location = new Point(9, 24);
            IntervalLabel.Name = "IntervalLabel";
            IntervalLabel.Size = new Size(74, 14);
            IntervalLabel.TabIndex = 0;
            IntervalLabel.Text = "Интервал:";
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(8F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(624, 441);
            Controls.Add(CommonSettingsGroupBox);
            Controls.Add(DiadocSettingsGroupBox);
            Controls.Add(WmsSettingsGroupBox);
            Controls.Add(DbSettingsGroupBox);
            Controls.Add(CancelButton);
            Controls.Add(SaveButton);
            Font = new Font("Verdana", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Настройки";
            Shown += SettingsForm_Shown;
            KeyDown += SettingsForm_KeyDown;
            DbSettingsGroupBox.ResumeLayout(false);
            DbSettingsGroupBox.PerformLayout();
            CommonSettingsGroupBox.ResumeLayout(false);
            CommonSettingsGroupBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button SaveButton;
        private Button CancelButton;
        private GroupBox DbSettingsGroupBox;
        private GroupBox WmsSettingsGroupBox;
        private GroupBox DiadocSettingsGroupBox;
        private Label DbDataSourceLabel;
        private TextBox DbDataSourceTextBox;
        private Label DbLoginLabel;
        private TextBox DbLoginTextBox;
        private Label DbPasswordLabel;
        private TextBox DbPasswordTextBox;
        private GroupBox CommonSettingsGroupBox;
        private TextBox IntervalTextBox;
        private Label IntervalLabel;
    }
}