using System;

namespace MightyCalc.IntegrationTests.Tools
{
    public static class KnownConnectionStrings
    {
        private const int dbPort = 31045;
        private static readonly string DefaultReadModel = $"Host=localhost;Port={dbPort};Database=readmodel;User ID=postgres;";
        private static readonly string DefaultJournalModel = $"Host=localhost;Port={dbPort};Database=journal;User ID=postgres;";
        private static readonly string DefaultSnapshotStoreModel =
            $"Host=localhost;Port={dbPort};Database=snapshotstore;User ID=postgres;";

        public static string ReadModel =>
            Environment.GetEnvironmentVariable("MightyCalc_ReadModel") ?? DefaultReadModel;

        public static string Journal =>
            Environment.GetEnvironmentVariable("MightyCalc_Journal") ?? DefaultJournalModel;

        public static string SnapshotStore =>
            Environment.GetEnvironmentVariable("MightyCalc_SnapshotStore") ?? DefaultSnapshotStoreModel;
    }
}