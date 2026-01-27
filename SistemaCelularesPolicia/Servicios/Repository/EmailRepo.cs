using SistemaCelularesPolicia.Servicios.Interfaces;
using System.Net;
using System.Net.Mail;

namespace SistemaCelularesPolicia.Servicios.Repository
{
    public class EmailRepo : IEmail
    {
        private readonly IConfiguration _configuration;

        public EmailRepo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarCorreoVerificacion(string emailDestino, string nombreUsuario, string codigo)
        {
            // Configuración desde appsettings.json (Más adelante la agregas)
            string smtpServer = _configuration["EmailSettings:Server"];
            int port = int.Parse(_configuration["EmailSettings:Port"]);
            string senderEmail = _configuration["EmailSettings:SenderEmail"];
            string password = _configuration["EmailSettings:Password"];

            var message = new MailMessage(senderEmail, emailDestino)
            {
                Subject = "Verifica tu cuenta - Sistema Policial",
                IsBodyHtml = true,
                Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                        <h2 style='color: #044c36;'>Bienvenido, {nombreUsuario}</h2>
                        <p>Gracias por registrarte. Para activar tu cuenta, ingresa el siguiente código:</p>
                        <h1 style='background-color: #f3f4f6; padding: 10px; text-align: center; letter-spacing: 5px; color: #1e3a8a;'>{codigo}</h1>
                        <p style='color: #999; font-size: 12px;'>Este código expira en 10 minutos.</p>
                    </div>"
            };

            using (var client = new SmtpClient(smtpServer, port))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, password);
                await client.SendMailAsync(message);
            }
        }
    }
}
