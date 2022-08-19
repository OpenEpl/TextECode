using Antlr4.Runtime.Misc;
using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.Expressions;
using QIQI.EProjectFile.Expressions;
using QIQI.EProjectFile.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal
{
    internal class StatementTranslator : EplParserBaseVisitor<Statement>
    {
        public readonly CodeTranslatorContext C;
        public TextECodeRestorer P => C.P;

        public StatementTranslator(CodeTranslatorContext c)
        {
            C = c ?? throw new ArgumentNullException(nameof(c));
        }

        public override Statement VisitCounterStatement([NotNull] EplParser.CounterStatementContext context)
        {
            var stat = new CounterStatement()
            {
                CommentOnEnd = context.commentOnEnd?.Text,
                Block = C.TranslateStatementBlock(context.statementBlock())
            };
            try
            {
                var args = (TecArgListExpression)context.argumentsOnStart.Accept(C.ExprTrans);
                stat.Count = args.GetOrDefault(0)?.ToNative();
                stat.Var = args.GetOrDefault(1)?.ToNative();
                stat.CommentOnStart = context.commentOnStart?.Text;
            }
            catch (Exception)
            {
                var sb = new StringBuilder();
                sb.Append("计次循环首 ");
                sb.Append(TokenUtils.GetRawText(context.argumentsOnStart));
                if (context.commentOnStart != null)
                {
                    sb.Append("  ' ");
                    sb.Append(context.commentOnStart.Text);
                }
                stat.UnexaminedCode = sb.ToString();
            }
            return stat;
        }

        public override Statement VisitDoWhileStatement([NotNull] EplParser.DoWhileStatementContext context)
        {
            var args = (TecArgListExpression)context.argumentsOnEnd.Accept(C.ExprTrans);
            var stat = new DoWhileStatement()
            {
                CommentOnStart = context.commentOnStart?.Text,
                Block = C.TranslateStatementBlock(context.statementBlock()),
            };
            try
            {
                stat.Condition = args.GetOrDefault(0)?.ToNative();
                stat.CommentOnEnd = context.commentOnEnd?.Text;
            }
            catch (Exception)
            {
                var sb = new StringBuilder();
                sb.Append("循环判断尾 ");
                sb.Append(TokenUtils.GetRawText(context.argumentsOnEnd));
                if (context.commentOnEnd != null)
                {
                    sb.Append("  ' ");
                    sb.Append(context.commentOnEnd.Text);
                }
                stat.UnexaminedCode = sb.ToString();
            }
            return stat;
        }

        public override Statement VisitEmptyStatement([NotNull] EplParser.EmptyStatementContext context)
        {
            return new ExpressionStatement()
            {
                Expression = null,
                Comment = context.Comment()?.GetText()
            };
        }

        public override Statement VisitExpressionStatement([NotNull] EplParser.ExpressionStatementContext context)
        {
            var expr = context.expression().Accept(C.ExprTrans).ToNative();
            return new ExpressionStatement()
            {
                Expression = (CallExpression)expr,
                Comment = context.Comment()?.GetText()
            };
        }

        public override Statement VisitForStatement([NotNull] EplParser.ForStatementContext context)
        {
            var stat = new ForStatement()
            {
                CommentOnEnd = context.commentOnEnd?.Text,
                Block = C.TranslateStatementBlock(context.statementBlock())
            };
            try
            {
                var args = (TecArgListExpression)context.argumentsOnStart.Accept(C.ExprTrans);
                stat.Start = args.GetOrDefault(0)?.ToNative();
                stat.End = args.GetOrDefault(1)?.ToNative();
                stat.Step = args.GetOrDefault(2)?.ToNative();
                stat.Var = args.GetOrDefault(3)?.ToNative();
                stat.CommentOnStart = context.commentOnStart?.Text;
            }
            catch (Exception)
            {
                var sb = new StringBuilder();
                sb.Append("变量循环首 ");
                sb.Append(TokenUtils.GetRawText(context.argumentsOnStart));
                if (context.commentOnStart != null)
                {
                    sb.Append("  ' ");
                    sb.Append(context.commentOnStart.Text);
                }
                stat.UnexaminedCode = sb.ToString();
            }
            return stat;
        }

        public override Statement VisitIfStatement([NotNull] EplParser.IfStatementContext context)
        {
            var stat = new IfElseStatement()
            {
                BlockOnTrue = C.TranslateStatementBlock(context.onTrue),
                BlockOnFalse = C.TranslateStatementBlock(context.onFalse)
            };
            try
            {
                var args = (TecArgListExpression)context.arguments().Accept(C.ExprTrans);
                stat.Condition = args.GetOrDefault(0)?.ToNative();
                stat.Comment = context.comment?.Text;
            }
            catch (Exception)
            {
                var sb = new StringBuilder();
                sb.Append("如果 ");
                sb.Append(TokenUtils.GetRawText(context.arguments()));
                if (context.comment != null)
                {
                    sb.Append("  ' ");
                    sb.Append(context.comment.Text);
                }
                stat.UnexaminedCode = sb.ToString();
            }
            return stat;
        }

        public override Statement VisitIfTrueStatement([NotNull] EplParser.IfTrueStatementContext context)
        {
            var stat = new IfStatement()
            {
                Block = C.TranslateStatementBlock(context.onTrue),
            };
            try
            {
                var args = (TecArgListExpression)context.arguments().Accept(C.ExprTrans);
                stat.Condition = args.GetOrDefault(0)?.ToNative();
                stat.Comment = context.comment?.Text;
            }
            catch (Exception)
            {
                var sb = new StringBuilder();
                sb.Append("如果真 ");
                sb.Append(TokenUtils.GetRawText(context.arguments()));
                if (context.comment != null)
                {
                    sb.Append("  ' ");
                    sb.Append(context.comment.Text);
                }
                stat.UnexaminedCode = sb.ToString();
            }
            return stat;
        }

        public override Statement VisitWhenStatement([NotNull] EplParser.WhenStatementContext context)
        {
            var stat = new SwitchStatement();
            foreach (var item in context.whenIf())
            {
                var caseInfo = new SwitchStatement.CaseInfo()
                {
                    Block = C.TranslateStatementBlock(item.statementBlock())
                };
                try
                {
                    var args = (TecArgListExpression)item.arguments().Accept(C.ExprTrans);
                    caseInfo.Condition = args.GetOrDefault(0)?.ToNative();
                    caseInfo.Comment = item.comment?.Text;
                }
                catch (Exception)
                {
                    var sb = new StringBuilder();
                    sb.Append("判断 ");
                    sb.Append(TokenUtils.GetRawText(item.arguments()));
                    if (item.comment != null)
                    {
                        sb.Append("  ' ");
                        sb.Append(item.comment.Text);
                    }
                    caseInfo.UnexaminedCode = sb.ToString();
                }
                stat.Case.Add(caseInfo);
            }
            stat.DefaultBlock = C.TranslateStatementBlock(context.whenElse()?.statementBlock());
            return stat;
        }

        public override Statement VisitWhileStatement([NotNull] EplParser.WhileStatementContext context)
        {
            var stat = new WhileStatement()
            {
                CommentOnEnd = context.commentOnEnd?.Text,
                Block = C.TranslateStatementBlock(context.statementBlock())
            };
            try
            {

                var args = (TecArgListExpression)context.argumentsOnStart.Accept(C.ExprTrans);
                stat.Condition = args.GetOrDefault(0)?.ToNative();
                stat.CommentOnStart = context.commentOnStart?.Text;
            }
            catch (Exception)
            {
                var sb = new StringBuilder();
                sb.Append("判断循环首 ");
                sb.Append(TokenUtils.GetRawText(context.argumentsOnStart));
                if (context.commentOnStart != null)
                {
                    sb.Append("  ' ");
                    sb.Append(context.commentOnStart.Text);
                }
                stat.UnexaminedCode = sb.ToString();
            }
            return stat;
        }

        public override Statement VisitInvaildStatement([NotNull] EplParser.InvaildStatementContext context)
        {
            throw new Exception("InvaildStatement");
        }
    }
}
