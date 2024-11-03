using System;

namespace Ixnas.AltchaNet.Internal
{
    internal class Result
    {
        public bool Success { get; set; }
        public Error Error { get; set; }

        protected Result()
        {
        }

        public static Result Ok()
        {
            return new Result
            {
                Success = true,
                Error = Error.Create(ErrorCode.NoError)
            };
        }

        public static Result Fail(ErrorCode errorCode)
        {
            return new Result
            {
                Error = Error.Create(errorCode)
            };
        }
    }

    internal class Result<TValue> : Result
    {
        public TValue Value { get; set; }

        private Result()
        {
        }

        public static Result<TValue> Ok(TValue value)
        {
            return new Result<TValue>
            {
                Success = true,
                Value = value,
                Error = Error.Create(ErrorCode.NoError)
            };
        }

        public static Result<TValue> Fail()
        {
            return new Result<TValue>();
        }

        public new static Result<TValue> Fail(ErrorCode errorCode)
        {
            return new Result<TValue>
            {
                Error = Error.Create(errorCode)
            };
        }

        public static Result<TValue> Fail(Result result)
        {
            return new Result<TValue>
            {
                Error = result.Error
            };
        }

        public static Result<TValue> From<TInputValue>(Result<TInputValue> result,
                                                       Func<TInputValue, TValue> valueConverter)
        {
            return new Result<TValue>
            {
                Success = result.Success,
                Value = valueConverter(result.Value),
                Error = result.Error
            };
        }

        public static Result<TValue> From(Result result, TValue value)
        {
            return new Result<TValue>
            {
                Success = result.Success,
                Value = value,
                Error = result.Error
            };
        }
    }
}
