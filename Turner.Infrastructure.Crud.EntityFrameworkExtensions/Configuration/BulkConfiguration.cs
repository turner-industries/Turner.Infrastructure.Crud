using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Exceptions;
using Z.BulkOperations;

// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.EntityFrameworkExtensions.Configuration
{
    public interface IBulkConfiguration
    {
        BulkOperation<TOperationEntity> Apply<TOperationEntity>(IRequestConfig config, BulkOperation<TOperationEntity> operation)
            where TOperationEntity : class;
    }

    public interface IBulkConfiguration<TEntity> : IBulkConfiguration
        where TEntity : class
    {
    }

    public abstract class BulkConfiguration<TEntity, TConfiguration> : IBulkConfiguration<TEntity>
        where TEntity : class
        where TConfiguration : BulkConfiguration<TEntity, TConfiguration>
    {
        protected Expression<Func<TEntity, object>> KeyColumnMapping { get; set; }

        protected Expression<Func<TEntity, object>> OutputExpression { get; set; }
        
        protected List<Expression<Func<TEntity, object>>> ColumnMappings { get; }
            = new List<Expression<Func<TEntity, object>>>();
        
        protected readonly HashSet<MemberInfo> IgnoredColumns = new HashSet<MemberInfo>();

        protected int? BatchSize { get; set; }

        protected int? BatchTimeout { get; set; }
        
        public virtual BulkOperation<TOperationEntity> Apply<TOperationEntity>(
            IRequestConfig config, 
            BulkOperation<TOperationEntity> operation)
            where TOperationEntity : class
        {
            var keyColumn = operation.ColumnMappings.Find(x => x.IsPrimaryKey);

            if (KeyColumnMapping != null)
            {
                if (keyColumn != null)
                    operation.ColumnMappings.Remove(keyColumn);
            }
            else
            {
                KeyColumnMapping = keyColumn != null
                    ? UpcastExpression(keyColumn.SourceExpression)
                    : GetDefaultKeyMapping(config);
            }

            if (keyColumn == null)
            {
                if (KeyColumnMapping == null)
                {
                    var message = $"No key column has been declared for bulk request '{config.RequestType}' " +
                                  $"and entity '{typeof(TOperationEntity)}'.";

                    throw new BadConfigurationException(message);
                }

                operation.ColumnMappings.Add(DowncastExpression<TOperationEntity>(KeyColumnMapping), true);
            }

            if (ColumnMappings.Count == 0)
                AddDefaultColumnMappings(operation.ColumnMappings);

            foreach (var mapping in ColumnMappings)
                operation.ColumnMappings.Add(DowncastExpression<TOperationEntity>(mapping), false);

            if (OutputExpression == null)
            {
                OutputExpression = KeyColumnMapping;

                if (operation.ColumnOutputExpression == null)
                    operation.ColumnOutputExpression = DowncastExpression<TOperationEntity>(OutputExpression);
            }

            if (BatchSize.HasValue)
                operation.BatchSize = BatchSize.Value;

            if (BatchTimeout.HasValue)
                operation.BatchTimeout = BatchTimeout.Value;

            return operation;
        }

        public TConfiguration WithPrimaryKey(Expression<Func<TEntity, object>> expression)
        {
            KeyColumnMapping = expression;

            return (TConfiguration)this;
        }

        public TConfiguration WithOutput(Expression<Func<TEntity, object>> outputExpression)
        {
            OutputExpression = outputExpression;

            return (TConfiguration)this;
        }

        public TConfiguration WithColumns(params Expression<Func<TEntity, object>>[] members)
        {
            AddColumns(members);

            return (TConfiguration)this;
        }

        public TConfiguration WithBatchSize(int size)
        {
            BatchSize = size;

            return (TConfiguration)this;
        }

        public TConfiguration WithBatchTimeout(int timeoutSeconds)
        {
            BatchTimeout = timeoutSeconds;

            return (TConfiguration)this;
        }

        protected Expression<Func<TOperationEntity, object>> CreateNewExpression<TOperationEntity>(HashSet<MemberInfo> members)
            where TOperationEntity : class
        {
            var eParamExpr = Expression.Parameter(typeof(TOperationEntity));
            var memberGetters = new Expression[members.Count];
            var memberInfo = new MemberInfo[members.Count];
            var ctorParams = new ParameterMirage[members.Count];

            var index = 0;

            foreach (var member in members)
            {
                if (!(typeof(TOperationEntity).GetMember(member.Name)[0] is PropertyInfo info))
                    throw new ArgumentException($"Member '{member}' is not a property on '{typeof(TOperationEntity)}'", nameof(members));

                memberGetters[index] = Expression.Property(eParamExpr, member.Name);
                memberInfo[index] = new PropertyMirageInfo<TOperationEntity>(info);
                ctorParams[index] = new ParameterMirage { Name = info.Name, Type = info.PropertyType };

                index++;
            }

            var fakeCtor = new ConstructorMirageInfo<TOperationEntity>(ctorParams);
            var newBody = Expression.New(fakeCtor, memberGetters, memberInfo);

            return Expression.Lambda<Func<TOperationEntity, object>>(newBody, eParamExpr);
        }

        protected Expression<Func<TOperationEntity, object>> MakeColumnExpression<TOperationEntity>(MemberInfo member)
            where TOperationEntity : class
        {
            var paramExpr = Expression.Parameter(typeof(TOperationEntity));
            var memberExpr = Expression.MakeMemberAccess(paramExpr, member);
            var convertExpr = Expression.Convert(memberExpr, typeof(object));

            return Expression.Lambda<Func<TOperationEntity, object>>(convertExpr, paramExpr);
        }

        protected Expression<Func<TEntity, object>> GetDefaultKeyMapping(IRequestConfig config)
        {
            var entityKey = config.GetKeyFor<TEntity>();

            if (entityKey != null && entityKey.KeyExpression.Body is MemberExpression memberExpression)
                return MakeColumnExpression<TEntity>(memberExpression.Member);

            return null;
        }

        protected void AddDefaultColumnMappings<TOperationEntity>(
            List<ColumnMapping<TOperationEntity>> mappings)
            where TOperationEntity : class
        {
            var ignoredProperties = IgnoredColumns.Select(x => x.Name).ToList();

            ignoredProperties.AddRange(mappings
                .Where(x => !x.IsPrimaryKey)
                .Select(x =>
                {
                    var memberExpression = (x.SourceExpression.Body is UnaryExpression unaryExpression)
                        ? unaryExpression.Operand as MemberExpression
                        : x.SourceExpression.Body as MemberExpression;

                    return memberExpression?.Member.Name;
                })
                .Where(x => !string.IsNullOrEmpty(x)));

            var columns = typeof(TEntity)
                .GetProperties()
                .Where(x => !ignoredProperties.Contains(x.Name))
                .ToList();

            if (KeyColumnMapping != null)
            {
                var keyExpression = KeyColumnMapping.Body is UnaryExpression unaryExpression
                    ? unaryExpression.Operand as MemberExpression
                    : KeyColumnMapping.Body as MemberExpression;

                // ReSharper disable once PossibleNullReferenceException
                columns.RemoveAll(x => x.Name == keyExpression.Member.Name);
            }

            ColumnMappings.AddRange(columns.Select(MakeColumnExpression<TEntity>));
        }

        protected void AddColumns(params Expression<Func<TEntity, object>>[] members)
        {
            foreach (var member in members)
            {
                if (member.Body is NewExpression newExpression)
                {
                    foreach (var arg in newExpression.Arguments)
                    {
                        ColumnMappings.Add(Expression.Lambda<Func<TEntity, object>>(
                            Expression.Convert(arg, typeof(object)),
                            member.Parameters));
                    }
                }
                else
                {
                    var memberExpression = (member.Body is UnaryExpression unaryExpression)
                        ? unaryExpression.Operand as MemberExpression
                        : member.Body as MemberExpression;

                    if (memberExpression == null)
                        throw new ArgumentException($"Invalid expression in parameter list: '{member}'", nameof(members));
                    
                    ColumnMappings.Add(MakeColumnExpression<TEntity>(memberExpression.Member));
                }
            }
        }

        protected IEnumerable<MemberInfo> GetMembers(Expression<Func<TEntity, object>> members)
        {
            if (members.Body is NewExpression newExpression)
            {
                return newExpression.Members;
            }
            else
            {
                var memberExpression = members.Body is UnaryExpression unaryExpression
                    ? unaryExpression.Operand as MemberExpression
                    : members.Body as MemberExpression;

                if (memberExpression == null)
                    throw new ArgumentException($"Invalid expression: '{members}'", nameof(members));

                return new[] { memberExpression.Member };
            }
        }

        protected Expression<Func<TEntity, object>> UpcastExpression<TOperationEntity>(
            Expression<Func<TOperationEntity, object>> expression)
            where TOperationEntity : class
        {
            if (expression.Body is NewExpression newExpression)
            {
                return CreateNewExpression<TEntity>(new HashSet<MemberInfo>(newExpression.Members));
            }

            var memberExpression = (expression.Body is UnaryExpression unaryExpression)
                ? unaryExpression.Operand as MemberExpression
                : expression.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException($"Invalid expression: '{expression}'", nameof(expression));

            return MakeColumnExpression<TEntity>(memberExpression.Member);
        }

        protected Expression<Func<TOperationEntity, object>> DowncastExpression<TOperationEntity>(
            Expression<Func<TEntity, object>> expression)
            where TOperationEntity : class
        {
            if (expression.Body is NewExpression newExpression)
            {
                return CreateNewExpression<TOperationEntity>(new HashSet<MemberInfo>(newExpression.Members));
            }

            var memberExpression = (expression.Body is UnaryExpression unaryExpression)
                ? unaryExpression.Operand as MemberExpression
                : expression.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException($"Invalid expression: '{expression}'", nameof(expression));

            return MakeColumnExpression<TOperationEntity>(memberExpression.Member);
        }
    }
}
