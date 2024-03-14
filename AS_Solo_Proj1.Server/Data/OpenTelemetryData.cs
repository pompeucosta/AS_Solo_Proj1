using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AS_Solo_Proj1.Server.Data
{
    public static class OpenTelemetryData
    {
        private static Meter _meter = new Meter(MeterName);
        public static string ServiceName => "AS.Proj1";
        public static string ServiceVersion => "1.0";
        public static string MeterName => "LoginMetrics";

        public static readonly Counter<long> SuccessfulLoginsCounter = _meter.CreateCounter<long>("successful_logins");
        public static readonly Counter<long> FailedLoginsCounter = _meter.CreateCounter<long>("failed_logins");
        public static readonly ActivitySource MyActivitySource = new(ServiceName, ServiceVersion);
    }
}
