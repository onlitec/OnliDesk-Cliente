using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace OnliDeskSimples
{
    public partial class OnliDeskForm : Form
    {
        private TextBox txtId;
        private Button btnConectar;
        private Label lblStatus;
        private Label lblTitulo;
        
        public OnliDeskForm()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "OnliDesk - Acesso Remoto";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            
            // Título
            lblTitulo = new Label();
            lblTitulo.Text = "OnliDesk";
            lblTitulo.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(197, 48, 48);
            lblTitulo.Location = new Point(180, 30);
            lblTitulo.Size = new Size(150, 40);
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitulo);
            
            // Subtítulo
            var lblSubtitulo = new Label();
            lblSubtitulo.Text = "Acesso Remoto Seguro";
            lblSubtitulo.Font = new Font("Segoe UI", 12);
            lblSubtitulo.ForeColor = Color.Gray;
            lblSubtitulo.Location = new Point(170, 75);
            lblSubtitulo.Size = new Size(170, 25);
            lblSubtitulo.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblSubtitulo);
            
            // Label ID
            var lblId = new Label();
            lblId.Text = "ID de Conexão:";
            lblId.Font = new Font("Segoe UI", 10);
            lblId.Location = new Point(50, 130);
            lblId.Size = new Size(120, 25);
            this.Controls.Add(lblId);
            
            // TextBox ID
            txtId = new TextBox();
            txtId.Font = new Font("Segoe UI", 12);
            txtId.Location = new Point(50, 160);
            txtId.Size = new Size(200, 30);
            txtId.PlaceholderText = "XXX XXX XXX";
            this.Controls.Add(txtId);
            
            // Botão Conectar
            btnConectar = new Button();
            btnConectar.Text = "Conectar";
            btnConectar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnConectar.BackColor = Color.FromArgb(197, 48, 48);
            btnConectar.ForeColor = Color.White;
            btnConectar.FlatStyle = FlatStyle.Flat;
            btnConectar.Location = new Point(270, 160);
            btnConectar.Size = new Size(120, 35);
            btnConectar.Click += BtnConectar_Click;
            this.Controls.Add(btnConectar);
            
            // Status
            lblStatus = new Label();
            lblStatus.Text = "OnliDesk carregado - Servidor: 172.20.120.40:7070";
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.ForeColor = Color.Green;
            lblStatus.Location = new Point(50, 220);
            lblStatus.Size = new Size(400, 25);
            this.Controls.Add(lblStatus);
            
            // Configurações
            var btnConfig = new Button();
            btnConfig.Text = "Configurações";
            btnConfig.Font = new Font("Segoe UI", 10);
            btnConfig.Location = new Point(50, 270);
            btnConfig.Size = new Size(120, 30);
            btnConfig.Click += BtnConfig_Click;
            this.Controls.Add(btnConfig);
            
            // Sobre
            var btnSobre = new Button();
            btnSobre.Text = "Sobre";
            btnSobre.Font = new Font("Segoe UI", 10);
            btnSobre.Location = new Point(180, 270);
            btnSobre.Size = new Size(80, 30);
            btnSobre.Click += BtnSobre_Click;
            this.Controls.Add(btnSobre);
            
            // Sair
            var btnSair = new Button();
            btnSair.Text = "Sair";
            btnSair.Font = new Font("Segoe UI", 10);
            btnSair.Location = new Point(270, 270);
            btnSair.Size = new Size(80, 30);
            btnSair.Click += (s, e) => this.Close();
            this.Controls.Add(btnSair);
        }
        
        private void BtnConectar_Click(object sender, EventArgs e)
        {
            string id = txtId.Text.Trim();
            
            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("Por favor, digite um ID de conexão.", "OnliDesk", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            lblStatus.Text = $"Conectando ao ID: {id}...";
            lblStatus.ForeColor = Color.Orange;
            
            // Simular conexão
            var timer = new Timer();
            timer.Interval = 2000;
            timer.Tick += (s, ev) => {
                timer.Stop();
                lblStatus.Text = $"Conectado com sucesso ao ID: {id}";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show($"Conexão estabelecida com sucesso!\n\nID: {id}\nServidor: 172.20.120.40:7070", 
                              "OnliDesk - Conectado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            timer.Start();
        }
        
        private void BtnConfig_Click(object sender, EventArgs e)
        {
            var configForm = new ConfigForm();
            configForm.ShowDialog();
        }
        
        private void BtnSobre_Click(object sender, EventArgs e)
        {
            MessageBox.Show("OnliDesk v1.1.0\n\n" +
                          "Acesso Remoto Seguro\n" +
                          "© 2024 OnliTec\n\n" +
                          "Servidor: 172.20.120.40:7070\n" +
                          "Protocolo: HTTP/HTTPS\n" +
                          "Autenticação: JWT\n\n" +
                          "Desenvolvido com .NET 8",
                          "Sobre o OnliDesk", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            this.Text = "OnliDesk - Configurações";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            var lblServidor = new Label();
            lblServidor.Text = "Servidor:";
            lblServidor.Location = new Point(20, 30);
            lblServidor.Size = new Size(80, 25);
            this.Controls.Add(lblServidor);
            
            var txtServidor = new TextBox();
            txtServidor.Text = "172.20.120.40";
            txtServidor.Location = new Point(100, 30);
            txtServidor.Size = new Size(150, 25);
            this.Controls.Add(txtServidor);
            
            var lblPorta = new Label();
            lblPorta.Text = "Porta:";
            lblPorta.Location = new Point(20, 70);
            lblPorta.Size = new Size(80, 25);
            this.Controls.Add(lblPorta);
            
            var txtPorta = new TextBox();
            txtPorta.Text = "7070";
            txtPorta.Location = new Point(100, 70);
            txtPorta.Size = new Size(80, 25);
            this.Controls.Add(txtPorta);
            
            var btnSalvar = new Button();
            btnSalvar.Text = "Salvar";
            btnSalvar.Location = new Point(100, 120);
            btnSalvar.Size = new Size(80, 30);
            btnSalvar.Click += (s, e) => {
                MessageBox.Show("Configurações salvas com sucesso!", "OnliDesk", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            };
            this.Controls.Add(btnSalvar);
            
            var btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(190, 120);
            btnCancelar.Size = new Size(80, 30);
            btnCancelar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancelar);
        }
    }
    
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                Application.Run(new OnliDeskForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao executar OnliDesk:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
