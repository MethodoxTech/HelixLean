using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CommunityToolkit.Diagnostics
{
    public static class Guard
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsNotNull<T>(T? value, [CallerArgumentExpression("value")] string? name = null)
            where T : class
        {
            if (value is null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}

namespace CommunityToolkit.Diagnostics
{
    public static class ThrowHelper
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentOutOfRangeException<T>(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName);
        }
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentOutOfRangeException<T>(string paramName, T actualValue, string? message = null)
        {
            throw new ArgumentOutOfRangeException(paramName, actualValue, message);
        }
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentOutOfRangeException<T>(string paramName, T actualValue, T min, T max, string? message = null)
            where T : IComparable<T>
        {
            string msg = message ?? $"Value should be in range [{min}, {max}].";
            throw new ArgumentOutOfRangeException(paramName, actualValue, msg);
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException(string message)
        {
            throw new InvalidOperationException(message);
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentNullException(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentNullExceptionIfNull(object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}

namespace Microsoft.Extensions.Logging
{
    public interface ILogger
    {
        IDisposable BeginScope<TState>(TState state) where TState : notnull;
        bool IsEnabled(LogLevel logLevel);
        void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter);
    }

    public interface ILogger<out TCategoryName> : ILogger
    {
    }

    public interface ILoggerFactory : IDisposable
    {
        ILogger CreateLogger(string categoryName);
        void AddProvider(ILoggerProvider provider);
    }

    public interface ILoggerProvider : IDisposable
    {
        ILogger CreateLogger(string categoryName);
    }

    public readonly struct EventId : IEquatable<EventId>
    {
        public int Id { get; }
        public string? Name { get; }

        public EventId(int id, string? name = null)
        {
            Id = id;
            Name = name;
        }

        public bool Equals(EventId other) => Id == other.Id && Name == other.Name;
        public override bool Equals(object? obj) => obj is EventId other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Id, Name);
        public override string ToString() => Name is null ? Id.ToString() : $"{Name} ({Id})";

        public static implicit operator EventId(int id) => new(id);
    }

    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        None = 6
    }

    public static class LoggerExtensions
    {
        public static void LogError(this ILogger logger, string message)
        {
            if (logger is null) throw new ArgumentNullException(nameof(logger));

            logger.Log(
                LogLevel.Error,
                default,
                message,
                null,
                static (state, exception) => state);
        }

        public static void LogError(this ILogger logger, Exception? exception, string message)
        {
            if (logger is null) throw new ArgumentNullException(nameof(logger));

            logger.Log(
                LogLevel.Error,
                default,
                message,
                exception,
                static (state, ex) => state);
        }

        public static void LogInformation(this ILogger logger, string message)
        {
            if (logger is null) throw new ArgumentNullException(nameof(logger));

            logger.Log(
                LogLevel.Information,
                default,
                message,
                null,
                static (state, exception) => state);
        }

        public static void LogWarning(this ILogger logger, string message)
        {
            if (logger is null) throw new ArgumentNullException(nameof(logger));

            logger.Log(
                LogLevel.Warning,
                default,
                message,
                null,
                static (state, exception) => state);
        }

        public static void LogDebug(this ILogger logger, string message)
        {
            if (logger is null) throw new ArgumentNullException(nameof(logger));

            logger.Log(
                LogLevel.Debug,
                default,
                message,
                null,
                static (state, exception) => state);
        }
    }

    public sealed class Logger<T> : ILogger<T>
    {
        private readonly ILogger _inner;

        public Logger(ILoggerFactory factory)
        {
            if (factory is null) throw new ArgumentNullException(nameof(factory));
            _inner = factory.CreateLogger(GetCategoryName());
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => _inner.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _inner.Log(logLevel, eventId, state, exception, formatter);
        }

        private static string GetCategoryName()
        {
            var type = typeof(T);
            return type.FullName ?? type.Name;
        }
    }

    public static class LoggerFactoryExtensions
    {
        public static ILogger<T> CreateLogger<T>(this ILoggerFactory factory)
        {
            if (factory is null) throw new ArgumentNullException(nameof(factory));
            return new Logger<T>(factory);
        }
    }

    public sealed class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new();

        private NullLogger()
        {
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            private NullScope()
            {
            }

            public void Dispose()
            {
            }
        }
    }

    public sealed class NullLogger<T> : ILogger<T>
    {
        public static readonly NullLogger<T> Instance = new();

        private NullLogger()
        {
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullLogger.Instance.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }

    public sealed class NullLoggerFactory : ILoggerFactory
    {
        public static readonly NullLoggerFactory Instance = new();

        private NullLoggerFactory()
        {
        }

        public ILogger CreateLogger(string categoryName) => NullLogger.Instance;
        public void AddProvider(ILoggerProvider provider) { }
        public void Dispose() { }
    }
}