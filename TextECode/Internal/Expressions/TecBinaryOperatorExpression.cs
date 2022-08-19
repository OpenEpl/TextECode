using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecBinaryOperatorExpression : TecExpression
    {
        private struct BinaryOperatorInfo
        {
            public int CmdId;
            public bool Reducible;
            public bool EqLike;
            public BinaryOperatorInfo(int cmdId, bool reducible, bool eqLike)
            {
                CmdId = cmdId;
                Reducible = reducible;
                EqLike = eqLike;
            }
            public BinaryOperatorInfo(int cmdId, bool reducible): this(cmdId, reducible, false)
            {
            }
        }
        private static readonly Dictionary<int, BinaryOperatorInfo> infoMap = new(){
            { EplParser.Mul, new BinaryOperatorInfo(15, true) },
            { EplParser.Div, new BinaryOperatorInfo(16, true) },
            { EplParser.IDiv, new BinaryOperatorInfo(17, true) },
            { EplParser.Mod, new BinaryOperatorInfo(18, true) },
            { EplParser.Plus, new BinaryOperatorInfo(19, true) },
            { EplParser.Minus, new BinaryOperatorInfo(20, true) },
            { EplParser.Equal, new BinaryOperatorInfo(38, false, true) },
            { EplParser.NotEqual, new BinaryOperatorInfo(39, false, true) },
            { EplParser.Less, new BinaryOperatorInfo(40, false, true) },
            { EplParser.Greater, new BinaryOperatorInfo(41, false, true) },
            { EplParser.LessOrEqual, new BinaryOperatorInfo(42, false, true) },
            { EplParser.GreaterOrEqual, new BinaryOperatorInfo(43, false, true) },
            { EplParser.ApproximatelyEqual, new BinaryOperatorInfo(44, false, true) },
            { EplParser.And, new BinaryOperatorInfo(45, true) },
            { EplParser.Or, new BinaryOperatorInfo(46, true) },
            { EplParser.Assign, new BinaryOperatorInfo(52, false) },
        };

        private readonly int operatorType;
        private readonly TecExpression lhsExpr;
        private readonly TecExpression rhsExpr;

        public TecBinaryOperatorExpression(CodeTranslatorContext c, int operatorType, TecExpression lhsExpr, TecExpression rhsExpr) : base(c)
        {
            this.operatorType = operatorType;
            this.lhsExpr = lhsExpr;
            this.rhsExpr = rhsExpr;
        }

        public override BaseDataTypeElem ResultDataType
        {
            get
            {
                if (infoMap[operatorType].EqLike)
                {
                    return P.SystemDataTypes.Bool;
                }
                return lhsExpr.ResultDataType;
            }
        }

        public void AddToParamList(ParamListExpression paramList)
        {
            paramList.Add(lhsExpr.ToNative());
            paramList.Add(rhsExpr.ToNative());
        }

        public override Expression ToNative()
        {
            var info = infoMap[operatorType];
            var paramList = new ParamListExpression();
            if (info.Reducible 
                && lhsExpr is TecBinaryOperatorExpression binaryExpr1 
                && binaryExpr1.operatorType == operatorType)
            {
                binaryExpr1.AddToParamList(paramList);
            }
            else
            {
                paramList.Add(lhsExpr.ToNative());
            }

            if (info.Reducible
                && rhsExpr is TecBinaryOperatorExpression binaryExpr2
                && binaryExpr2.operatorType == operatorType)
            {
                binaryExpr2.AddToParamList(paramList);
            }
            else
            {
                paramList.Add(rhsExpr.ToNative());
            }
            return new CallExpression(0, info.CmdId, paramList);
        }
    }
}
