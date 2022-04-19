﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Mailjet.Client;
using NewsletterStudio.Core.Models.System;
using NewsletterStudio.Core.Sending.Providers;
using NewsletterStudio.Plugins.Mailjet.Dtos;
using Newtonsoft.Json.Linq;

namespace NewsletterStudio.Plugins.Mailjet
{
    public class MailjetEmailServiceProvider : IEmailServiceProvider
    {

        public const string ProviderAlias = "mailjet"; 
        public string Alias => ProviderAlias;
        public string DisplayName => "Mailjet";
        public string SettingsView => "/App_Plugins/NewsletterStudio.Plugins.Mailjet/mailjet.settings.html";

        public Dictionary<string, object> Settings { get; set; }

        public SendOutConfiguration GetSendOutConfiguration()
        {
            return new SendOutConfiguration()
            {
                MaxItemsPerBatch = 25,
                SendBatchSize = 25
            };
        }

        public ValidationErrorCollection ValidateSettings(Dictionary<string, object> settings)
        {
            var configuration = new MailjetConfiguration(settings);

            var errors = new ValidationErrorCollection();

            if (string.IsNullOrEmpty(configuration.ApiKey))
                errors.Add(new ValidationError("mj_apiKey", "nsMailjet/Required","","required"));

            if(string.IsNullOrEmpty(configuration.ApiSecret))
                errors.Add(new ValidationError("mj_apiSecret", "nsMailjet/Required","","required"));

            return errors;

        }

        public void Send(List<SendEmailJob> batch)
        {
            var configuration = new MailjetConfiguration(Settings);
            
            if(batch.Count > 50)
                throw new ArgumentException("Can't send more than 50 emails per batch with Mailjet.");

            MailjetRequest request = new MailjetRequest
            {
                Resource = global::Mailjet.Client.Resources.Send.Resource
            };

            //TODO: Populate messages.
            var arr = new JArray();
            foreach (var message in batch)
            {
                var messageObject = MailjetMessageFromBatchJOb(message);
                arr.Add(messageObject);
            }
            
            //TODO: Would be nice if we could set the "CustomID" as well, could be useful.
            //      I also think that using the "Workspace key" might be better since when/if
            //      A Campaign/Transactional is removed we don't know what workspace to look for the recipient in.
            request.Property(global::Mailjet.Client.Resources.Send.Messages, arr);
            

            if (configuration.SandboxMode)
            {
                request.Property("SandboxMode", true);
            }

            // This will disable open and click-tracking from Mailjet
            request.Property("Globals", new JObject {
                    {"TrackOpens", "disabled"},
                    {"TrackClicks", "disabled"}
                }
            );

                
            MailjetClient client = new MailjetClient(configuration.ApiKey, configuration.ApiSecret)
            {
                Version = ApiVersion.V3_1
            };
            
            MailjetResponse response = Task.Run(() => client.PostAsync(request)).Result;

            if (response.IsSuccessStatusCode)
            {
                var data = response.GetData(); 

                for (int i = 0; i < batch.Count; i++)
                {
                    var mailJetResult = data[i].ToObject<MailJetMessageResponse>();

                    if(mailJetResult == null)
                        throw new ArgumentException("No MailJet-result for message");

                    batch[i].Successful = mailJetResult.Successful;

                    // Storing the "MessageId" as "ExternalId" for bounce reporting
                    batch[i].ExternalId = mailJetResult.To.First().MessageId.ToString(); //TODO: Make sure this is the best to store.

                    //TODO: Somewhere here we need to set the "Message Id".

                    if (mailJetResult.Successful == false)
                    {
                        batch[i].ErrorMessage = string.Join(", ", mailJetResult.Errors.Select(x => x.ErrorMessage));
                    }
                
                }
            }
            else
            {
                // API returned an error, setting all items in batch to error.

                var error = response.GetData()[0].ToObject<MailJetMessageError>();

                for (int i = 0; i < batch.Count; i++)
                {
                    batch[i].Successful = false;
                    batch[i].ErrorMessage = $"{error.ErrorMessage} ErrorCode: {error.ErrorCode}, ErrorRelatedTo: {string.Join(",",error.ErrorRelatedTo)}.";
                }


            }
            
            
            

        }

        private JToken MailjetMessageFromBatchJOb(SendEmailJob batchItem)
        {
            
            var message = new JObject
            {
                {
                    "From", new JObject
                    {
                        {"Email", batchItem.Message.From.Address},
                        {"Name", batchItem.Message.From.DisplayName}
                    }
                },
                {"Subject",batchItem.Message.Subject},
                //{"TextPart", "Dear passenger 1, welcome to Mailjet! May the delivery force be with you!"},
                {"HTMLPart", batchItem.Message.Body}
            };
            message.Add("CustomID",batchItem.WorkspaceKey.ToString());
            message.Add("To",RecipientsToJArray(batchItem.Message.To));

            if (batchItem.Message.CC.Any())
            {
                message.Add("Cc", RecipientsToJArray(batchItem.Message.CC));
            }
            
            if (batchItem.Message.Bcc.Any())
            {
                message.Add("Bcc", RecipientsToJArray(batchItem.Message.Bcc));
            }

            return message;

        }

        private JArray RecipientsToJArray(MailAddressCollection collection)
        {
            var arr = new JArray();

            foreach (var email in collection)
            {
                arr.Add( new JObject {
                            {"Email", email.Address},
                            {"Name", email.DisplayName}
                        });
            }

            return arr;
        }

        public CommandResult Send(MailMessage message)
        {

            var fakeBulk = new List<SendEmailJob>();
            fakeBulk.Add(new SendEmailJob()
            {
                Message = message
            });

            Send(fakeBulk);

            var res = fakeBulk.First();

            if(res.Successful)
                return CommandResult.Successful();

            return CommandResult.Error(new ValidationError("","",res.ErrorMessage));

        }
        
    }

    public class MailjetConfiguration
    {
        private readonly Dictionary<string, object> _settings;

        public MailjetConfiguration(Dictionary<string,object> settings)
        {
            if (settings == null)
                settings = new Dictionary<string, object>();

            _settings = settings;
        }

        public string ApiKey
        {
            get
            {
                if (_settings.ContainsKey("mj_apiKey"))
                    return _settings["mj_apiKey"].ToString();

                return null;
            }
        }

        public string ApiSecret
        {
            get
            {
                if (_settings.ContainsKey("mj_apiSecret"))
                    return _settings["mj_apiSecret"].ToString();

                return null;
            }
        }

        public bool SandboxMode
        {
            get
            {
                if (_settings.ContainsKey("mj_sandboxMode"))
                    return _settings["mj_sandboxMode"].ToString().Equals("true");

                return false;
            }
        }
        
    }
}
