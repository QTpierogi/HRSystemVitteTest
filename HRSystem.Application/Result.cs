using System;
using System.Collections.Generic;
using System.Text;

namespace HRSystem.Application
{
    public enum ResultErrorType
    {
        None,
        NotFound,
        Validation,
        Conflict
    }

    public class Result
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public ResultErrorType ErrorType { get; }

        protected Result(bool isSuccess, string? error, ResultErrorType errorType)
        {
            IsSuccess = isSuccess;
            Error = error;
            ErrorType = errorType;
        }

        public static Result Success() =>
            new(true, null, ResultErrorType.None);

        public static Result Failure(string error, ResultErrorType type = ResultErrorType.Validation) =>
            new(false, error, type);

        public static Result NotFound(string error) => Failure(error, ResultErrorType.NotFound);

        public static Result Conflict(string error) => Failure(error, ResultErrorType.Conflict);
    }

    public class Result<T> :Result
    {
        public T? Value { get; }

        protected Result(bool isSuccess, T? value, string? error, ResultErrorType errorType)
            : base(isSuccess, error, errorType)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(true, value, null, ResultErrorType.None);

        public static new Result<T> Failure(string error, ResultErrorType errorType = ResultErrorType.Validation) =>
            new(false, default, error, errorType);

        public static new Result<T> NotFound(string error) => Failure(error, ResultErrorType.NotFound);

        public static new Result<T> Conflict(string error) => Failure(error, ResultErrorType.Conflict);
    }
}
