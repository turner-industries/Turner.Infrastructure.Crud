using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud
{
    internal static class ExpressionExtensions
    {
        internal static Expression ReplaceParameter(this Expression expression, ParameterExpression source, Expression target)
        {
            var replacer = new ParameterReplacer(source, target);

            return replacer.Visit(expression);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            public ParameterExpression Source { get; }

            public Expression Target { get; }

            public ParameterReplacer(ParameterExpression source, Expression target)
            {
                Source = source;
                Target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => node == Source ? Target : base.VisitParameter(node);
        }
    }
}
