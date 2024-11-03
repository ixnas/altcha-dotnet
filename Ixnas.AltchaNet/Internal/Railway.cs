using System;
using System.Threading.Tasks;

namespace Ixnas.AltchaNet.Internal
{
    internal class Railway<TResult>
    {
        public Result<TResult> Result { get; }

        public Railway(Result<TResult> result)
        {
            Result = result;
        }

        public Railway<TNextResult> Then<TNextResult>(Func<TResult, Result<TNextResult>> func)
        {
            if (!Result.Success)
                return new Railway<TNextResult>(Result<TNextResult>.Fail(Result.Error.Code));
            var nextResult = func(Result.Value);
            return new Railway<TNextResult>(nextResult);
        }

        public async Task<Railway<TNextResult>> ThenAsync<TNextResult>(
            Func<TResult, Task<Result<TNextResult>>> func)
        {
            if (!Result.Success)
                return new Railway<TNextResult>(Result<TNextResult>.Fail(Result.Error.Code));
            var nextResult = await func(Result.Value);
            return new Railway<TNextResult>(nextResult);
        }
    }
}
