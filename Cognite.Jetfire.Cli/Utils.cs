using System;
using System.Threading;
using System.Threading.Tasks;
using Cognite.Jetfire.Api;

namespace Cognite.Jetfire.Cli
{
    public static class Utils
    {
        public static async Task<int> ResolveEitherId(int? id, string externalId, IJetfireClient jetfire)
        {
            if (id == null && externalId != null)
            {
                var transform = await jetfire.TransformConfigByExternalId(externalId, new CancellationToken());
                return transform.Id;
            }
            else if (id != null && externalId == null)
            {
                return id.Value;
            }
            else
            {
                throw new JetfireCliException("Either --id or --external-id must be specified");
            }
        }

        public static string FormatTimestamp(long? msTimestamp)
        {
            if (msTimestamp == null)
            {
                return "";
            }
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(msTimestamp.Value).ToLocalTime();
            return dtDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string FormatDuration(long? start, long? end)
        {
            if (start == null)
            {
                return "";
            }

            DateTime endTime;
            if (end == null)
            {
                endTime = DateTime.Now;
            }
            else
            {
                endTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                endTime = endTime.AddSeconds(end.Value).ToLocalTime();
            }
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            startTime = startTime.AddSeconds(start.Value).ToLocalTime();

            TimeSpan duration = endTime - startTime;
            return (int)duration.TotalHours + duration.ToString(@"\:mm\:ss");
        }
    }
}
