using Pulumi;

namespace StaticWebsite.Tests
{
    public static class OutputExtensions
    {
        public static T GetValue<T>(this Output<T> output)
        {
            T result=default;
            output.Apply(x => result = x);
            return result;
        }
        // public static Task<T> GetValueAsync<T>(this Output<T> output)
        // {
        //     var tcs = new TaskCompletionSource<T>();
        //     output.Apply(v => { tcs.SetResult(v); return v; });
        //     return tcs.Task;
        // }
    }
}