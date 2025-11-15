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

        public Client()
        {
            InitializeComponent();
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
                        lbl.Text = $"[{DateTime.Now:HH:mm}] {msg}";
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
                    // Format: [IMAGE]|username|filename|base64data
                    string[] parts = msg.Split('|');
                    string user = parts[1];
                    string fileName = parts[2];
                    string base64 = parts[3];

                    // Label tên người gửi
                    Label userLabel = new Label
                    {
                        Text = user + ":",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9.75f, FontStyle.Regular)
                    };
                    chatPanel.Controls.Add(userLabel);

                    // Base64 → Image
                    byte[] data = Convert.FromBase64String(base64);
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        PictureBox pic = new PictureBox
                        {
                            Image = Image.FromStream(ms),
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Width = 250,
                            Height = 180,
                            Cursor = Cursors.Hand
                        };

                        pic.Click += (s, e) =>
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
                                Image = pic.Image,
                                SizeMode = PictureBoxSizeMode.Zoom
                            };

                            viewer.Controls.Add(big);
                            viewer.ShowDialog();
                        };

                        chatPanel.Controls.Add(pic);
                    }
                    return;
                }

                // === 2) FILE ===
                if (msg.StartsWith("[FILE]"))
                {
                    // Format: [FILE]|username|filename|base64data
                    string[] parts = msg.Split('|');
                    string user = parts[1];
                    string fileName = parts[2];
                    string base64 = parts[3];

                    // Label tên người gửi
                    Label userLabel = new Label
                    {
                        Text = user + ":",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10, FontStyle.Regular)
                    };
                    chatPanel.Controls.Add(userLabel);

                    // Tạo link bấm vào tải file
                    Label link = new Label
                    {
                        Text = "📁 " + fileName,
                        AutoSize = true,
                        ForeColor = Color.Blue,
                        Cursor = Cursors.Hand,
                        Font = new Font("Segoe UI", 9.75f, FontStyle.Underline)
                    };

                    link.Click += (s, e) =>
                    {
                        try
                        {
                            string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                            File.WriteAllBytes(savePath, Convert.FromBase64String(base64));
                            Process.Start(savePath);
                        }
                        catch { MessageBox.Show("Cannot open file."); }
                    };

                    chatPanel.Controls.Add(link);
                    return;
                }

                // === 3) TEXT MESSAGE ===
                Label lbl = new Label
                {
                    Text = msg,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.75f, FontStyle.Regular)
                };
                chatPanel.Controls.Add(lbl);
            });
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
                    obj.handle.WaitOne();
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
                try { ip = Dns.GetHostAddresses(address)[0]; }
                catch { error = true; Log(SystemMsg("Invalid address")); }

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

        //Đóng form
        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            exit = true;
            if (connected) obj.client.Close();
        }

        private void chatPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
