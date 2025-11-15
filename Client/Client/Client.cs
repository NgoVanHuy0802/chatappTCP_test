using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Client
{
    public partial class Client : Form
    {
        private bool connected = false; //trạng thái kết nối
        private Thread client = null;
        private struct MyClient
        {
            public string username;
            public string key;
            public TcpClient client;
            public NetworkStream stream;
            public byte[] buffer;
            public StringBuilder data;
            public EventWaitHandle handle;
        };
        private MyClient obj;
        private Task send = null;
        private bool exit = false;
        private const int AuthorizationTimeoutMs = 5000;
        private const int AttachmentLimitBytes = 5 * 1024 * 1024; // 5MB

        public Client()
        {
            InitializeComponent();
            ClockTimer_Tick(this, EventArgs.Empty);
        }

        //ghi log lên chatPanel

        private void Log(string msg = "")
        {
            if (!exit)
            {
                chatPanel.Invoke((MethodInvoker)delegate
                {
                    if (msg.Length > 0)
                    {
                        Label lbl = new Label();
                        lbl.Text = $"[{DateTime.Now:HH:mm:ss}] {msg}";
                        lbl.AutoSize = true;
                        chatPanel.Controls.Add(lbl);
                    }
                    else
                    {
                        chatPanel.Controls.Clear();
                    }
                });
            }
        }


        //Trả về tin nhắn lỗi
        private string ErrorMsg(string msg) => $"ERROR: {msg}";
        //Trả về tin nhắn hệ thống
        private string SystemMsg(string msg) => $"SYSTEM: {msg}";
        //Cập nhật trạng thái kết nối và ui
        private void Connected(bool status)
        {
            if (!exit)
            {
                connectButton.Invoke((MethodInvoker)delegate
                {
                    connected = status;
                    addrTextBox.Enabled = !status;
                    portTextBox.Enabled = !status;
                    usernameTextBox.Enabled = !status;
                    keyTextBox.Enabled = !status;
                    connectButton.Text = status ? "Disconnect" : "Connect";
                    Log(SystemMsg(status ? "You are now connected" : "You are now disconnected"));
                });
            }
        }

        // Hiển thị tin nhắn lên chatPanel
        private void DisplayMessage(string msg)
        {
            chatPanel.Invoke((MethodInvoker)delegate
            {
                // === 1) IMAGE ===
                if (msg.StartsWith("[IMAGE]"))
                {
                    if (!TryParseAttachment(msg, out string user, out string fileName, out byte[] data, "hình ảnh"))
                    {
                        return;
                    }

                    AddSenderLabel(user);

                    Image previewImage;
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        try
                        {
                            previewImage = (Image)Image.FromStream(ms).Clone();
                        }
                        catch (Exception ex)
                        {
                            Log(ErrorMsg($"Không thể đọc hình ảnh: {ex.Message}"));
                            return;
                        }
                    }

                    PictureBox pic = new PictureBox
                    {
                        Image = previewImage,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Width = 250,
                        Height = 180,
                        Cursor = Cursors.Hand
                    };

                    pic.Click += (s, e) => ShowImageViewer(fileName, previewImage);
                    pic.Disposed += (s, e) => pic.Image?.Dispose();

                    ContextMenuStrip menu = new ContextMenuStrip();
                    menu.Items.Add("Xem ảnh", null, (s, e) => ShowImageViewer(fileName, previewImage));
                    menu.Items.Add("Lưu ảnh...", null, (s, e) => SaveAttachment(data, fileName, "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*", askOpen: false));
                    pic.ContextMenuStrip = menu;

                    chatPanel.Controls.Add(pic);
                    return;
                }

                // === 2) FILE ===
                if (msg.StartsWith("[FILE]"))
                {
                    if (!TryParseAttachment(msg, out string user, out string fileName, out byte[] data, "tệp"))
                    {
                        return;
                    }

                    AddSenderLabel(user);

                    FlowLayoutPanel fileContainer = new FlowLayoutPanel
                    {
                        AutoSize = true,
                        FlowDirection = FlowDirection.LeftToRight,
                        Margin = new Padding(0, 0, 0, 5)
                    };

                    Label fileLabel = new Label
                    {
                        Text = "📁 " + fileName,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9.75f, FontStyle.Underline),
                        ForeColor = Color.Blue,
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0, 6, 6, 0)
                    };
                    fileLabel.Click += (s, e) => SaveAttachment(data, fileName, "All files|*.*", askOpen: true);

                    Button downloadButton = new Button
                    {
                        Text = "Tải xuống",
                        AutoSize = true,
                        Margin = new Padding(0, 0, 6, 0)
                    };
                    downloadButton.Click += (s, e) => SaveAttachment(data, fileName, "All files|*.*", askOpen: true);

                    Label sizeLabel = new Label
                    {
                        AutoSize = true,
                        Text = $"({FormatFileSize(data.Length)})",
                        Margin = new Padding(0, 6, 0, 0)
                    };

                    fileContainer.Controls.Add(fileLabel);
                    fileContainer.Controls.Add(downloadButton);
                    fileContainer.Controls.Add(sizeLabel);
                    chatPanel.Controls.Add(fileContainer);
                    return;
                }

                // === 3) TEXT MESSAGE ===
                Label lbl = new Label
                {
                    Text = $"[{DateTime.Now:HH:mm:ss}] {msg}",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.75f, FontStyle.Regular)
                };
                chatPanel.Controls.Add(lbl);
            });
        }

        private bool TryDecodeBase64(string base64, string description, out byte[] data)
        {
            data = Array.Empty<byte>();
            try
            {
                data = Convert.FromBase64String(base64);
                return true;
            }
            catch (FormatException ex)
            {
                Log(ErrorMsg($"Dữ liệu {description} không hợp lệ: {ex.Message}"));
            }
            catch (Exception ex)
            {
                Log(ErrorMsg($"Không thể đọc dữ liệu {description}: {ex.Message}"));
            }
            return false;
        }

        private bool TryParseAttachment(string msg, out string user, out string fileName, out byte[] data, string description)
        {
            user = string.Empty;
            fileName = string.Empty;
            data = Array.Empty<byte>();
            string[] parts = msg.Split('|');
            if (parts.Length < 4)
            {
                Log(ErrorMsg($"Dữ liệu {description} không đúng định dạng."));
                return false;
            }
            user = parts[1];
            fileName = parts[2];
            string base64 = string.Join("|", parts, 3, parts.Length - 3);
            return TryDecodeBase64(base64, description, out data);
        }

        private void AddSenderLabel(string user)
        {
            Label userLabel = new Label
            {
                Text = $"[{DateTime.Now:HH:mm:ss}] {user}:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.75f, FontStyle.Regular)
            };
            chatPanel.Controls.Add(userLabel);
        }

        private void SaveAttachment(byte[] data, string fileName, string filter, bool askOpen)
        {
            try
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog
                {
                    FileName = fileName,
                    Filter = filter
                })
                {
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllBytes(saveDialog.FileName, data);
                        if (askOpen)
                        {
                            DialogResult open = MessageBox.Show("Tệp đã lưu. Mở ngay bây giờ?", "Mở tệp", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (open == DialogResult.Yes)
                            {
                                ProcessStartInfo psi = new ProcessStartInfo(saveDialog.FileName)
                                {
                                    UseShellExecute = true
                                };
                                Process.Start(psi);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu tệp: {ex.Message}");
            }
        }

        private void ShowImageViewer(string fileName, Image preview)
        {
            Form viewer = new Form
            {
                Text = fileName,
                Size = new Size(600, 600),
                StartPosition = FormStartPosition.CenterParent
            };

            PictureBox big = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = (Image)preview.Clone()
            };

            viewer.FormClosed += (s, e) => big.Image?.Dispose();
            viewer.Controls.Add(big);
            viewer.ShowDialog();
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            double size = bytes;
            int order = 0;
            while (size >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {suffixes[order]}";
        }




        private void Read(IAsyncResult result)
        {
            int bytes = 0;
            if (obj.client.Connected)
            {
                try { bytes = obj.stream.EndRead(result); }
                catch (Exception ex) { Log(ErrorMsg(ex.Message)); }
            }
            if (bytes > 0)
            {
                obj.data.Append(Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                try
                {
                    if (obj.stream.DataAvailable)
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, Read, null);
                    else
                    {
                        string message = obj.data.ToString();
                        DisplayMessage(message);
                        obj.data.Clear();
                        obj.handle.Set();
                    }
                }
                catch (Exception ex)
                {
                    obj.data.Clear();
                    Log(ErrorMsg(ex.Message));
                    obj.handle.Set();
                }
            }
            else
            {
                obj.client.Close();
                obj.handle.Set();
            }
        }

        private void ReadAuth(IAsyncResult result)
        {
            int bytes = 0;
            if (obj.client.Connected)
            {
                try { bytes = obj.stream.EndRead(result); }
                catch (Exception ex) { Log(ErrorMsg(ex.Message)); }
            }
            if (bytes > 0)
            {
                obj.data.Append(Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                try
                {
                    if (obj.stream.DataAvailable)
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, ReadAuth, null);
                    else
                    {
                        JavaScriptSerializer json = new JavaScriptSerializer();
                        Dictionary<string, string> data = json.Deserialize<Dictionary<string, string>>(obj.data.ToString());
                        if (data.ContainsKey("status") && data["status"].Equals("authorized"))
                            Connected(true);
                        obj.data.Clear();
                        obj.handle.Set();
                    }
                }
                catch (Exception ex)
                {
                    obj.data.Clear();
                    Log(ErrorMsg(ex.Message));
                    obj.handle.Set();
                }
            }
            else
            {
                obj.client.Close();
                obj.handle.Set();
            }
        }

        private bool Authorize()
        {
            bool success = false;
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "username", obj.username },
                { "key", obj.key }
            };
            JavaScriptSerializer json = new JavaScriptSerializer();
            Send(json.Serialize(data));
            while (obj.client.Connected)
            {
                try
                {
                    obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, ReadAuth, null);
                    bool signaled = obj.handle.WaitOne(AuthorizationTimeoutMs);
                    if (!signaled)
                    {
                        Log(SystemMsg("Authorization timed out"));
                        obj.client.Close();
                        break;
                    }
                    if (connected)
                    {
                        success = true;
                        break;
                    }
                }
                catch (Exception ex) { Log(ErrorMsg(ex.Message)); }
            }
            if (!connected) Log(SystemMsg("Unauthorized"));
            return success;
        }

        private void Connection(IPAddress ip, int port, string username, string key)
        {
            try
            {
                obj = new MyClient
                {
                    username = username,
                    key = key,
                    client = new TcpClient()
                };
                obj.client.Connect(ip, port);
                obj.stream = obj.client.GetStream();
                obj.buffer = new byte[obj.client.ReceiveBufferSize];
                obj.data = new StringBuilder();
                obj.handle = new EventWaitHandle(false, EventResetMode.AutoReset);
                if (Authorize())
                {
                    while (obj.client.Connected)
                    {
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, Read, null);
                        obj.handle.WaitOne();
                    }
                    obj.client.Close();
                    Connected(false);
                }
            }
            catch (Exception ex) { Log(ErrorMsg(ex.Message)); }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                obj.client.Close();
            }
            else if (client == null || !client.IsAlive)
            {
                bool error = false;
                string address = addrTextBox.Text.Trim();
                string number = portTextBox.Text.Trim();
                string username = usernameTextBox.Text.Trim();
                IPAddress ip = null;
                if (!TryResolveIPv4(address, out ip))
                {
                    error = true;
                    Log(SystemMsg("Invalid address (must resolve to IPv4)"));
                }

                if (!int.TryParse(number, out int port)) { error = true; Log(SystemMsg("Invalid port")); }
                if (username.Length < 1) { error = true; Log(SystemMsg("Username required")); }

                if (!error)
                {
                    client = new Thread(() => Connection(ip, port, username, keyTextBox.Text)) { IsBackground = true };
                    client.Start();
                }
            }
        }

        private void Write(IAsyncResult result)
        {
            if (obj.client.Connected)
            {
                try { obj.stream.EndWrite(result); }
                catch (Exception ex) { Log(ErrorMsg(ex.Message)); }
            }
        }

        private void BeginWrite(string msg)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (obj.client.Connected)
            {
                try { obj.stream.BeginWrite(buffer, 0, buffer.Length, Write, null); }
                catch (Exception ex) { Log(ErrorMsg(ex.Message)); }
            }
        }

        private void Send(string msg)
        {
            if (send == null || send.IsCompleted)
                send = Task.Factory.StartNew(() => BeginWrite(msg));
            else
                send.ContinueWith(_ => BeginWrite(msg));
        }

        
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!connected) return;
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Image", null, (s, ev) => SendImage());
            menu.Items.Add("File", null, (s, ev) => SendFile());
            menu.Items.Add("Emoji", null, (s, ev) => SendEmoji());
            menu.Show(Cursor.Position);
        }

        //Gửi Image
        private void SendImage()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string filePath = dlg.FileName;
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > AttachmentLimitBytes)
                {
                    MessageBox.Show($"Image exceeds {AttachmentLimitBytes / (1024 * 1024)} MB limit.");
                    return;
                }
                string fileName = Path.GetFileName(filePath);
                byte[] data = File.ReadAllBytes(filePath);
                string base64 = Convert.ToBase64String(data);

                string msg = $"[IMAGE]|{obj.username}|{fileName}|{base64}";

                // Hiện ảnh cho chính mình
                DisplayMessage(msg);

                // Gửi lên server
                Send(msg);
            }
        }


        //Gửi file
        private void SendFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string filePath = dlg.FileName;
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > AttachmentLimitBytes)
                {
                    MessageBox.Show($"File exceeds {AttachmentLimitBytes / (1024 * 1024)} MB limit.");
                    return;
                }
                string fileName = Path.GetFileName(filePath);
                byte[] data = File.ReadAllBytes(filePath);
                string base64 = Convert.ToBase64String(data);

                string msg = $"[FILE]|{obj.username}|{fileName}|{base64}";

                // Hiện file cho chính mình
                DisplayMessage(msg);

                // Gửi lên server
                Send(msg);
            }
        }


        //Chọn Emoji
        private void SendEmoji()
        {
            string[] emojis = { "😀", "😂", "😍", "😎", "😭", "😡", "👍", "❤️" };
            Form picker = new Form
            {
                Text = "Select Emoji",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(400, 200)
            };
            FlowLayoutPanel panel = new FlowLayoutPanel { Dock = DockStyle.Fill };
            foreach (string emoji in emojis)
            {
                Button btn = new Button
                {
                    Text = emoji,
                    Font = new Font("Segoe UI Emoji", 16),
                    Width = 40,
                    Height = 40
                };
                btn.Click += (s, e) =>
                {
                    DisplayMessage($"{obj.username}: {emoji}");
                    Send($"{obj.username}: {emoji}");
                    picker.Close();
                };
                panel.Controls.Add(btn);
            }
            picker.Controls.Add(panel);
            picker.ShowDialog();
        }

        //Gửi text khi ấn Enter
        private void SendTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                string msg = sendTextBox.Text.Trim();
                if (msg.Length > 0)
                {
                    DisplayMessage($"{obj.username} (You): {msg}");
                    Send($"{obj.username}: {msg}");
                    sendTextBox.Clear();
                }
            }
        }

        private void ConnectionFields_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ConnectButton_Click(sender, EventArgs.Empty);
            }
        }

        //Xóa log
        private void ClearButton_Click(object sender, EventArgs e)
            {
            Log();                      // Xóa log
            chatPanel.Controls.Clear(); // Xóa toàn bộ nội dung khung chat
            }

        //Hiện thị Key
        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            keyTextBox.PasswordChar = checkBox.Checked ? '*' : '\0';
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            clockLabel.Text = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
        }

        //Đóng form
        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            exit = true;
            if (connected) obj.client.Close();
        }

        private void chatPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private bool TryResolveIPv4(string host, out IPAddress address)
        {
            address = null;
            if (IPAddress.TryParse(host, out IPAddress literal) && literal.AddressFamily == AddressFamily.InterNetwork)
            {
                address = literal;
                return true;
            }

            try
            {
                foreach (IPAddress candidate in Dns.GetHostEntry(host).AddressList)
                {
                    if (candidate.AddressFamily == AddressFamily.InterNetwork)
                    {
                        address = candidate;
                        return true;
                    }
                }
            }
            catch
            {
                // ignored - caller will display a friendly error
            }

            return false;
        }
    }
}
