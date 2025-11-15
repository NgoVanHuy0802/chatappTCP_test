using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        private bool active = false;
        private Thread listener = null;
        private long id = 0;
        private struct MyClient
        {
            public long id;
            public StringBuilder username;
            public TcpClient client;
            public NetworkStream stream;
            public byte[] buffer;
            public StringBuilder data;
            public EventWaitHandle handle;
        };
        private ConcurrentDictionary<long, MyClient> clients = new ConcurrentDictionary<long, MyClient>();
        private Task send = null;
        private Thread disconnect = null;
        private bool exit = false;
        private TcpClient tcpClient;

        public Server()
        {
            InitializeComponent();
        }

        private void Log(string msg)
        {
            if (exit || string.IsNullOrWhiteSpace(msg))
            {
                return;
            }

            chatPanel.Invoke((MethodInvoker)delegate
            {
                Color color = SystemColors.ControlText;
                if (msg.StartsWith("ERROR"))
                {
                    color = Color.Firebrick;
                }
                else if (msg.StartsWith("SYSTEM"))
                {
                    color = Color.SteelBlue;
                }

                int maxWidth = Math.Max(50, chatPanel.ClientSize.Width - 25);

                Label logEntry = new Label
                {
                    AutoSize = true,
                    MaximumSize = new Size(maxWidth, 0),
                    ForeColor = color,
                    Text = string.Format("[ {0} ] {1}", DateTime.Now.ToString("HH:mm"), msg),
                    Margin = new Padding(0, 0, 0, 4)
                };

                chatPanel.Controls.Add(logEntry);
                chatPanel.ScrollControlIntoView(logEntry);
            });
        }

        private string ErrorMsg(string msg)
        {
            return string.Format("ERROR: {0}", msg);
        }

        private string SystemMsg(string msg)
        {
            return string.Format("SYSTEM: {0}", msg);
        }

        private void Active(bool status)
        {
            if (!exit)
            {
                startButton.Invoke((MethodInvoker)delegate
                {
                    active = status;
                    if (status)
                    {
                        addrTextBox.Enabled = false;
                        portTextBox.Enabled = false;
                        usernameTextBox.Enabled = false;
                        keyTextBox.Enabled = false;
                        startButton.Text = "Stop";
                        Log(SystemMsg("Server has started"));
                    }
                    else
                    {
                        addrTextBox.Enabled = true;
                        portTextBox.Enabled = true;
                        usernameTextBox.Enabled = true;
                        keyTextBox.Enabled = true;
                        startButton.Text = "Start";
                        Log(SystemMsg("Server has stopped"));
                    }
                });
            }
        }

        private void AddToGrid(long id, string name)
        {
            if (!exit)
            {
                clientsDataGridView.Invoke((MethodInvoker)delegate
                {
                    string[] row = new string[] { id.ToString(), name };
                    clientsDataGridView.Rows.Add(row);
                    totalLabel.Text = string.Format("Total clients: {0}", clientsDataGridView.Rows.Count);
                });
            }
        }

        private void RemoveFromGrid(long id)
        {
            if (!exit)
            {
                clientsDataGridView.Invoke((MethodInvoker)delegate
                {
                    foreach (DataGridViewRow row in clientsDataGridView.Rows)
                    {
                        if (row.Cells["identifier"].Value.ToString() == id.ToString())
                        {
                            clientsDataGridView.Rows.RemoveAt(row.Index);
                            break;
                        }
                    }
                    totalLabel.Text = string.Format("Total clients: {0}", clientsDataGridView.Rows.Count);
                });
            }
        }

        private void Read(IAsyncResult result)
        {
            MyClient obj = (MyClient)result.AsyncState;
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
                    {
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, Read, obj);
                    }
                    else
                    {
                        string rawMsg = obj.data.ToString();   // Tin nhắn gốc từ client
                        obj.data.Clear();

                        //------------------------------------------------------
                        // 🔥 KIỂM TRA LOẠI TIN NHẮN ĐỂ SERVER HIỂN BÌNH THƯỜNG
                        //------------------------------------------------------
                        string logMsg;

                        if (rawMsg.StartsWith("[IMAGE]"))
                        {
                            logMsg = $"{obj.username} sent an image.";
                            DisplayAttachment(rawMsg);
                        }
                        else if (rawMsg.StartsWith("[FILE]"))
                        {
                            logMsg = $"{obj.username} sent a file.";
                            DisplayAttachment(rawMsg);
                        }
                        else
                        {
                            logMsg = $"{obj.username}: {rawMsg}";
                        }

                        Log(logMsg);  // Server chỉ log thông báo sạch sẽ

                        //------------------------------------------------------
                        // 🔥 GỬI NGUYÊN BẢN (Base64 / File bytes / Text) CHO CLIENT
                        //------------------------------------------------------
                        Send(rawMsg, obj.id);

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
            MyClient obj = (MyClient)result.AsyncState;
            int bytes = 0;
            if (obj.client.Connected)
            {
                try
                {
                    bytes = obj.stream.EndRead(result);
                }
                catch (Exception ex)
                {
                    Log(ErrorMsg(ex.Message));
                }
            }
            if (bytes > 0)
            {
                obj.data.AppendFormat("{0}", Encoding.UTF8.GetString(obj.buffer, 0, bytes));
                try
                {
                    if (obj.stream.DataAvailable)
                    {
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(ReadAuth), obj);
                    }
                    else
                    {
                        JavaScriptSerializer json = new JavaScriptSerializer(); // feel free to use JSON serializer
                        Dictionary<string, string> data = json.Deserialize<Dictionary<string, string>>(obj.data.ToString());
                        if (!data.ContainsKey("username") || data["username"].Length < 1 || !data.ContainsKey("key") || !data["key"].Equals(keyTextBox.Text))
                        {
                            obj.client.Close();
                        }
                        else
                        {
                            obj.username.Append(data["username"].Length > 200 ? data["username"].Substring(0, 200) : data["username"]);
                            Send("{\"status\": \"authorized\"}", obj);
                        }
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

        private bool Authorize(MyClient obj)
        {
            bool success = false;
            while (obj.client.Connected)
            {
                try
                {
                    obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(ReadAuth), obj);
                    obj.handle.WaitOne();
                    if (obj.username.Length > 0)
                    {
                        success = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log(ErrorMsg(ex.Message));
                }
            }
            return success;
        }

        private void Connection(MyClient obj)
        {
            if (Authorize(obj))
            {
                tcpClient = obj.client;
                clients.TryAdd(obj.id, obj);
                AddToGrid(obj.id, obj.username.ToString());
                string msg = string.Format("{0} has connected", obj.username);
                Log(SystemMsg(msg));
                Send(SystemMsg(msg), obj.id);
                while (obj.client.Connected)
                {
                    try
                    {
                        obj.stream.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(Read), obj);
                        obj.handle.WaitOne();
                    }
                    catch (Exception ex)
                    {
                        Log(ErrorMsg(ex.Message));
                    }
                }
                obj.client.Close();
                clients.TryRemove(obj.id, out MyClient tmp);
                RemoveFromGrid(tmp.id);
                msg = string.Format("{0} has disconnected", tmp.username);
                Log(SystemMsg(msg));
                Send(msg, tmp.id);
            }
        }

        private void Listener(IPAddress ip, int port)
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(ip, port);
                listener.Start();
                Active(true);
                while (active)
                {
                    if (listener.Pending())
                    {
                        try
                        {
                            MyClient obj = new MyClient();
                            obj.id = id;
                            obj.username = new StringBuilder();
                            obj.client = listener.AcceptTcpClient();
                            obj.stream = obj.client.GetStream();
                            obj.buffer = new byte[obj.client.ReceiveBufferSize];
                            obj.data = new StringBuilder();
                            obj.handle = new EventWaitHandle(false, EventResetMode.AutoReset);
                            Thread th = new Thread(() => Connection(obj))
                            {
                                IsBackground = true
                            };
                            th.Start();
                            id++;
                        }
                        catch (Exception ex)
                        {
                            Log(ErrorMsg(ex.Message));
                        }
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                Active(false);
            }
            catch (Exception ex)
            {
                Log(ErrorMsg(ex.Message));
            }
            finally
            {
                if (listener != null)
                {
                    listener.Server.Close();
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (active)
            {
                active = false;
            }
            else if (listener == null || !listener.IsAlive)
            {
                string address = addrTextBox.Text.Trim();
                string number = portTextBox.Text.Trim();
                string username = usernameTextBox.Text.Trim();
                bool error = false;
                IPAddress ip = null;
                if (address.Length < 1)
                {
                    error = true;
                    Log(SystemMsg("Address is required"));
                }
                else if (!TryResolveIPv4(address, out ip))
                {
                    error = true;
                    Log(SystemMsg("Address is not valid or is not IPv4"));
                }
                int port = -1;
                if (number.Length < 1)
                {
                    error = true;
                    Log(SystemMsg("Port number is required"));
                }
                else if (!int.TryParse(number, out port))
                {
                    error = true;
                    Log(SystemMsg("Port number is not valid"));
                }
                else if (port < 0 || port > 65535)
                {
                    error = true;
                    Log(SystemMsg("Port number is out of range"));
                }
                if (username.Length < 1)
                {
                    error = true;
                    Log(SystemMsg("Username is required"));
                }
                if (!error)
                {
                    listener = new Thread(() => Listener(ip, port))
                    {
                        IsBackground = true
                    };
                    listener.Start();
                }
            }
        }

        private void Write(IAsyncResult result)
        {
            MyClient obj = (MyClient)result.AsyncState;
            if (obj.client.Connected)
            {
                try
                {
                    obj.stream.EndWrite(result);
                }
                catch (Exception ex)
                {
                    Log(ErrorMsg(ex.Message));
                }
            }
        }

        private void BeginWrite(string msg, MyClient obj) // send the message to a specific client
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            if (obj.client.Connected)
            {
                try
                {
                    obj.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), obj);
                }
                catch (Exception ex)
                {
                    Log(ErrorMsg(ex.Message));
                }
            }
        }

        private void BeginWrite(string msg, long id = -1) // send the message to everyone except the sender or set ID to lesser than zero to send to everyone
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            foreach (KeyValuePair<long, MyClient> obj in clients)
            {
                if (id != obj.Value.id && obj.Value.client.Connected)
                {
                    try
                    {
                        obj.Value.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), obj.Value);
                    }
                    catch (Exception ex)
                    {
                        Log(ErrorMsg(ex.Message));
                    }
                }
            }
        }

        private void Send(string msg, MyClient obj)
        {
            if (send == null || send.IsCompleted)
            {
                send = Task.Factory.StartNew(() => BeginWrite(msg, obj));
            }
            else
            {
                send.ContinueWith(antecendent => BeginWrite(msg, obj));
            }
        }

        private void Send(string msg, long id = -1)
        {
            if (send == null || send.IsCompleted)
            {
                send = Task.Factory.StartNew(() => BeginWrite(msg, id));
            }
            else
            {
                send.ContinueWith(antecendent => BeginWrite(msg, id));
            }
        }

        private void SendTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (sendTextBox.Text.Length > 0)
                {
                    string msg = sendTextBox.Text;
                    sendTextBox.Clear();
                    Log(string.Format("{0} (You): {1}", usernameTextBox.Text.Trim(), msg));
                    Send(string.Format("{0}: {1}", usernameTextBox.Text.Trim(), msg));
                }
            }
        }
        private void ClientsDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == clientsDataGridView.Columns["dc"].Index)
            {
                long.TryParse(clientsDataGridView.Rows[e.RowIndex].Cells["identifier"].Value.ToString(), out long id);
                Disconnect(id);
            }
            if (e.RowIndex >= 0 && e.ColumnIndex == clientsDataGridView.Columns["Message"].Index)
            {
                long clientId = Convert.ToInt64(clientsDataGridView.Rows[e.RowIndex].Cells["identifier"].Value);
                using (Message messageForm = new Message(clientId))
                {
                    if (messageForm.ShowDialog() == DialogResult.OK)
                    {
                        string message = messageForm.MessageText;
                        SendMessageToClient(message, clientId);
                    }
                }
            }
        }
        // Phương thức gửi tin nhắn cho client
        private void SendMessageToClient(string message, long clientId)
        {
            if (clients.TryGetValue(clientId, out MyClient client))
            {
                string finalMsg = $"Server: {message}";

                byte[] buffer = Encoding.UTF8.GetBytes(finalMsg);
                client.stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(Write), client);

                Log($"SYSTEM: Gửi tin nhắn riêng đến client ({client.username}): {message}");
            }
        }


        private void Disconnect(long id = -1) // disconnect everyone if ID is not supplied or is lesser than zero
        {
            if (disconnect == null || !disconnect.IsAlive)
            {
                disconnect = new Thread(() =>
                {
                    if (id >= 0)
                    {
                        clients.TryGetValue(id, out MyClient obj);
                        obj.client.Close();
                        RemoveFromGrid(obj.id);
                    }
                    else
                    {
                        foreach (KeyValuePair<long, MyClient> obj in clients)
                        {
                            obj.Value.client.Close();
                            RemoveFromGrid(obj.Value.id);
                        }
                    }
                })
                {
                    IsBackground = true
                };
                disconnect.Start();
            }
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            exit = true;
            active = false;
            Disconnect();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            chatPanel.Controls.Clear();
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (keyTextBox.PasswordChar == '*')
            {
                keyTextBox.PasswordChar = '\0';
            }
            else
            {
                keyTextBox.PasswordChar = '*';
            }
        }

        private void clientsDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private bool TryResolveIPv4(string host, out IPAddress ipAddress)
        {
            ipAddress = null;
            if (IPAddress.TryParse(host, out IPAddress literal) && literal.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = literal;
                return true;
            }

            try
            {
                foreach (IPAddress candidate in Dns.GetHostEntry(host).AddressList)
                {
                    if (candidate.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = candidate;
                        return true;
                    }
                }
            }
            catch
            {
                // ignored - method will return false so we can show a friendly error.
            }

            return false;
        }

        private void DisplayAttachment(string msg)
        {
            if (exit)
            {
                return;
            }

            chatPanel.Invoke((MethodInvoker)delegate
            {
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
                    chatPanel.ScrollControlIntoView(pic);
                    return;
                }

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
                    chatPanel.ScrollControlIntoView(fileContainer);
                }
            });
        }

        private void AddSenderLabel(string user)
        {
            Label userLabel = new Label
            {
                Text = user + ":",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.75f, FontStyle.Regular)
            };
            chatPanel.Controls.Add(userLabel);
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
    }
}
