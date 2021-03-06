﻿#define MAILKIT
using System;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using MailKit.Net.Imap;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using MailKit.Security;
using System.Threading;


#if STANDARD
using System.Net.Mail;
#endif


namespace Email
{
#if STANDARD
	//System.Net.Mail implementation of sending an email
	class Sender
	{
		const string emailAddres = "kleprlikjan@gmail.com";
		const string subject = "SUBJ";
		const string body = "SMS";

		MailMessage mail;
		SmtpClient Server;

		public void PrepareMail(string Reciever)
		{
			mail = new MailMessage();
			mail.From = new MailAddress(emailAddres);
			mail.To.Add(Reciever);
			mail.Subject = subject;
			mail.Body = body;


			Server = new SmtpClient("smtp.gmail.com");
			Server.Port = 587;
			Server.Credentials = new System.Net.NetworkCredential("timemanagementprojectmff@gmail.com", "nxritutmirxtoyev");
			Server.EnableSsl = true;
		}

		public void SendMail()
		{
			Server.Send(mail);
		}

	}
#endif

#if MAILKIT
	public class MailRepository
    {
        private readonly string mailServerRecieve, mailServerSend, login, password;
        private readonly int port;
        private readonly bool ssl;

        public MailRepository(string mailServerRecieve, string mailServerSend, int port, bool ssl, string login, string password)
        {
            this.mailServerRecieve = mailServerRecieve;
			this.mailServerSend = mailServerSend;
            this.port = port;
            this.ssl = ssl;
            this.login = login;
            this.password = password;
        }

        public void SendMessage(MimeMessage message)
        {
            using (var client = new SmtpClient(new ProtocolLogger("smtp.log")))
            {
                client.Connect(mailServerRecieve, port, ssl);

                client.Authenticate(login, password);

                client.Send(message);

                client.Disconnect(true);
            }
        }

		//NOT WORKING - clientSecrets are not valid!
		/*/
		public async System.Threading.Tasks.Task SendMessageSAFEAsync(MimeMessage message)
		{
			string GMailAccount = login;

			//NOT WORKING - clientSecrets are not valid!
			var clientSecrets = new ClientSecrets
			{	
				ClientId = "545779208103-augc39m8hhvqe4st3rrphl854p2qjmee.apps.googleusercontent.com",
				ClientSecret = "NGnYdo2rDWO2UBJrzizWQpjo"
			};

			var codeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
			{
				// Cache tokens in ~/.local/share/google-filedatastore/CredentialCacheFolder on Linux/Mac
				DataStore = new FileDataStore("CredentialCacheFolder", false),
				Scopes = new[] { "https://mail.google.com/" },
				ClientSecrets = clientSecrets
			});

			var codeReceiver = new LocalServerCodeReceiver();
			var authCode = new AuthorizationCodeInstalledApp(codeFlow, codeReceiver);
			var credential = await authCode.AuthorizeAsync(GMailAccount, CancellationToken.None);
			if (authCode.ShouldRequestAuthorizationCode(credential.Token))
			{
				await credential.RefreshTokenAsync(CancellationToken.None);
			}

			var oauth2 = new SaslMechanismOAuth2(credential.UserId, credential.Token.AccessToken);

			using (var client = new SmtpClient(new ProtocolLogger("smtp.log")))
			{
				await client.ConnectAsync(mailServerRecieve, port, SecureSocketOptions.SslOnConnect);
				await client.AuthenticateAsync(oauth2);
				client.Send(message);
				await client.DisconnectAsync(true);
			}

		}
		/**/
		public void DownloadMessages()
		{
			using (var client = new ImapClient(new ProtocolLogger("imap.log")))
			{
				client.Connect("imap.gmail.com", 993, MailKit.Security.SecureSocketOptions.SslOnConnect);

				client.Authenticate(login, password);

				client.Inbox.Open(FolderAccess.ReadWrite);

				var uids = client.Inbox.Search(MailKit.Search.SearchQuery.NotSeen);

				foreach (var uid in uids)
				{
					var message = client.Inbox.GetMessage(uid);
					client.Inbox.AddFlags(uid, MessageFlags.Seen, true);
					//Print text content of all the messages.
					Console.WriteLine("____________________________________________________");
					Console.Write(message.TextBody);
					Console.WriteLine("____________________________________________________");
				}

				client.Disconnect(true);
			}
		}

	}
#endif

    class mailingDEMO
	{
#if MAILKIT
		static MimeMessage CreateTestMessaage(string reciever)
		{
			var msg = new MimeMessage();

			msg.From.Add(new MailboxAddress("TestFrom", "timemanagementprojectmff@gmail.com"));
			msg.To.Add(new MailboxAddress("TestTo", reciever));
			msg.Subject = "Test mail message.";
			msg.Body = new TextPart("plain")
			{
				Text = @"Hey reciever,

This is default test message.

Sincerely,

-- Sender
"
			};

			return msg;
		}
#endif

		static void Main(string[] args)
		{

#if MAILKIT
			MailRepository repo = new MailRepository("smtp.gmail.com", "imap.gmail.com", 465, true, "timemanagementprojectmff@gmail.com", "nxritutmirxtoyev");

			repo.SendMessage(CreateTestMessaage("timemanagementprojectmff@gmail.com"));
			//repo.SendMessageSAFEAsync(CreateTestMessaage("timemanagementprojectmff@gmail.com"));
			
			Console.WriteLine("Default message sent.");
			Console.WriteLine("Press any key to display unread messages...");
			Console.ReadKey();
			Console.WriteLine("Loading unread messages...");

			repo.DownloadMessages();
#endif
#if STANDARD
			//e-mail demo
			Sender s = new Sender();
			s.PrepareMail("kleprlikovajana@seznam.cz");
			s.SendMail();
			Console.WriteLine("Email sent.");


			//SMS demo
			/*/
			s.PrepareMail("+420606739616@sms.cz.o2.com");
			s.SendMail();
			Console.WriteLine("SMS sent.");
			/**/
#endif


		}
	}
}
