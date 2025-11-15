using System;
using System.Windows.Forms;

namespace Server
{
    public partial class Message : Form
    {
        public long ClientId { get; private set; }
        public string MessageText { get; private set; }

        public Message(long clientId)
        {
            InitializeComponent();
            ClientId = clientId;  // Nhận clientId từ Server
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            MessageText = textBox1.Text;  // Lấy tin nhắn từ TextBox
            if (!string.IsNullOrEmpty(MessageText))  // Kiểm tra nếu tin nhắn không trống
            {
                this.DialogResult = DialogResult.OK;  // Đặt kết quả là OK để đóng form
                this.Close();  // Đóng form
            }
            else
            {
                MessageBox.Show("Please enter a message.");  // Hiển thị thông báo nếu không có tin nhắn
            }
        }
    }
}
