using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace Cognite.Jetfire.Cli.Delete
{
    public class DeleteCommand : IJetfireSubCommand
    {
        public Command Command { get; }
        private ISecretsProvider secrets;

        public DeleteCommand(ISecretsProvider secrets)
        {
            this.secrets = secrets;

            Command = new Command("delete", "Delete a transformation")
            {
                new Option<int?>("--id")
                {
                    Description = "The id of the transformation to delete. Either this or --external-id must be specified."
                },
                new Option<string>("--external-id")
                {
                    Description = "The externalId of the transformation to delete. Either this or --id must be specified."
                },
            };

            Command.Handler = CommandHandler.Create<string, int?, string>(Handle);
        }

        async Task Handle(string cluster, int? id, string externalId)
        {
            if (id == null && externalId == null ||
                id != null && externalId != null)
            {
                throw new Exception("Either --id or --external-id must be specified");
            }

            using (var client = JetfireClientFactory.CreateClient(secrets, cluster))
            {
                // Resolve externalId if necessary
                if (id == null)
                {
                    var transform = await client.TransformConfigByExternalId(externalId, new CancellationToken());
                    id = transform.Id;
                }

                await client.TransformConfigDelete(id.Value);
            }
        }
    }
}
