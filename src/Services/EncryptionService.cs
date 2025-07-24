using System.Security.Cryptography;
using System.Text;

namespace MicroJack.API.Services
{
    public class EncryptionService
    {
        private const string KeyFileName = "db.key";
        
        public static string GetOrCreateDatabaseKey()
        {
            var keyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, KeyFileName);
            
            if (File.Exists(keyPath))
            {
                return File.ReadAllText(keyPath);
            }
            
            // Generar nueva clave de 32 bytes (256 bits)
            using var rng = RandomNumberGenerator.Create();
            var keyBytes = new byte[32];
            rng.GetBytes(keyBytes);
            
            // Convertir a hex string
            var key = Convert.ToHexString(keyBytes);
            
            // Guardar la clave de forma segura
            File.WriteAllText(keyPath, key);
            
            // Establecer permisos restrictivos en el archivo de clave
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                // Solo el propietario puede leer/escribir (chmod 600)
                File.SetUnixFileMode(keyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
            
            return key;
        }
    }
}