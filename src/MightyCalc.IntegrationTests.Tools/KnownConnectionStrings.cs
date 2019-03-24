using System;

namespace MightyCalc.IntegrationTests.Tools
{
    public static class KnownConnectionStrings
    {
        private const string DefaultReadModel = "Host=localhost;Port=5432;Database=readmodel;User ID=postgres;";
        private const string DefaultJournalModel = "Host=localhost;Port=5432;Database=journal;User ID=postgres;";

        private const string DefaultSnapshotStoreModel =
            "Host=localhost;Port=5432;Database=snapshotstore;User ID=postgres;";

        public static string ReadModel =>
            Environment.GetEnvironmentVariable("MightyCalc_ReadModel") ?? DefaultReadModel;

        public static string Journal =>
            Environment.GetEnvironmentVariable("MightyCalc_Journal") ?? DefaultJournalModel;

        public static string SnapshotStore =>
            Environment.GetEnvironmentVariable("MightyCalc_SnapshotStore") ?? DefaultSnapshotStoreModel;
    }
}