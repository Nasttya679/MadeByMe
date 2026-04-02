namespace MadeByMe.Application.Common
{
    using System;

    // Варіант 1: Для методів, які нічого не повертають
    public readonly struct Result
    {
        private Result(bool isSuccess, string errorMessage)
        {
            this.IsSuccess = isSuccess;
            this.ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !this.IsSuccess;

        public string ErrorMessage { get; }

        public static implicit operator Result(string errorMessage) => Failure(errorMessage);

        public static Result Success() => new Result(true, string.Empty);

        public static Result Failure(string errorMessage) => new Result(false, errorMessage);
    }

    // Варіант 2: Для методів, які повертають дані
    public readonly struct Result<T>
    {
        private readonly T? innerValue;

        private Result(bool isSuccess, string errorMessage, T? value)
        {
            this.IsSuccess = isSuccess;
            this.ErrorMessage = errorMessage;
            this.innerValue = value;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !this.IsSuccess;

        public string ErrorMessage { get; }

        public T Value => this.IsSuccess ? this.innerValue! : throw new InvalidOperationException("Неможливо отримати значення: операція завершилася помилкою.");

        public static implicit operator Result<T>(T value) => Success(value);

        public static implicit operator Result<T>(string errorMessage) => Failure(errorMessage);

        public static Result<T> Success(T value) => new Result<T>(true, string.Empty, value);

        public static Result<T> Failure(string errorMessage) => new Result<T>(false, errorMessage, default);
    }
}