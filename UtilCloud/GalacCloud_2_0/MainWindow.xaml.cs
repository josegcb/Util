using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GalacCloud_2_0 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public string Dominio { get; set; }
        public string DominioOU { get; set; }
        public string RutaOU { get; set; }
        public string Cliente { get; set; }
        public string Rif { get; set; }
        public string Producto { get; set; }
        public string Grupo { get; set; }
        public string Usuario { get; set; }
        public int CantidadUsuario { get; set; }
        public string DataDirectory { get; set; }
        public string LogDirectory { get; set; }
        public MainWindow() {
            InitializeComponent();
            Dominio = ConfigurationManager.AppSettings["Dominio"];
            DominioOU = ConfigurationManager.AppSettings["DominioOU"];
            RutaOU = ConfigurationManager.AppSettings["RutaOU"];
            Cliente = ConfigurationManager.AppSettings["Cliente"];
            Rif = ConfigurationManager.AppSettings["Rif"];
            Producto = ConfigurationManager.AppSettings["Producto"];
            Grupo = $"RD_{Cliente}_{Rif}_{Producto}";
            Usuario = $"Usr_{Cliente}_{Producto}_";
            CantidadUsuario = Convert.ToInt32(ConfigurationManager.AppSettings["CantidadUsuario"]);
            LLenarComboSQL();
            lblDominio.Content = Dominio;
            txtCliente.Text = Cliente;
            txtRif.Text = Rif;
            cmbProducto.Text = Producto;
            txtCantidad.Text = CantidadUsuario.ToString();
        }

        private void Iniciar_Click(object sender, RoutedEventArgs e) {
            Cliente = txtCliente.Text;
            Cliente = Cliente.Trim();
            string pattern = @"[^a-zA-Z0-9\s]";
            Cliente = Regex.Replace(txtCliente.Text, pattern, "");
            if (Cliente.Length <= 30) {
                Cliente = Cliente;
            } else {
                Cliente = Cliente.Substring(0, 30);
            }
            Rif = txtRif.Text;
            Producto = cmbProducto.Text;
            Grupo = $"RD_{Cliente}_{Rif}_{Producto}";
            Usuario = $"Usr_{Cliente}_{Producto}_";
            CantidadUsuario = Convert.ToInt32(txtCantidad.Text);
        }

        private void CrearCliente_Click(object sender, RoutedEventArgs e) {
            CrearClienteEnDirectorio();
        }

        private void CrearEstructura_Click(object sender, RoutedEventArgs e) {
            CrearEstructura();
        }

        private void CrearGrupo_Click(object sender, RoutedEventArgs e) {
            CrearGrupo();
        }

        private void CrearUsuario_Click(object sender, RoutedEventArgs e) {
            CrearUsuario();
        }

        private void AsignarUsuario_Click(object sender, RoutedEventArgs e) {
            AsignarUsuarioAlGrupo();
        }

        private void CrearDB_Click(object sender, RoutedEventArgs e) {
            CrearBaseDeDatos();
        }

       

        private void CrearClienteEnDirectorio() {
            try {
                DirectoryEntry entrada = new DirectoryEntry($"LDAP://{DominioOU}");
                DirectoryEntry ClienteNuevo = entrada.Children.Add($"OU={Cliente},{RutaOU}", "OrganizationalUnit");
                ClienteNuevo.CommitChanges();
                MessageBox.Show("OK");
            } catch (Exception ex) {
                MessageBox.Show(Error(ex));
            }
        }

        private void CrearEstructura() {
            try {
                DirectoryEntry entrada = new DirectoryEntry($"LDAP://{DominioOU}");
                DirectoryEntry ClienteEstructura = entrada.Children.Add($"OU=Grupos, OU={Cliente},{RutaOU}", "OrganizationalUnit");
                ClienteEstructura.CommitChanges();
                ClienteEstructura = entrada.Children.Add($"OU=Usuarios, OU={Cliente},{RutaOU}", "OrganizationalUnit");
                ClienteEstructura.CommitChanges();
                MessageBox.Show("OK");
            } catch (Exception ex) {
                MessageBox.Show(Error(ex));
            }
        }

        private void CrearGrupo() {
            try {
                using (PrincipalContext contexto = new PrincipalContext(ContextType.Domain, Dominio, $"OU=Grupos, OU={Cliente},{RutaOU},{DominioOU}")) {
                    using (GroupPrincipal grupo = new GroupPrincipal(contexto)) {
                        grupo.Name = Grupo;
                        grupo.GroupScope = GroupScope.Global;
                        grupo.IsSecurityGroup = true;
                        grupo.Save();
                    }
                }
                MessageBox.Show("OK");
            } catch (Exception ex) {
                MessageBox.Show(Error(ex));
            }
        }

        private void CrearUsuario() {
            try {
                using (PrincipalContext contexto = new PrincipalContext(ContextType.Domain, Dominio, $"OU=Usuarios, OU={Cliente},{RutaOU},{DominioOU}")) {
                    for (int i = 0; i < CantidadUsuario; i++) {
                        string nombreUsuario = Usuario + i.ToString();
                        string contraseña = GenerarPassword();
                        using (UserPrincipal usuario = new UserPrincipal(contexto)) {
                            usuario.SamAccountName = nombreUsuario;
                            usuario.SetPassword(contraseña);
                            usuario.DisplayName = nombreUsuario;
                            usuario.Enabled = true;
                            usuario.UserCannotChangePassword = true;
                            usuario.PasswordNeverExpires = true;
                            usuario.Save();
                        }
                    }
                }
                MessageBox.Show("OK");
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void AsignarUsuarioAlGrupo() {
            try {
                string groupDn = $"CN={Grupo},OU=Grupos, OU={Cliente},{RutaOU},{DominioOU}";
                string ldapPathUsuarios = $"LDAP://OU=Usuarios, OU={Cliente},{RutaOU},{DominioOU}";
                DirectoryEntry entry = new DirectoryEntry(ldapPathUsuarios);
                DirectorySearcher searcher = new DirectorySearcher(entry) {
                    Filter = "(objectClass=user)"
                };
                searcher.PropertiesToLoad.Add("distinguishedName");
                DirectoryEntry group = new DirectoryEntry($"LDAP://{groupDn}");
                foreach (SearchResult result in searcher.FindAll()) {
                    string userDn = result.Properties["distinguishedName"][0].ToString();
                    group.Properties["member"].Add(userDn);
                    group.CommitChanges();
                }
                MessageBox.Show("OK");
            } catch (Exception ex) {
                MessageBox.Show("ERROR" + Environment.NewLine + Error(ex));
            }
        }

        private void CrearBaseDeDatos() {
            throw new NotImplementedException();
        }

        private void LLenarComboSQL() {
            foreach (ConnectionStringSettings servidor in ConfigurationManager.ConnectionStrings) {
                cbServidores.Items.Add(servidor.Name);
            }
        }

        private string Error(Exception ex) {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(ex.Message);
            if (ex.InnerException != null) {
                stringBuilder.AppendLine(Error(ex.InnerException));
            }
            return stringBuilder.ToString();
        }
        private string GenerarPassword() {
            return "1q2w3e4R*";
        }

        
    }
}