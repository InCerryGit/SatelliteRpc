using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace SatelliteRpc.Shared;

/// <summary>
/// A static class that provides a method to create a method invoker.
/// </summary>
public static class MethodInvoker
{
    /// <summary>
    /// Creates a method invoker for a given type and method.
    /// </summary>
    /// <param name="type">The type of the object that contains the method to invoke.</param>
    /// <param name="methodInfo">The method to invoke.</param>
    /// <returns>A function that takes an object instance and an array of arguments, and returns the result of the method invocation.</returns>
    public static Func<object, object?[], object> CreateInvoker(Type type, MethodInfo methodInfo)
    {
        // Get the parameters of the method
        var parameters = methodInfo.GetParameters();

        // Create the parameters for the lambda expression
        var instance = Expression.Parameter(typeof(object), "instance");
        var arguments = Expression.Parameter(typeof(object[]), "arguments");

        // Create the method call expression
        var methodCall = Expression.Call(
            Expression.Convert(instance, type),
            methodInfo,
            CreateParameterExpressions(parameters, arguments));

        // Create the lambda expression based on the return type of the method
        LambdaExpression lambdaExpression;
        if (methodInfo.ReturnType == typeof(void))
        {
            var voidLambda = Expression.Lambda<Action<object, object[]>>(methodCall, instance, arguments);
            return (ins, args) =>
            {
                voidLambda.Compile().Invoke(ins, args!);
                return null!;
            };
        }

        if (methodInfo.ReturnType == typeof(Task))
        {
            lambdaExpression = Expression.Lambda<Func<object, object[], Task>>(methodCall, instance, arguments);
        }
        else if (methodInfo.ReturnType.IsGenericType &&
                 methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            lambdaExpression =
                Expression.Lambda(
                    typeof(Func<,,>).MakeGenericType(typeof(object), typeof(object[]), methodInfo.ReturnType),
                    methodCall, instance, arguments);
        }
        else
        {
            lambdaExpression = Expression.Lambda<Func<object, object[], object>>(methodCall, instance, arguments);
        }
        
        return (Func<object, object?[], object>)lambdaExpression.Compile();
    }

    /// <summary>
    /// A ConcurrentDictionary that caches compiled functions for extracting results from different types of Tasks.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Func<Task, object>> ResultExtractors = new();

    /// <summary>
    /// Gets the result of a Task.
    /// </summary>
    /// <param name="task">The Task to extract the result from.</param>
    /// <returns>The result of the Task.</returns>
    public static object GetTaskResult(Task task)
    {
        var taskType = task.GetType();
        if (!ResultExtractors.TryGetValue(taskType, out var extractor))
        {
            extractor = CreateResultExtractor(taskType);
            ResultExtractors.TryAdd(taskType, extractor);
        }

        return extractor(task);
    }

    /// <summary>
    /// Creates a function that can extract the result from a Task of a specific type.
    /// </summary>
    /// <param name="taskType">The type of the Task to create the extractor for.</param>
    /// <returns>A function that can extract the result from a Task of the provided type.</returns>
    private static Func<Task, object> CreateResultExtractor(Type taskType)
    {
        var parameter = Expression.Parameter(typeof(Task), "task");
        var cast = Expression.Convert(parameter, taskType);
        var property = Expression.Property(cast, "Result");
        var castResult = Expression.Convert(property, typeof(object));
        var lambda = Expression.Lambda<Func<Task, object>>(castResult, parameter);
        return lambda.Compile();
    }

    /// <summary>
    /// Creates an array of Expressions that represents the parameters for a method call.
    /// </summary>
    /// <param name="parameters">The parameters of the method.</param>
    /// <param name="arguments">The Expression that represents the arguments to the method.</param>
    /// <returns>An array of Expressions that represents the parameters for a method call.</returns>
    private static Expression[] CreateParameterExpressions(ParameterInfo[] parameters, Expression arguments)
    {
        var expressions = new Expression[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var argument = Expression.ArrayIndex(arguments, Expression.Constant(i));
            expressions[i] = Expression.Convert(argument, parameter.ParameterType);
        }

        return expressions;
    }
}