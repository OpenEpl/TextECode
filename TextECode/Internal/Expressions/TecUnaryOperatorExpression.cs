using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecUnaryOperatorExpression : TecExpression
    {
        private struct UnaryOperatorInfo
        {
            public int CmdId;

            public UnaryOperatorInfo(int cmdId)
            {
                CmdId = cmdId;
            }
        }
        private static readonly Dictionary<int, UnaryOperatorInfo> infoMap = new(){
            { EplParser.Minus, new UnaryOperatorInfo(21) }
        };

        private readonly int operatorType;
        private readonly TecExpression operandExpr;

        public TecUnaryOperatorExpression(CodeTranslatorContext c, int operatorType, TecExpression operandExpr) : base(c)
        {
            this.operatorType = operatorType;
            this.operandExpr = operandExpr;
        }

        public override BaseDataTypeElem ResultDataType => operandExpr.ResultDataType;

        public override Expression ToNative()
        {
            var info = infoMap[operatorType];
            return new CallExpression(0, info.CmdId, new ParamListExpression() {
                operandExpr.ToNative()
            });
        }
    }
}
