namespace MightyCalc.Reports.Streams
{
    public static class ProjectorActorProtocol
    {
        public class Start
        {
            private Start()
            {
            }

            public static readonly Start Instance = new Start();
        }

        public class Next
        {
            private Next()
            {
            }

            public static readonly Next Instance = new Next();
        }

        public class ProjectionDone
        {
            private ProjectionDone()
            {
            }

            public static readonly ProjectionDone Instance = new ProjectionDone();
        }
    }
}