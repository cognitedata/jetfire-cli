using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using Cognite.Jetfire.Api.Model;

namespace Cognite.Jetfire.Cli.Show
{
    public class ShowCommand : IJetfireSubCommand
    {
        public Command Command { get; }
        private ISecretsProvider Secrets;

        public ShowCommand(ISecretsProvider secrets)
        {
            Secrets = secrets;

            Command = new Command("show", "Show detalis of a transformation")
            {
                new Option<int?>("--id")
                {
                    Description = "The id of the transformation to show. Either this or --external-id must be specified."
                },
                new Option<string>("--external-id")
                {
                    Description = "The externalId of the transformation to show. Either this or --id must be specified."
                }
            };

            Command.Handler = CommandHandler.Create<string, int?, string>(Handle);

        }

        async Task Handle(string cluster, int? id, string externalId)
        {
            TransformConfigRead transform;

            using (var client = JetfireClientFactory.CreateClient(Secrets, cluster))
            {

                if (id == null && externalId != null)
                {
                    transform = await client.TransformConfigByExternalId(externalId, new CancellationToken());
                }
                else if (id != null && externalId == null)
                {
                    transform = await client.TransformConfigById(id.Value, new CancellationToken());
                }
                else
                {
                    throw new Exception("Either --id or --external-id must be specified");
                }
            }

            Console.WriteLine($"Name:           {transform.Name}");
            Console.WriteLine($"ID:             {transform.Id}");
            Console.WriteLine($"External ID:    {transform.ExternalId}");

            Console.WriteLine();

            Console.WriteLine($"Destination:    {transform.Destination.Type}");
            if (transform.Destination.Type == "raw_table")
            {
                Console.WriteLine($"   Database:    {transform.Destination.Database}");
                Console.WriteLine($"   Table:       {transform.Destination.Table}");
                Console.WriteLine($"   Raw type:    {transform.Destination.RawType}");
            }
            Console.WriteLine($"Action:         {transform.ConflictMode}");
            Console.WriteLine($"Schedule:       {transform?.Schedule?.ToString()?.ToLower() ?? "no schedule"}");

            Console.WriteLine();
            Console.WriteLine("".PadLeft(50, '-'));
            Console.WriteLine();

            Console.WriteLine(transform.Query);
        }
    }
}
