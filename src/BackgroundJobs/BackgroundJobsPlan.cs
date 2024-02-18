using System.Collections.Generic;

namespace Hazelnut
{
    public abstract class BackgroundJobsPlan
    {
        public static Dictionary<string, BackgroundJob> Jobs { get; } = new();

        public abstract void Initialize();

        public void Register(BackgroundJob job) => Jobs[job.Name.ToPascalCaseId()] = job;
    }
}