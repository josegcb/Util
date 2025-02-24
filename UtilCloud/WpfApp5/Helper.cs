using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp5 {
    internal class Helper {

        public static string Error(Exception ex) {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(ex.Message);
            if (ex.InnerException != null) {
                stringBuilder.AppendLine(Error(ex.InnerException));
            }
            return stringBuilder.ToString();
        }
        public static string GenerarPassword() {
            int longitud = 9;
            string specialChars = "!@#$%.+-&*";
            string numbers = "0123456789";
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random random = new Random();
            char specialChar = specialChars[random.Next(specialChars.Length)];
            char number = numbers[random.Next(numbers.Length)];
            string restOfChars = new string(Enumerable.Repeat(letters, longitud - 2)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            string password = $"{specialChar}{number}{restOfChars}";
            password = new string(password.OrderBy(c => random.Next()).ToArray());
            if (ConfigurationManager.AppSettings["QA"] == "S") {
                return "1q2w3e4R*";
            }
            return password.Substring(0, longitud);
        }

        public static string NombreBaseDeDatos(string vProducto) {
            switch (vProducto) {
                case "SAW":
                return "SAWDB";
                case "IVA":
                return "IVADB";
                case "WCO":
                return "ContabilDB";
                case "NOM":
                return "NOMDB";
                case "DER":
                return "DERDB";
                case "AXI":
                return "AXIDB";
                default:
                return "SAWDB";
            }
        }
        public static string[] SchemasByProgram(string vProducto) {
            string vDepSaw = "Saw";
            if (vProducto.Equals("WCO", StringComparison.CurrentCultureIgnoreCase)) {
                return new string[] { "Comun", "Lib", "Emp", "Contab", "Balance" };
            } else if (vProducto.Equals("IVA", StringComparison.CurrentCultureIgnoreCase)) {
                return new string[] { "Comun", "Lib", "Emp", "Contab", "Iva", "Balance" };
            } else if (vProducto.Equals("SAW", StringComparison.CurrentCultureIgnoreCase)) {
                return new string[] { "Comun", "Lib", "Emp", "Contab", "Adm", "Balance", vDepSaw };
            } else if (vProducto.Equals("DER", StringComparison.CurrentCultureIgnoreCase)) {
                return new string[] { "Comun", "Lib", "Emp", "Der" };
            } else if (vProducto.Equals("NOM", StringComparison.CurrentCultureIgnoreCase)) {
                return new string[] { "Comun", "Lib", "Emp", "Nomina" };
            } else if (vProducto.Equals("AXI", StringComparison.CurrentCultureIgnoreCase)) {
                return new string[] { "Comun", "Lib", "Emp", "Axi", "Balance" };
            } else {
                return new string[] { "Lib" };
            }
        }
        public static string IniFileName(string vProducto) {
            string vresult = string.Empty;
            switch (vProducto) {
                case "SAW":
                vresult = "SAWDB";
                break;
                case "NOM":
                vresult = "NOMINA";
                break;
                case "AXI":
                vresult = "AXIDB";
                break;
                case "DER":
                vresult = "DERDB";
                break;
                case "IVA":
                vresult = "IVADB";
                break;
                case "WCO":
                vresult = "CONTABIL";
                break;
                default:
                break;
            }
            return vresult;
        }

        public static string PublicFolderName(string vProducto) {
            string vResult;
            switch (vProducto) {                
                case "SAW":
                vResult = "Saw";
                break;
                case "WCO":
                vResult = "Contabilidad";
                break;                
                break;
                case "IVA":
                vResult = "Iva";
                break;
                case "DER":
                vResult = "Der";
                break;                
                case "AXI":
                vResult = "Axi";
                break;
                case "NOM":
                vResult = "Nomina";
                break;               
                default:
                vResult = "AUN NO HA AGREGADO EL NOMBRE DE LA CARPETA EN LIBRERIA";
                break;
            }
            return vResult;
        }

        public static string ReplaceIgnoreCase(string input, string oldValue, string newValue) {
            return Regex.Replace(input, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase);
        }
    }

    public class UsuarioCreado {
        public UsuarioCreado(string username, string password) {
            Username = username;
            Password = password;
        }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class DatosSQL {
        public string Servidor { get; set; }
        public string DataBase { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

    }

    public class CuentaUSuario {
        public string sAMAccountName { get; set; }
        public string userAccountControl { get; set; }
    }
}


