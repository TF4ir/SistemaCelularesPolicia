using BCrypt.Net;

namespace SistemaCelularesPolicia.Recursos
{
    public class Utilidades
    {
        // 1. Encriptar (Hash)
        public static string EncriptarClave(string password)
        {
            // BCrypt genera el Salt automáticamente y lo incluye en el string resultante
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // 2. Verificar
        public static bool VerificarClave(string passwordIngresada, string hashGuardado)
        {
            // Verifica la contraseña contra el hash
            return BCrypt.Net.BCrypt.Verify(passwordIngresada, hashGuardado);
        }
    }
}