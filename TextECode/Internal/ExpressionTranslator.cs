using Antlr4.Runtime.Misc;
using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Internal.Expressions;
using OpenEpl.TextECode.Internal.ProgramElems.External;
using OpenEpl.TextECode.Utils.Scopes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenEpl.TextECode.Internal
{
    internal class ExpressionTranslator : EplParserBaseVisitor<TecExpression>
    {
        public readonly CodeTranslatorContext C;
        public TextECodeRestorer P => C.P;

        public ExpressionTranslator(CodeTranslatorContext c)
        {
            C = c ?? throw new ArgumentNullException(nameof(c));
        }

        public override TecExpression VisitParenthesizedExpression([NotNull] EplParser.ParenthesizedExpressionContext context)
        {
            return context.expression().Accept(this);
        }

        public override TecExpression VisitNumberLiteralExpression([NotNull] EplParser.NumberLiteralExpressionContext context)
        {
            return new TecNumberLiteral(C, double.Parse(context.GetText()));
        }

        public override TecExpression VisitUnaryOperatorExpression([NotNull] EplParser.UnaryOperatorExpressionContext context)
        {
            var operatorType = context.@operator.Type;
            var operandExpr = context.operand.Accept(this);
            return new TecUnaryOperatorExpression(C, operatorType, operandExpr);
        }

        public override TecExpression VisitStringLiteralExpression([NotNull] EplParser.StringLiteralExpressionContext context)
        {
            var x = context.GetText();
            x = x[1..^1];
            return new TecStringLiteral(C, x);
        }

        public override TecExpression VisitNormalCallExpression([NotNull] EplParser.NormalCallExpressionContext context)
        {
            var elem = (BaseCallableElem)C.Scope[ProgramElemName.Method(context.Identifier().GetText())];
            var args = (TecArgListExpression)context.arguments().Accept(this);
            var expr = new TecCallExpression(C, elem, null, args);
            return expr;
        }

        public override TecExpression VisitEnumExpression([NotNull] EplParser.EnumExpressionContext context)
        {
            var dataTypeElem = (BaseDataTypeElem)C.Scope[ProgramElemName.Type(context.dataType.Text)];
            var memberElem = (IMemberElem)dataTypeElem.ScopeFromOuter[ProgramElemName.Constant(context.name.Text)];
            return new TecEnumExpression(C, memberElem);
        }

        public override TecExpression VisitConstantExpression([NotNull] EplParser.ConstantExpressionContext context)
        {
            var elem = C.Scope[ProgramElemName.Constant(context.name.Text)];
            return new TecConstantExpression(C, elem);
        }

        public override TecExpression VisitArrayLiteralExpression([NotNull] EplParser.ArrayLiteralExpressionContext context)
        {
            var exprList = context.expressionList();
            var expressions = exprList != null
                ? exprList.expression().Select(x => x.Accept(this)).ToList()
                : null;
            return new TecArrayLiteralExpression(C, expressions);
        }

        public override TecExpression VisitMemberCallExpression([NotNull] EplParser.MemberCallExpressionContext context)
        {
            TecExpression targetExpr;
            var methodName = context.Identifier().GetText();
            if (context.target is EplParser.VariableExpressionContext vec)
            {
                var varName = vec.GetText();
                if (varName == "真")
                {
                    targetExpr = new TecBoolLiterall(C, true);
                }
                else if (varName == "假")
                {
                    targetExpr = new TecBoolLiterall(C, false);
                }
                else
                {
                    var varKey = ProgramElemName.Var(varName);
                    var existence = C.Scope.TryGetValue(ProgramElemName.Var(varName), out var targetVariableElem);
                    switch (existence)
                    {
                        case KeyExistence.Available:
                            targetExpr = targetVariableElem switch
                            {
                                BaseVariableElem variableElem => new TecVariableExpression(C, variableElem),
                                IMemberElem memberElem when memberElem.ImplicitTarget != null => new TecAccessMemberExpression(C, null, memberElem),
                                _ => throw new Exception($"{varKey} have a unsupported subtype")
                            };
                            break;
                        case KeyExistence.Ambiguous:
                            throw new AmbiguousKeyException($"{varKey} is ambiguous");
                        case KeyExistence.NotFound:
                            if (C.Scope.TryGetValue(ProgramElemName.Type(varName), out var targetDataType) == KeyExistence.Available)
                            {
                                var elem = (BaseCallableElem)
                                    ((BaseDataTypeElem)targetDataType).ScopeFromOuter[ProgramElemName.Method(methodName)];
                                var args = (TecArgListExpression)context.arguments().Accept(this);
                                var expr = new TecCallExpression(C, elem, null, args, true);
                                return expr;
                            }
                            else
                            {
                                throw new KeyNotFoundException($"{varKey} not found and failed to treat it as a data type qualifier");
                            }
                        default:
                            throw new Exception($"Unknown existence: {existence}");
                    }
                }
            }
            else
            {
                targetExpr = context.target.Accept(this);
            }
            {
                var elem = (BaseCallableElem)targetExpr.ResultDataType.ScopeFromOuter[ProgramElemName.Method(methodName)];
                var args = (TecArgListExpression)context.arguments().Accept(this);
                var expr = new TecCallExpression(C, elem, targetExpr, args);
                return expr;
            }
        }

        public override TecExpression VisitVariableExpression([NotNull] EplParser.VariableExpressionContext context)
        {
            var name = context.Identifier().GetText();
            if (name == "真")
            {
                return new TecBoolLiterall(C, true);
            }
            if (name == "假")
            {
                return new TecBoolLiterall(C, false);
            }
            var varKey = ProgramElemName.Var(name);
            return C.Scope[varKey] switch
            {
                BaseVariableElem variableElem => new TecVariableExpression(C, variableElem),
                IMemberElem memberElem when memberElem.ImplicitTarget != null => new TecAccessMemberExpression(C, null, memberElem),
                _ => throw new Exception($"{varKey} have a unsupported subtype")
            };
        }

        public override TecExpression VisitDateTimeLiteralExpression([NotNull] EplParser.DateTimeLiteralExpressionContext context)
        {
            var value = TokenUtils.ReadDateTimeLiteral(context.DateTimeLiteral().GetText());
            return new TecDateTimeLiteral(C, value);
        }

        public override TecExpression VisitBinaryOperatorExpression([NotNull] EplParser.BinaryOperatorExpressionContext context)
        {
            var operatorType = context.@operator.Type;
            var lhsExpr = context.lhs.Accept(this);
            var rhsExpr = context.rhs.Accept(this);
            return new TecBinaryOperatorExpression(C, operatorType, lhsExpr, rhsExpr);
        }

        public override TecExpression VisitAccessMemberExpression([NotNull] EplParser.AccessMemberExpressionContext context)
        {
            var targetExpr = context.target.Accept(this);
            var memberName = context.Identifier().GetText();
            var memberElem = (IMemberElem)targetExpr.ResultDataType.ScopeFromOuter[ProgramElemName.Var(memberName)];
            return new TecAccessMemberExpression(C, targetExpr, memberElem);
        }

        public override TecExpression VisitAccessArrayExpression([NotNull] EplParser.AccessArrayExpressionContext context)
        {
            var targetExpr = context.target.Accept(this);
            var indexExpr = context.index.Accept(this);
            return new TecAccessArrayExpression(C, targetExpr, indexExpr);
        }

        public override TecExpression VisitAddrOfExpression([NotNull] EplParser.AddrOfExpressionContext context)
        {
            var elem = (BaseCallableElem)C.Scope[ProgramElemName.Method(context.Identifier().GetText())];
            return new TecAddrOfExpression(C, elem);
        }

        public override TecExpression VisitArguments([NotNull] EplParser.ArgumentsContext context)
        {
            var args = context.expressionOrNothing();
            if (args.Length == 1 && args[0].expression() == null)
            {
                return new TecArgListExpression(C, Array.Empty<TecExpression>());
            }
            return new TecArgListExpression(C, args.Select(x => x.expression()?.Accept(this)).ToArray());
        }
    }
}
