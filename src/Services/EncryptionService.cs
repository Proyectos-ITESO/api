using System.Security.Cryptography;
using System.Text;

namespace MicroJack.API.Services
{
    public class EncryptionService
    {
        private const string KeyFileName = "db.key";
        
        public static string GetOrCreateDatabaseKey()
        {
            // Use a writable directory instead of the application base directory
            var dataDirectory = GetDataDirectory();
            var keyPath = Path.Combine(dataDirectory, KeyFileName);
            
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
        
        private static string GetDataDirectory()
        {
            // Check for environment variable first (for containerized/packaged apps)
            var dataDir = Environment.GetEnvironmentVariable("MICROJACK_DATA_DIR");
            if (!string.IsNullOrEmpty(dataDir) && Directory.Exists(dataDir))
            {
                return dataDir;
            }
            
            // Use current working directory if writable, otherwise use user data directory
            var currentDir = Directory.GetCurrentDirectory();
            try
            {
                var testFile = Path.Combine(currentDir, ".write_test");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return currentDir;
            }
            catch
            {
                // Current directory is not writable, use user data directory
                var userDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appDataDir = Path.Combine(userDataDir, "MicroJack");
                Directory.CreateDirectory(appDataDir);
                return appDataDir;
            }
        }
    }
}