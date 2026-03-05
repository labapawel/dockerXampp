using DockerManager.Services;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using System.Drawing;

namespace DockerManager
{
    public partial class MainForm : MaterialForm
    {
        private readonly DockerService _dockerService;
        private IList<ContainerListResponse> _containers;

        // Lista dozwolonych / zarządzalnych kontenerów w tej aplikacji
        private readonly List<AppContainerInfo> _managedContainers = new List<AppContainerInfo>
        {
            new AppContainerInfo { Id = "projekt_www", Name = "WWW Stack (Apache/PHP/MySQL/PMA)" },
            new AppContainerInfo { Id = "projekt_mysql_standalone", Name = "MySQL Standalone" },
            new AppContainerInfo { Id = "projekt_postgres", Name = "PostgreSQL" },
            new AppContainerInfo { Id = "projekt_redis", Name = "Redis" }
        };

        public MainForm()
        {
            InitializeComponent();
            _dockerService = new DockerService();
            _containers = new List<ContainerListResponse>();

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await RefreshContainerList();
        }

        private async Task RefreshContainerList()
        {
            try
            {
                var containers = await _dockerService.ListContainersAsync();
                _containers = containers;

                flowLayoutPanelContainers.SuspendLayout();
                flowLayoutPanelContainers.Controls.Clear();

                foreach (var managed in _managedContainers)
                {
                    // Szukamy, czy kontener istnieje w dockerze
                    var dockerContainer = _containers.FirstOrDefault(c => 
                        c.Names.Any(n => n.Contains(managed.Id))
                    );

                    bool isInstalled = dockerContainer != null;
                    bool isRunning = isInstalled && dockerContainer.State == "running";

                    // Tworzymy kartę / panel dla każdego kontenera
                    var card = new MaterialCard();
                    card.Width = 750; // Zwiększone dla szerszego okna
                    card.Height = 120;
                    card.Margin = new Padding(10, 5, 10, 5);

                    // Etykieta nazwy
                    var lblName = new MaterialLabel();
                    lblName.Text = managed.Name;
                    lblName.Location = new Point(15, 15);
                    lblName.AutoSize = true;
                    card.Controls.Add(lblName);

                    // Etykieta statusu
                    var lblState = new MaterialLabel();
                    lblState.Text = isInstalled ? $"Stan: {dockerContainer.State} - {dockerContainer.Status}" : "Stan: Nie zainstalowano";
                    lblState.Location = new Point(15, 45);
                    lblState.AutoSize = true;
                    lblState.FontType = MaterialSkinManager.fontType.Body2;
                    if(isRunning) lblState.ForeColor = Color.LightGreen;
                    card.Controls.Add(lblState);

                    // Przycisk głównej akcji (Start / Stop / Instaluj)
                    var mainActionBtn = new MaterialButton();
                    mainActionBtn.AutoSize = false;
                    mainActionBtn.Size = new Size(120, 40);
                    mainActionBtn.Location = new Point(450, 65);
                    mainActionBtn.Type = MaterialButton.MaterialButtonType.Contained;
                    
                    if (!isInstalled)
                    {
                        mainActionBtn.Text = "Instaluj";
                        mainActionBtn.Click += async (s, e) => await InstallContainerAction(managed.Id);
                    }
                    else if (isRunning)
                    {
                        mainActionBtn.Text = "Stop";
                        mainActionBtn.UseAccentColor = false; // Normalny kolor dla Stop
                        mainActionBtn.Click += async (s, e) => {
                            UpdateStatus($"Zatrzymywanie {managed.Name}...");
                            await _dockerService.StopContainerAsync(dockerContainer.ID);
                            await RefreshContainerList();
                        };
                    }
                    else
                    {
                        mainActionBtn.Text = "Start";
                        mainActionBtn.UseAccentColor = false;
                        mainActionBtn.Click += async (s, e) => {
                            UpdateStatus($"Uruchamianie {managed.Name}...");
                            await _dockerService.StartContainerAsync(dockerContainer.ID);
                            await RefreshContainerList();
                        };
                    }
                    card.Controls.Add(mainActionBtn);

                    // Przycisk Usuń (tylko, jeżeli zainstalowany)
                    if (isInstalled)
                    {
                        var removeBtn = new System.Windows.Forms.Button();
                        removeBtn.Text = "Usuń";
                        removeBtn.FlatStyle = FlatStyle.Flat;
                        removeBtn.FlatAppearance.BorderSize = 0;
                        removeBtn.BackColor = Color.FromArgb(220, 53, 69); // Czerwony kolor
                        removeBtn.ForeColor = Color.White;
                        removeBtn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                        removeBtn.Cursor = Cursors.Hand;
                        removeBtn.Size = new Size(120, 40);
                        removeBtn.Location = new Point(585, 65);
                        removeBtn.Click += async (s, e) => {
                            var result = MessageBox.Show($"Czy na pewno chcesz usunąć {managed.Name}? Wolumeny również zostaną usunięte.", "Potwierdź", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (result == DialogResult.Yes)
                            {
                                UpdateStatus($"Usuwanie {managed.Name}...");
                                await _dockerService.RemoveContainerAsync(dockerContainer.ID);
                                await RefreshContainerList();
                            }
                        };
                        card.Controls.Add(removeBtn);
                    }

                    flowLayoutPanelContainers.Controls.Add(card);
                }

                flowLayoutPanelContainers.ResumeLayout();
                UpdateStatus("Odświeżono listę.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Błąd odświeżania: {ex.Message}");
            }
        }

        private async Task InstallContainerAction(string id)
        {
            try
            {
                if (id == "projekt_www")
                {
                    UpdateStatus("Uruchamiam docker-compose up dla WWW...");
                    string composeDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "container", "www");
                    composeDir = System.IO.Path.GetFullPath(composeDir);
                    
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "docker-compose",
                        Arguments = "up -d",
                        WorkingDirectory = composeDir,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                else if (id == "projekt_mysql_standalone")
                {
                    UpdateStatus("Instalacja MySQL...");
                    string cid = await _dockerService.CreateContainerAsync("mysql", "8.0", "projekt_mysql_standalone",
                        new HostConfig { PortBindings = new Dictionary<string, IList<PortBinding>> { { "3306/tcp", new List<PortBinding> { new PortBinding { HostPort = "3306" } } } } }, 
                        new List<string> { "MYSQL_ALLOW_EMPTY_PASSWORD=yes" });
                    await _dockerService.StartContainerAsync(cid);
                }
                else if (id == "projekt_postgres")
                {
                    UpdateStatus("Instalacja PostgreSQL...");
                    string cid = await _dockerService.CreateContainerAsync("postgres", "latest", "projekt_postgres",
                        new HostConfig { PortBindings = new Dictionary<string, IList<PortBinding>> { { "5432/tcp", new List<PortBinding> { new PortBinding { HostPort = "5432" } } } } }, 
                        new List<string> { "POSTGRES_PASSWORD=root" });
                    await _dockerService.StartContainerAsync(cid);
                }
                else if (id == "projekt_redis")
                {
                    UpdateStatus("Instalacja Redis...");
                    string cid = await _dockerService.CreateContainerAsync("redis", "latest", "projekt_redis",
                        new HostConfig { PortBindings = new Dictionary<string, IList<PortBinding>> { { "6379/tcp", new List<PortBinding> { new PortBinding { HostPort = "6379" } } } } });
                    await _dockerService.StartContainerAsync(cid);
                }

                await RefreshContainerList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void refreshTimer_Tick(object sender, EventArgs e)
        {
            await RefreshContainerList();
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = message;
        }
    }

    public class AppContainerInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
