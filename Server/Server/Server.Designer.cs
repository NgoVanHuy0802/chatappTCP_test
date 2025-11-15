using System.Windows.Forms;

namespace Server
{
    partial class Server
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle27 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle28 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
            this.startButton = new System.Windows.Forms.Button();
            this.portLabel = new System.Windows.Forms.Label();
            this.localaddrLabel = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.sendTextBox = new System.Windows.Forms.TextBox();
            this.sendLabel = new System.Windows.Forms.Label();
            this.clientsDataGridView = new System.Windows.Forms.DataGridView();
            this.identifier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dc = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Message = new System.Windows.Forms.DataGridViewButtonColumn();
            this.totalLabel = new System.Windows.Forms.Label();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.keyTextBox = new System.Windows.Forms.TextBox();
            this.keyLabel = new System.Windows.Forms.Label();
            this.addrTextBox = new System.Windows.Forms.TextBox();
            this.checkBox = new System.Windows.Forms.CheckBox();
            this.contentLabel = new System.Windows.Forms.Label();
            this.chatPanel = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.clientsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(13, 13);
            this.startButton.Margin = new System.Windows.Forms.Padding(4);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(116, 28);
            this.startButton.TabIndex = 23;
            this.startButton.TabStop = false;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.BackColor = System.Drawing.Color.Transparent;
            this.portLabel.Location = new System.Drawing.Point(410, 21);
            this.portLabel.Margin = new System.Windows.Forms.Padding(8, 4, 4, 4);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(29, 13);
            this.portLabel.TabIndex = 22;
            this.portLabel.Text = "Port:";
            // 
            // localaddrLabel
            // 
            this.localaddrLabel.AutoSize = true;
            this.localaddrLabel.BackColor = System.Drawing.Color.Transparent;
            this.localaddrLabel.Location = new System.Drawing.Point(210, 21);
            this.localaddrLabel.Margin = new System.Windows.Forms.Padding(8, 4, 4, 4);
            this.localaddrLabel.Name = "localaddrLabel";
            this.localaddrLabel.Size = new System.Drawing.Size(48, 13);
            this.localaddrLabel.TabIndex = 21;
            this.localaddrLabel.Text = "Address:";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(447, 18);
            this.portTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.portTextBox.MaxLength = 10;
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(132, 20);
            this.portTextBox.TabIndex = 20;
            this.portTextBox.TabStop = false;
            this.portTextBox.Text = "9000";
            this.portTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(13, 123);
            this.clearButton.Margin = new System.Windows.Forms.Padding(4);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(116, 28);
            this.clearButton.TabIndex = 25;
            this.clearButton.TabStop = false;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // disconnectButton
            // 
            this.disconnectButton.Location = new System.Drawing.Point(463, 123);
            this.disconnectButton.Margin = new System.Windows.Forms.Padding(4);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(116, 28);
            this.disconnectButton.TabIndex = 26;
            this.disconnectButton.TabStop = false;
            this.disconnectButton.Text = "Disconnect all";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.DisconnectButton_Click);
            // 
            // sendTextBox
            // 
            this.sendTextBox.Location = new System.Drawing.Point(13, 580);
            this.sendTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.sendTextBox.Name = "sendTextBox";
            this.sendTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.sendTextBox.Size = new System.Drawing.Size(566, 20);
            this.sendTextBox.TabIndex = 27;
            this.sendTextBox.TabStop = false;
            this.sendTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SendTextBox_KeyDown);
            //
            // sendLabel
            //
            this.sendLabel.AutoSize = true;
            this.sendLabel.BackColor = System.Drawing.Color.Transparent;
            this.sendLabel.Location = new System.Drawing.Point(10, 559);
            this.sendLabel.Margin = new System.Windows.Forms.Padding(8, 4, 4, 4);
            this.sendLabel.Name = "sendLabel";
            this.sendLabel.Size = new System.Drawing.Size(32, 13);
            this.sendLabel.TabIndex = 28;
            this.sendLabel.Text = "Send";
            // 
            // 
            // clientsDataGridView
            // 
            this.clientsDataGridView.AllowUserToAddRows = false;
            this.clientsDataGridView.AllowUserToDeleteRows = false;
            this.clientsDataGridView.AllowUserToResizeColumns = false;
            this.clientsDataGridView.AllowUserToResizeRows = false;
            this.clientsDataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
            this.clientsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.clientsDataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.clientsDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle22.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle22.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle22.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle22.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle22.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle22.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.clientsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle22;
            this.clientsDataGridView.ColumnHeadersHeight = 24;
            this.clientsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.clientsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.identifier,
            this.name,
            this.dc,
            this.Message});
            this.clientsDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.clientsDataGridView.EnableHeadersVisualStyles = false;
            this.clientsDataGridView.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.clientsDataGridView.Location = new System.Drawing.Point(587, 13);
            this.clientsDataGridView.Margin = new System.Windows.Forms.Padding(4);
            this.clientsDataGridView.MultiSelect = false;
            this.clientsDataGridView.Name = "clientsDataGridView";
            this.clientsDataGridView.ReadOnly = true;
            this.clientsDataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle27.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle27.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle27.ForeColor = System.Drawing.SystemColors.WindowText;
            this.clientsDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle27;
            this.clientsDataGridView.RowHeadersVisible = false;
            this.clientsDataGridView.RowHeadersWidth = 40;
            this.clientsDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle28.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle28.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            dataGridViewCellStyle28.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle28.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle28.SelectionForeColor = System.Drawing.Color.Black;
            this.clientsDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle28;
            this.clientsDataGridView.RowTemplate.Height = 24;
            this.clientsDataGridView.RowTemplate.ReadOnly = true;
            this.clientsDataGridView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.clientsDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.clientsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.clientsDataGridView.ShowCellErrors = false;
            this.clientsDataGridView.ShowCellToolTips = false;
            this.clientsDataGridView.ShowEditingIcon = false;
            this.clientsDataGridView.ShowRowErrors = false;
            this.clientsDataGridView.Size = new System.Drawing.Size(341, 587);
            this.clientsDataGridView.TabIndex = 30;
            this.clientsDataGridView.TabStop = false;
            this.clientsDataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ClientsDataGridView_CellClick);
            this.clientsDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.clientsDataGridView_CellContentClick);
            // 
            // identifier
            // 
            this.identifier.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle23.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.identifier.DefaultCellStyle = dataGridViewCellStyle23;
            this.identifier.HeaderText = "ID";
            this.identifier.MaxInputLength = 20;
            this.identifier.MinimumWidth = 20;
            this.identifier.Name = "identifier";
            this.identifier.ReadOnly = true;
            this.identifier.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.identifier.Width = 70;
            // 
            // name
            // 
            this.name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle24.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.name.DefaultCellStyle = dataGridViewCellStyle24;
            this.name.HeaderText = "Name";
            this.name.MaxInputLength = 20;
            this.name.MinimumWidth = 20;
            this.name.Name = "name";
            this.name.ReadOnly = true;
            this.name.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dc
            // 
            this.dc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle25.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle25.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dc.DefaultCellStyle = dataGridViewCellStyle25;
            this.dc.HeaderText = "Disconnect";
            this.dc.MinimumWidth = 20;
            this.dc.Name = "dc";
            this.dc.ReadOnly = true;
            this.dc.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dc.Text = "Kick";
            this.dc.UseColumnTextForButtonValue = true;
            this.dc.Width = 80;
            // 
            // Message
            // 
            this.Message.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle26.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle26.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.Message.DefaultCellStyle = dataGridViewCellStyle26;
            this.Message.HeaderText = "Gửi tin nhắn";
            this.Message.MinimumWidth = 20;
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            this.Message.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Message.Text = "Gửi";
            this.Message.UseColumnTextForButtonValue = true;
            this.Message.Width = 80;
            // 
            // totalLabel
            // 
            this.totalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.totalLabel.BackColor = System.Drawing.Color.Transparent;
            this.totalLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.totalLabel.Location = new System.Drawing.Point(489, 102);
            this.totalLabel.Margin = new System.Windows.Forms.Padding(8, 4, 4, 4);
            this.totalLabel.Name = "totalLabel";
            this.totalLabel.Size = new System.Drawing.Size(90, 13);
            this.totalLabel.TabIndex = 31;
            this.totalLabel.Text = "Total clients: 0";
            this.totalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.BackColor = System.Drawing.Color.Transparent;
            this.usernameLabel.Location = new System.Drawing.Point(206, 49);
            this.usernameLabel.Margin = new System.Windows.Forms.Padding(8, 4, 4, 4);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(52, 13);
            this.usernameLabel.TabIndex = 33;
            this.usernameLabel.Text = "Userame:";
            // 
            // usernameTextBox
            // 
            this.usernameTextBox.Location = new System.Drawing.Point(266, 46);
            this.usernameTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.usernameTextBox.MaxLength = 50;
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(132, 20);
            this.usernameTextBox.TabIndex = 34;
            this.usernameTextBox.TabStop = false;
            this.usernameTextBox.Text = "Server";
            this.usernameTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // keyTextBox
            // 
            this.keyTextBox.Location = new System.Drawing.Point(447, 46);
            this.keyTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.keyTextBox.MaxLength = 200;
            this.keyTextBox.Name = "keyTextBox";
            this.keyTextBox.Size = new System.Drawing.Size(132, 20);
            this.keyTextBox.TabIndex = 36;
            this.keyTextBox.TabStop = false;
            this.keyTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // keyLabel
            // 
            this.keyLabel.AutoSize = true;
            this.keyLabel.BackColor = System.Drawing.Color.Transparent;
            this.keyLabel.Location = new System.Drawing.Point(411, 49);
            this.keyLabel.Margin = new System.Windows.Forms.Padding(8, 4, 4, 4);
            this.keyLabel.Name = "keyLabel";
            this.keyLabel.Size = new System.Drawing.Size(28, 13);
            this.keyLabel.TabIndex = 35;
            this.keyLabel.Text = "Key:";
            // 
            // addrTextBox
            // 
            this.addrTextBox.Location = new System.Drawing.Point(266, 18);
            this.addrTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.addrTextBox.MaxLength = 200;
            this.addrTextBox.Name = "addrTextBox";
            this.addrTextBox.Size = new System.Drawing.Size(132, 20);
            this.addrTextBox.TabIndex = 37;
            this.addrTextBox.TabStop = false;
            this.addrTextBox.Text = "127.0.0.1";
            this.addrTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // checkBox
            // 
            this.checkBox.BackColor = System.Drawing.Color.Transparent;
            this.checkBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.checkBox.Location = new System.Drawing.Point(447, 74);
            this.checkBox.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox.Name = "checkBox";
            this.checkBox.Size = new System.Drawing.Size(72, 20);
            this.checkBox.TabIndex = 42;
            this.checkBox.Text = "Hide key";
            this.checkBox.UseVisualStyleBackColor = false;
            this.checkBox.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
            // 
            // contentLabel
            //
            this.contentLabel.AutoSize = true;
            this.contentLabel.BackColor = System.Drawing.Color.Transparent;
            this.contentLabel.Location = new System.Drawing.Point(10, 138);
            this.contentLabel.Margin = new System.Windows.Forms.Padding(8, 4, 4, 4);
            this.contentLabel.Name = "contentLabel";
            this.contentLabel.Size = new System.Drawing.Size(119, 13);
            this.contentLabel.TabIndex = 43;
            this.contentLabel.Text = "Hoạt động & đính kèm";
            //
            // chatPanel
            //
            this.chatPanel.AutoScroll = true;
            this.chatPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.chatPanel.Location = new System.Drawing.Point(13, 159);
            this.chatPanel.Margin = new System.Windows.Forms.Padding(4);
            this.chatPanel.Name = "chatPanel";
            this.chatPanel.Size = new System.Drawing.Size(566, 412);
            this.chatPanel.TabIndex = 44;
            // 
            // Server
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(938, 620);
            this.Controls.Add(this.checkBox);
            this.Controls.Add(this.chatPanel);
            this.Controls.Add(this.contentLabel);
            this.Controls.Add(this.addrTextBox);
            this.Controls.Add(this.keyTextBox);
            this.Controls.Add(this.keyLabel);
            this.Controls.Add(this.usernameTextBox);
            this.Controls.Add(this.usernameLabel);
            this.Controls.Add(this.totalLabel);
            this.Controls.Add(this.clientsDataGridView);
            this.Controls.Add(this.sendLabel);
            this.Controls.Add(this.sendTextBox);
            this.Controls.Add(this.disconnectButton);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.localaddrLabel);
            this.Controls.Add(this.portTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "Server";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Server_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.clientsDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.Label localaddrLabel;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.TextBox sendTextBox;
        private System.Windows.Forms.Label sendLabel;
        private System.Windows.Forms.DataGridView clientsDataGridView;
        private System.Windows.Forms.Label totalLabel;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.TextBox keyTextBox;
        private System.Windows.Forms.Label keyLabel;
        private System.Windows.Forms.TextBox addrTextBox;
        private System.Windows.Forms.CheckBox checkBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn identifier;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewButtonColumn dc;
        private System.Windows.Forms.DataGridViewButtonColumn Message;
        private System.Windows.Forms.Label contentLabel;
        private System.Windows.Forms.FlowLayoutPanel chatPanel;
    }
}

