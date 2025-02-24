using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1 {
    internal class Program {
        static void Main(string[] args) {

            Console.WriteLine(GeneratePassword());
            Console.ReadLine();



            //string folderPath = @"C:\Galac Cloud\Tenants\Josegcb1\SAW";
            //string domain = "community.Local";
            //string groupName = "RD_Josegcb1_140933409_SAW";

            //// Obtener el SID del grupo desde Active Directory
            //SecurityIdentifier groupSID = null;
            //using (var context = new PrincipalContext(ContextType.Domain, domain)) {
            //    var group = GroupPrincipal.FindByIdentity(context, groupName);
            //    if (group != null) {
            //        groupSID = (SecurityIdentifier)group.Sid;
            //    } else {
            //        Console.WriteLine("Error: No se pudo encontrar el grupo en Active Directory.");
            //       Console.ReadLine();
            //    }
            //}
            //DirectorySecurity dirSecurity = Directory.GetAccessControl(folderPath);

            //// Crear una nueva regla de acceso para el grupo
            //FileSystemAccessRule accessRule = new FileSystemAccessRule(
            //    groupSID,  // SID del grupo
            //    FileSystemRights.FullControl,  // Permisos (FullControl, Modify, Read, etc.)
            //    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,  // Herencia
            //    PropagationFlags.None,  // Propagación
            //    AccessControlType.Allow  // Tipo de acceso (Allow o Deny)
            //);

            //// Añadir la regla de acceso a la ACL
            //dirSecurity.AddAccessRule(accessRule);

            //// Aplicar la nueva ACL a la carpeta
            //Directory.SetAccessControl(folderPath, dirSecurity);

            //Console.WriteLine("Permisos otorgados correctamente.");



            //var s = "Usr_Prueba03_SAW_0";
            //string downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            ////downloadsPath = downloadsPath.Replace(Environment.UserName, UsuariosCreados[0].Username);
            //downloadsPath = downloadsPath.Replace(Environment.UserName, s);
            //Console.WriteLine(downloadsPath);


            ////string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            //downloadsPath = downloadsPath.Replace(Environment.UserName, "APi2");
            //Console.WriteLine(downloadsPath);
            //Console.WriteLine(DownLoadPath());
            //downloadsPath.Replace(Environment.UserName, "APi2");
            //Console.ReadLine();

            //decimal[] A = { 13, 10, 9, 6, 3, 4, 10, 5, 5, 6, 8, 9, 9, 3, 4, 10, 12, 8, 11, 4, 9, 10, 11, 4, 5, 9, 8, 8, 6, 8, 5, 11, 10, 6, 13, 1, 6, 7, 6, 6 };
            //decimal[] B = { 13, 6, 9, 0, 6, 13, 3, 0, 11, 0, 12, 11, 11, 8, 0, 8, 5, 10, 8, 7, 7, 7, 11, 8, 11, 5, 4, 4, 7, 7, 3, 1, 8, 8, 3, 8, 6, 8, 4, 8, 3, 3, 3 };
            //for (int i = 0; i < 40; i++) {
            //    bool existeY = false;
            //    for (int j = 0; j < 43; j++) {
            //        var a = A[i];
            //        var b = B[j];
            //        if (a > b) {
            //            existeY = true;
            //            break;
            //        }
            //    }
            //    if (!existeY) {
            //        Console.WriteLine("False");
            //    }
            //}
            //Console.WriteLine("True");
            Console.ReadLine();
        }

        private static Guid FolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);
        public static string DownLoadPath() {
            string vResult = string.Empty;
            IntPtr path;
            if (SHGetKnownFolderPath(ref FolderDownloads, 0, IntPtr.Zero, out path) == 0) {
                string downloadsPath = System.Runtime.InteropServices.Marshal.PtrToStringUni(path);
                vResult = downloadsPath;
            }
            return vResult;
        }


        static string GeneratePassword() {
            string specialChars = "!@#$%.+-&*";
            string numbers = "0123456789";
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random random = new Random();
            char specialChar = specialChars[random.Next(specialChars.Length)];
            char number = numbers[random.Next(numbers.Length)];
            string restOfChars = new string(Enumerable.Repeat(letters, 7)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            string password = $"{specialChar}{number}{restOfChars}";
            password = new string(password.OrderBy(c => random.Next()).ToArray());
            return password.Substring(0, 9);
        }
    }
}
