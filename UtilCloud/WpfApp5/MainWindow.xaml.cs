using System;
using System.Management;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Contexts;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Reflection;
using System.Security.Principal;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Text.RegularExpressions;
using System.Security.Policy;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Data;
using System.Collections;
using static System.Net.WebRequestMethods;


namespace WpfApp5 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public string Dominio { get; set; }
        public string DominioOU { get; set; }
        public string ClientesOU { get; set; }
        public string Cliente { get; set; }
        public string Rif { get; set; }
        public string Producto { get; set; }
        public string Grupo { get; set; }
        public string Usuario { get; set; }
        public string ServidorBD { get; set; }
        public int CantidadUsuario { get; set; }
        public string DataDirectory { get; set; }
        public string LogDirectory { get; set; }
        public string RutaTenants { get; set; }
        public string FormatoDeFechaEnElServidor { get; set; }
        public List<UsuarioCreado> UsuariosCreados { get; set; }
        public DatosSQL Datos_SQL { get; set; }
        public string Tenant { get; set; }
        public string ClientesRuta { get; set; }
        public int IndiceCliente { get; set; }
        public bool NuevoProducto { get; set; }
        public MainWindow() {
            InitializeComponent();
            Dominio = ConfigurationManager.AppSettings["Dominio"];
            DominioOU = ConfigurationManager.AppSettings["DominioOU"];
            ClientesOU = ConfigurationManager.AppSettings["ClientesOU"];

            ClientesRuta = ConfigurationManager.AppSettings["ClientesRuta"];

            DataDirectory = ConfigurationManager.AppSettings["DataDirectory"];
            LogDirectory = ConfigurationManager.AppSettings["LogDirectory"];

            RutaTenants = ConfigurationManager.AppSettings["RutaTenant"];
            FormatoDeFechaEnElServidor = ConfigurationManager.AppSettings["FormatoDeFechaEnElServidor"];

            LLenarCombos();
            lblDominio.Content = Dominio;
            lblmdf.Content = DataDirectory;
            lblldf.Content = LogDirectory;
        }



        private void Iniciar_Click(object sender, RoutedEventArgs e) {
            try {
                ProcesarSolicitud();
                MessageBox.Show("OK");
            } catch (Exception ex) {
                MessageBox.Show(Helper.Error(ex) + Environment.NewLine + ex.StackTrace);
            }
        }
        private void Listar_Click(object sender, RoutedEventArgs e) {
            try {
                ListarUsuario();
            } catch (Exception ex) {
                MessageBox.Show(Helper.Error(ex) + Environment.NewLine + ex.StackTrace);
            }
        }

        private void Eliminar_Click(object sender, RoutedEventArgs e) {
            try {
                Eliminar();
                MessageBox.Show("Usuarios desactivados");
            } catch (Exception ex) {
                MessageBox.Show(Helper.Error(ex) + Environment.NewLine + ex.StackTrace);
            }
        }


        private void ProcesarSolicitud() {
            try {
                Cliente = txtCliente.Text;
                Rif = txtRif.Text;
                Producto = cmbProducto.Text;
                Grupo = $"RD_{Rif}_{Producto}";
                Usuario = $"UsrCloud";
                CantidadUsuario = Convert.ToInt32(txtCantidad.Text);
                ServidorBD = cmbServidores.Text;
                UsuariosCreados = new List<UsuarioCreado>();
                NuevoProducto = false;
                Datos_SQL = new DatosSQL();
                CrearClienteEnDirectorio();
                if (NuevoProducto) {
                    CrearBaseDeDatos();
                    CrearTenant();
                }
                CrearPerfil();
                Finalizar();
            } catch (Exception ex) {
                throw ex;
            }
        }
        private void CrearClienteEnDirectorio() {
            try {
                if (!ExisteClienteEnAD()) {
                    using (DirectoryEntry entrada = new DirectoryEntry($"LDAP://{DominioOU}")) {
                        using (DirectoryEntry ClienteNuevo = entrada.Children.Add($"OU={Rif},{ClientesOU}", "OrganizationalUnit")) {
                            ClienteNuevo.Properties["description"].Value = Cliente;
                            IndiceCliente = CountCliente() + 1;
                            ClienteNuevo.Properties["postalCode"].Value = IndiceCliente.ToString();
                            ClienteNuevo.CommitChanges();
                        }
                    }
                    CrearEstructura();
                } else {
                    IndiceCliente = IndiceClienteEnAD();
                }

                CrearGrupo();
                CrearUsuario();
            } catch (Exception ex) {
                throw ex;
            }
        }

        private bool ExisteClienteEnAD() {
            try {
                using (DirectoryEntry rootOU = new DirectoryEntry($"LDAP://{ClientesOU},{DominioOU}")) {
                    using (DirectorySearcher searcher = new DirectorySearcher(rootOU)) {
                        searcher.Filter = $"(name={Rif})";
                        searcher.SearchScope = SearchScope.OneLevel;
                        SearchResultCollection results = searcher.FindAll();
                        return results.Count > 0;
                    }
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private int CountCliente() {
            try {
                int max = 0;
                using (DirectoryEntry rootOU = new DirectoryEntry($"LDAP://{ClientesOU},{DominioOU}")) {
                    using (DirectorySearcher searcher = new DirectorySearcher(rootOU)) {
                        searcher.Filter = "(objectCategory=organizationalUnit)";
                        searcher.SearchScope = SearchScope.OneLevel;
                        SearchResultCollection results = searcher.FindAll();
                        foreach (SearchResult result in results) {
                            int tmp = Convert.ToInt32(result.Properties["postalCode"][0].ToString());
                            if (tmp > max) {
                                max = tmp;
                            }
                        }
                        return max;
                    }
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private int IndiceClienteEnAD() {
            try {
                using (DirectoryEntry rootOU = new DirectoryEntry($"LDAP://{ClientesOU},{DominioOU}")) {
                    using (DirectorySearcher searcher = new DirectorySearcher(rootOU)) {
                        searcher.Filter = $"(name={Rif})";
                        searcher.SearchScope = SearchScope.OneLevel;
                        SearchResultCollection results = searcher.FindAll();
                        return Convert.ToInt32(results[0].Properties["postalCode"][0].ToString());
                    }
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void CrearEstructura() {
            try {
                using (DirectoryEntry entrada = new DirectoryEntry($"LDAP://{DominioOU}")) {
                    using (DirectoryEntry ClienteEstructura = entrada.Children.Add($"OU=Grupos, OU={Rif},{ClientesOU}", "OrganizationalUnit")) {
                        ClienteEstructura.CommitChanges();
                    }
                    using (DirectoryEntry ClienteEstructura = entrada.Children.Add($"OU=Usuarios, OU={Rif},{ClientesOU}", "OrganizationalUnit")) {
                        ClienteEstructura.CommitChanges();
                    }
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void CrearGrupo() {
            try {
                if (!ExisteGrupoEnElCliente()) {
                    using (PrincipalContext contexto = new PrincipalContext(ContextType.Domain, Dominio, $"OU=Grupos, OU={Rif},{ClientesOU},{DominioOU}")) {
                        using (GroupPrincipal grupo = new GroupPrincipal(contexto)) {
                            grupo.Name = Grupo;
                            grupo.GroupScope = GroupScope.Global;
                            grupo.IsSecurityGroup = true;
                            grupo.Description = $"{Cliente}  {Producto} ";
                            grupo.Save();
                        }
                    }
                    NuevoProducto = true;
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private bool ExisteGrupoEnElCliente() {
            bool result;
            using (DirectoryEntry rootOU = new DirectoryEntry($"LDAP://OU=Grupos, OU={Rif},{ClientesOU},{DominioOU}")) {
                using (DirectorySearcher searcher = new DirectorySearcher(rootOU)) {
                    searcher.Filter = $"(name={Grupo})";
                    searcher.SearchScope = SearchScope.OneLevel;
                    SearchResultCollection results = searcher.FindAll();
                    result = results.Count > 0;
                }
            }

            return result;
        }

        private void CrearUsuario() {
            int vUsuariosExistentes = 0;
            try {
                using (DirectoryEntry rootOU = new DirectoryEntry($"LDAP://OU=Usuarios, OU={Rif},{ClientesOU},{DominioOU}")) {
                    using (DirectorySearcher searcher = new DirectorySearcher(rootOU)) {
                        searcher.Filter = "(objectCategory=user)";
                        //searcher.Filter = $"(&(objectClass=user)(sn={Producto}))";
                        searcher.SearchScope = SearchScope.OneLevel;
                        SearchResultCollection results = searcher.FindAll();
                        foreach (SearchResult result in results) {
                            int tmp = Convert.ToInt32(result.Properties["middleName"][0].ToString());
                            if (tmp > vUsuariosExistentes) {
                                vUsuariosExistentes = tmp;
                            }
                        }
                    }
                }
                using (PrincipalContext contexto = new PrincipalContext(ContextType.Domain, Dominio, $"OU=Usuarios, OU={Rif},{ClientesOU},{DominioOU}")) {
                    for (int i = 0; i < CantidadUsuario; i++) {
                        vUsuariosExistentes++;
                        string nombreUsuario = Usuario + IndiceCliente.ToString() + "_" + vUsuariosExistentes.ToString();
                        string contraseña = Helper.GenerarPassword();
                        using (UserPrincipal usuario = new UserPrincipal(contexto)) {
                            usuario.SamAccountName = nombreUsuario;
                            usuario.SetPassword(contraseña);
                            usuario.DisplayName = nombreUsuario;
                            usuario.Enabled = true;
                            usuario.UserCannotChangePassword = true;
                            usuario.PasswordNeverExpires = true;
                            usuario.Description = Cliente;
                            usuario.Surname = Producto;
                            usuario.MiddleName = vUsuariosExistentes.ToString();
                            usuario.Save();
                            UsuariosCreados.Add(new UsuarioCreado(nombreUsuario, contraseña));
                        }
                    }
                }
                AsignarUsuarioAlGrupo();
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void AsignarUsuarioAlGrupo() {
            try {
                string groupDn = $"CN={Grupo},OU=Grupos, OU={Rif},{ClientesOU},{DominioOU}";
                string ldapPathUsuarios = $"LDAP://OU=Usuarios, OU={Rif},{ClientesOU},{DominioOU}";
                DirectoryEntry entry = new DirectoryEntry(ldapPathUsuarios);
                DirectorySearcher searcher = new DirectorySearcher(entry) {
                    //Filter = "(objectClass=user)"
                    Filter = $"(&(objectClass=user)(sn={Producto}))"
                };
                searcher.PropertiesToLoad.Add("distinguishedName");
                DirectoryEntry group = new DirectoryEntry($"LDAP://{groupDn}");
                foreach (SearchResult result in searcher.FindAll()) {
                    string userDn = result.Properties["distinguishedName"][0].ToString();
                    group.Properties["member"].Add(userDn);
                    group.CommitChanges();

                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void CrearBaseDeDatos() {
            try {
                StringBuilder vSql = new StringBuilder();
                string valDataBaseName = $"{Helper.NombreBaseDeDatos(Producto)}_CL{Rif}";
                string valDataFile = valDataBaseName + ".mdf";
                valDataFile = System.IO.Path.Combine(DataDirectory, valDataFile);
                string valLogFile = valDataBaseName + ".ldf";
                valLogFile = System.IO.Path.Combine(LogDirectory, valLogFile);
                string vInitialDBSize = "4";
                var ConnectionString = ConfigurationManager.ConnectionStrings[ServidorBD].ConnectionString;
                vSql.AppendLine(" CREATE DATABASE [" + valDataBaseName + "] ");
                vSql.AppendLine($" ON     (NAME = '{valDataBaseName}FileData', FILENAME = '{valDataFile}' , SIZE = 4, FILEGROWTH = 10%) ");
                vSql.AppendLine($" LOG ON (NAME = '{valDataBaseName}Log',      FILENAME = '{valLogFile}'  , SIZE = 1, FILEGROWTH = 10%) ");
                vSql.AppendLine(" COLLATE Modern_Spanish_CI_AS ");

                using (SqlConnection conexion = new SqlConnection(ConnectionString)) {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand(vSql.ToString(), conexion)) {
                        command.ExecuteNonQuery();
                    }
                    vSql = new StringBuilder();
                    vSql.AppendLine($" ALTER DATABASE[{valDataBaseName}] SET ALLOW_SNAPSHOT_ISOLATION ON ");
                    using (SqlCommand command = new SqlCommand(vSql.ToString(), conexion)) {
                        command.ExecuteNonQuery();
                    }
                    vSql = new StringBuilder();
                    vSql.AppendLine($" ALTER DATABASE[{valDataBaseName}] SET READ_COMMITTED_SNAPSHOT ON ");
                    using (SqlCommand command = new SqlCommand(vSql.ToString(), conexion)) {
                        command.ExecuteNonQuery();
                    }
                }
                string[] valSchemas = Helper.SchemasByProgram(Producto);
                ConnectionString = Helper.ReplaceIgnoreCase(ConnectionString, "Master", valDataBaseName);
                using (SqlConnection conexion = new SqlConnection(ConnectionString)) {
                    conexion.Open();
                    foreach (string vSchema in valSchemas) {
                        vSql = new StringBuilder();
                        vSql.Append("CREATE SCHEMA [" + vSchema + "] AUTHORIZATION [dbo]");
                        using (SqlCommand command = new SqlCommand(vSql.ToString(), conexion)) {
                            command.ExecuteNonQuery();
                        }
                    }
                    vSql = new StringBuilder();
                    var table = "ParametrosMsdeManager";
                    if (Producto == "AXI" || Producto == "DER" || Producto == "NOM") {
                        table = "IG" + Producto + "_ParametrosMsdeManager";
                    }
                    vSql.Append($"CREATE TABLE[dbo].[{table}](");
                    vSql.Append("SecuencialInterno int CONSTRAINT nn" + table + "Secuencial NOT NULL, ");
                    vSql.Append("ReestructuracionAutomatica char(1) CONSTRAINT nn" + table + "Reestructu NOT NULL, ");
                    vSql.Append("TablasCreadas char(1) CONSTRAINT nn" + table + "TablasCrea NOT NULL, ");
                    vSql.Append("fldTimeStamp timestamp)");
                    using (SqlCommand command = new SqlCommand(vSql.ToString(), conexion)) {
                        command.ExecuteNonQuery();
                    }
                    vSql = new StringBuilder();
                    vSql.Append(" ALTER TABLE ");
                    vSql.Append(table);
                    vSql.Append("  add CONSTRAINT p_");
                    vSql.Append(table);
                    vSql.Append(" primary key (SecuencialInterno)");
                    using (SqlCommand command = new SqlCommand(vSql.ToString(), conexion)) {
                        command.ExecuteNonQuery();
                    }
                    vSql = new StringBuilder();
                    vSql.Append(" INSERT INTO ");
                    vSql.Append(table);
                    vSql.Append(" (SecuencialInterno, ReestructuracionAutomatica, TablasCreadas)");
                    vSql.Append(" VALUES (1, ");
                    vSql.Append("'S'");
                    vSql.Append(", 'N') ");
                    using (SqlCommand command = new SqlCommand(vSql.ToString(), conexion)) {
                        command.ExecuteNonQuery();
                    }
                }
                Datos_SQL.DataBase = valDataBaseName;
                CrearUsuarioSQL();
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void CrearUsuarioSQL() {
            try {
                string vSql;
                var ConnectionString = ConfigurationManager.ConnectionStrings[ServidorBD].ConnectionString;
                string valUserSql = $"UsrSql{Rif}_{Producto}";
                string valPasswordUserSql = Helper.GenerarPassword();
                Datos_SQL.Login = valUserSql;
                Datos_SQL.Password = valPasswordUserSql;

                using (SqlConnection conexion = new SqlConnection(ConnectionString)) {
                    conexion.Open();
                    vSql = $" EXEC sp_addlogin @loginame = '{valUserSql}', @passwd = '{valPasswordUserSql}' ";
                    using (SqlCommand command = new SqlCommand(vSql, conexion)) {
                        command.ExecuteNonQuery();
                    }
                }

                string valDataBaseName = $"{Helper.NombreBaseDeDatos(Producto)}_CL{Rif}";
                ConnectionString = Helper.ReplaceIgnoreCase(ConnectionString, "Master", valDataBaseName);
                using (SqlConnection conexion = new SqlConnection(ConnectionString)) {
                    conexion.Open();
                    vSql = $"CREATE USER [{valUserSql}] FOR LOGIN [{valUserSql}]";
                    using (SqlCommand command = new SqlCommand(vSql, conexion)) {
                        command.ExecuteNonQuery();
                    }
                    vSql = $" EXEC sp_addrolemember 'db_datareader', '{valUserSql}'";
                    using (SqlCommand command = new SqlCommand(vSql, conexion)) {
                        command.ExecuteNonQuery();
                    }
                    vSql = $" EXEC sp_addrolemember 'db_datawriter', '{valUserSql}'";
                    using (SqlCommand command = new SqlCommand(vSql, conexion)) {
                        command.ExecuteNonQuery();
                    }
                    vSql = $" EXEC sp_addrolemember 'db_owner', '{valUserSql}'";
                    using (SqlCommand command = new SqlCommand(vSql, conexion)) {
                        command.ExecuteNonQuery();
                    }
                    DataTable dataTable = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_OWNER = 'dbo'", conexion)) {
                        adapter.Fill(dataTable);
                    }
                    foreach (DataRow item in dataTable.Rows) {
                        vSql = $"GRANT DELETE ON SCHEMA::[{item["SCHEMA_NAME"]}] TO [{valUserSql}]";
                        using (SqlCommand command = new SqlCommand(vSql, conexion)) {
                            command.ExecuteNonQuery();
                        }
                        vSql = $"GRANT EXECUTE ON SCHEMA::[{item["SCHEMA_NAME"]}] TO [{valUserSql}]";
                        using (SqlCommand command = new SqlCommand(vSql, conexion)) {
                            command.ExecuteNonQuery();
                        }
                        vSql = $"GRANT INSERT ON SCHEMA::[{item["SCHEMA_NAME"]}] TO [{valUserSql}]";
                        using (SqlCommand command = new SqlCommand(vSql, conexion)) {
                            command.ExecuteNonQuery();
                        }
                        vSql = $"GRANT SELECT ON SCHEMA::[{item["SCHEMA_NAME"]}] TO [{valUserSql}]";
                        using (SqlCommand command = new SqlCommand(vSql, conexion)) {
                            command.ExecuteNonQuery();
                        }
                        vSql = $"GRANT UPDATE ON SCHEMA::[{item["SCHEMA_NAME"]}] TO [{valUserSql}]";
                        using (SqlCommand command = new SqlCommand(vSql, conexion)) {
                            command.ExecuteNonQuery();
                        }
                    }

                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void CrearTenant() {
            try {
                Tenant = Rif;
                Directory.CreateDirectory(RutaTenants + $@"\{Tenant}");
                DarPermisos();
                Directory.CreateDirectory(RutaTenants + $@"\{Tenant}\{Producto}");
                string vPathTenant = RutaTenants + $@"\{Tenant}\{Producto}";
                StreamWriter vFileWriter;
                string vFileName = System.IO.Path.Combine(vPathTenant, Helper.IniFileName(Producto) + ".INI");
                vFileWriter = new StreamWriter(vFileName, true, System.Text.Encoding.Default);
                var parameters = ConfigurationManager.ConnectionStrings[ServidorBD].ConnectionString.Split(';');
                foreach (string parameter in parameters) {
                    if (parameter.Trim().StartsWith("Data Source", StringComparison.OrdinalIgnoreCase)) {
                        Datos_SQL.Servidor = parameter.Split('=')[1].Trim().Replace("\\", "\\\\");
                    }
                }
                vFileWriter.WriteLine($"\"[SERVIDOR] = {Datos_SQL.Servidor}||{Datos_SQL.DataBase}\"");
                vFileWriter.WriteLine($"\"[UNIDADLOGICASERVIDOR] = {vPathTenant}\"");
                vFileWriter.WriteLine($"\"[FORMATODEFECHAENELSERVIDOR] = {FormatoDeFechaEnElServidor}\"");
                vFileWriter.WriteLine($"\"[ASKFORSAVEINFO] = No\"");
                vFileWriter.WriteLine($"\"[SQLUSERLOGIN] = {Datos_SQL.Login}\"");
                vFileWriter.WriteLine($"\"[SQLUSERPASSWORD] = {Datos_SQL.Password}\"");
                vFileWriter.WriteLine($"\"[SECURITYMODE] = SQL\"");
                vFileWriter.Close();
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void CrearPerfil() {
            try {
                for (int i = 0; i < UsuariosCreados.Count; i++) {
                    string script = $@"
                        $username = '{Dominio}\{UsuariosCreados[i].Username}';
                        $password = ConvertTo-SecureString '{UsuariosCreados[i].Password}' -AsPlainText -Force;
                        $credential = New-Object System.Management.Automation.PSCredential($username, $password);
                        Start-Process -FilePath ""C:\Windows\System32\cmd.exe"" -ArgumentList ""/c echo exit | runas /user:$username cmd"" -Credential $credential
                        ";
                    using (PowerShell ps = PowerShell.Create()) {
                        ps.AddScript(script);
                        ps.Invoke();
                    }
                    string downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    downloadsPath = Helper.ReplaceIgnoreCase(downloadsPath, Environment.UserName, UsuariosCreados[i].Username);
                    string vDir = System.IO.Path.Combine(downloadsPath, "Galac Software");
                    System.IO.Directory.CreateDirectory(vDir);
                    vDir = System.IO.Path.Combine(vDir, Helper.PublicFolderName(Producto));
                    System.IO.Directory.CreateDirectory(vDir);
                    string valPathAndFileName = System.IO.Path.Combine(vDir, "Tenant.txt");
                    StreamWriter vFileWriter;
                    vFileWriter = new StreamWriter(valPathAndFileName, true, System.Text.Encoding.Default);
                    vFileWriter.WriteLine(Tenant);
                    vFileWriter.Close();
                    System.IO.File.SetAttributes(valPathAndFileName, FileAttributes.Hidden | FileAttributes.ReadOnly);
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void DarPermisos() {
            try {
                string folderPath = RutaTenants + $@"\{Tenant}";
                string groupName = Grupo;
                SecurityIdentifier groupSID = null;
                using (var context = new PrincipalContext(ContextType.Domain, Dominio)) {
                    var group = GroupPrincipal.FindByIdentity(context, groupName);
                    if (group != null) {
                        groupSID = (SecurityIdentifier)group.Sid;
                    } else {
                        throw new Exception("Error: No se pudo encontrar el grupo en Active Directory.");
                    }
                }
                DirectorySecurity dirSecurity = Directory.GetAccessControl(folderPath);
                FileSystemAccessRule accessRule = new FileSystemAccessRule(
                    groupSID,
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow
                );
                dirSecurity.AddAccessRule(accessRule);
                Directory.SetAccessControl(folderPath, dirSecurity);
                QuitarPermisos();
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void QuitarPermisos() {
            string folderPath = RutaTenants + $@"\{Tenant}";
            try {
                string script = $@"
                    Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force
                    $folderPath = '{folderPath}'
                    $acl = Get-Acl -Path $folderPath
                    $acl.SetAccessRuleProtection($true, $true)                                      
                    Set-Acl -Path $folderPath -AclObject $acl                                        
                 ";
                using (PowerShell ps = PowerShell.Create()) {
                    ps.AddScript(script);
                    ps.Invoke();
                }
                script = $@"
                    Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force
                    $folderPath = '{folderPath}'
                    $acl = Get-Acl -Path $folderPath                    
                    $authenticatedUsersSID = [System.Security.Principal.SecurityIdentifier]::new([System.Security.Principal.WellKnownSidType]::AuthenticatedUserSid, $null)
                    $usersSID = [System.Security.Principal.SecurityIdentifier]::new([System.Security.Principal.WellKnownSidType]::BuiltinUsersSid, $null)
                    $rules = $acl.GetAccessRules($true, $true, [System.Security.Principal.SecurityIdentifier])
                    foreach ($rule in $rules) {{
                        if ($rule.IdentityReference -eq $authenticatedUsersSID) {{
                            $acl.RemoveAccessRuleSpecific($rule)
                        }} elseif ($rule.IdentityReference -eq $usersSID) {{
                            $acl.RemoveAccessRuleSpecific($rule)
                        }}
                    }}
                    Set-Acl -Path $folderPath -AclObject $acl
                 ";
                using (PowerShell ps = PowerShell.Create()) {
                    ps.AddScript(script);
                    ps.Invoke();
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void Finalizar() {
            StreamWriter vFileWriter = new StreamWriter("Resultado.txt", true, System.Text.Encoding.Default);
            vFileWriter.WriteLine($"*****************");
            vFileWriter.WriteLine($"OU en el AD          : {Rif}");
            vFileWriter.WriteLine($"Grupo Creado en el AD: {Grupo}");
            vFileWriter.WriteLine($"Usuarios Creados");
            foreach (var item in UsuariosCreados) {
                vFileWriter.WriteLine($"     {item.Username}   {item.Password}");
            }
            vFileWriter.WriteLine($"Base de Datos Creada");
            vFileWriter.WriteLine($"     Servidor     :{Datos_SQL.Servidor}");
            vFileWriter.WriteLine($"     Base de Datos:{Datos_SQL.DataBase}");
            vFileWriter.WriteLine($"     Login        :{Datos_SQL.Login}");
            vFileWriter.WriteLine($"     Password     :{Datos_SQL.Password}");
            vFileWriter.WriteLine($"Tenant Creado       :{Tenant}");
            vFileWriter.Close();
        }
        private void LLenarCombos() {
            foreach (ConnectionStringSettings servidor in ConfigurationManager.ConnectionStrings) {
                cmbServidores.Items.Add(servidor.Name);
            }
            cmbProducto.Items.Add("SAW");
            cmbProducto.Items.Add("IVA");
            cmbProducto.Items.Add("WCO");
            cmbProducto.Items.Add("NOM");
            cmbProducto.Items.Add("AXI");
            cmbProducto.Items.Add("DER");

            cmbProducto.SelectedIndex = 0;
            cmbProducto.Text = cmbProducto.SelectedItem.ToString();

            cmbServidores.SelectedIndex = 0;
            cmbServidores.Text = cmbServidores.SelectedItem.ToString();
        }
        private void ListarUsuario() {
            Cliente = txtCliente.Text;
            Rif = txtRif.Text;
            Producto = cmbProducto.Text;
            Grupo = $"RD_{Rif}_{Producto}";
            try {
                using (DirectoryEntry rootOU = new DirectoryEntry($"LDAP://OU=Usuarios, OU={Rif},{ClientesOU},{DominioOU}")) {
                    using (DirectorySearcher searcher = new DirectorySearcher(rootOU)) {
                        searcher.Filter = $"(objectCategory=user)";                        
                        searcher.SearchScope = SearchScope.OneLevel;
                        SearchResultCollection results = searcher.FindAll();
                        foreach (SearchResult result in results) {                            
                            if (result.Properties["sn"][0].ToString() == Producto) {
                                var user = new CuentaUSuario {
                                    sAMAccountName = result.Properties["sAMAccountName"][0].ToString(),
                                    userAccountControl = result.Properties["userAccountControl"][0].ToString() == "66080" ||
                                    result.Properties["userAccountControl"][0].ToString() == "512"
                                    ? "Activo" : "Desactivado"
                                };
                                UserListBox.Items.Add(user);
                                //UserListBox.Items.Add(result.Properties["sAMAccountName"][0].ToString());
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void Eliminar() {
            try {
                foreach (var selectedUser in UserListBox.SelectedItems) {
                    CuentaUSuario usuarioSeleccionado = selectedUser as CuentaUSuario;
                    string userDN = $"CN={usuarioSeleccionado.sAMAccountName},OU=Usuarios,OU={Rif},{ClientesOU},{DominioOU}";
                    MessageBox.Show(userDN);
                    using (DirectoryEntry userEntry = new DirectoryEntry($"LDAP://{userDN}")) {                        
                        userEntry.Properties["userAccountControl"].Value = 0x0202;
                        userEntry.CommitChanges();
                    }
                }
                UserListBox.Items.Clear(); 
            } catch (Exception ex) {
                throw ex; 
            }
        }


    }
}


  